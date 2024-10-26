const express = require("express");
const app = express();
const fs = require("fs");
const { createPublicKey } = require("crypto");
const { ProviderMapper } = require("./provider_mapper");
const base64url = require("base64url");
const crypto = require("crypto");
const cookieParser = require('cookie-parser');
require("dotenv").config();

app.use(cookieParser(process.env.COOKIE_SECRET));

const secret = process.env.SECRET;

const allowedClients = new Set()
var index = 1;
while (process.env[`CLIENT_ID_${index}`]) {
  allowedClients.add(process.env[`CLIENT_ID_${index}`]);
  index++;
}

const privateKeyFileName = ["private1.pem", "private2.pem", "private3.pem"];

var keys = [];

privateKeyFileName.forEach((fileName) => {
  //read file from local folder and append to keys in jwks format
  const key = fs.readFileSync(`./${fileName}`);
  var privateKey = createPublicKey(key);
  var jwkFormat = privateKey.export({ format: "jwk", type: "pkcs1" });
  keys.push({
    alg: "RS256",
    kty: "RSA",
    use: "sig",
    n: jwkFormat.n,
    e: jwkFormat.e,
    kid: fileName,
  });
});

app.get("/oauth", (req, res) => {
  res.setHeader('Cache-Control', 'no-store');
  const query = req.query;
  const provider = req.query.provider;
  const providerMapper = new ProviderMapper(provider);
  const clientEndpoint = providerMapper.getUri();
  const signature = query.signature;
  const client_uri = query.client_uri;
  delete query.signature;
  delete query.client_uri;
  delete query.provider;
  const key_id = crypto.randomBytes(8).toString('hex');

  const hashed_client = crypto.createHmac('sha256', secret).update(query.client_id).digest('base64');
  const cookie_exp = (new Date()).getTime() + 30*1000;
  const cookie_expires = new Date(cookie_exp);
  const cookieObj = {
    client_uri: client_uri,
    provider: provider,
    expires_at: cookie_exp
  }
  if (base64url.fromBase64(hashed_client) !== signature) {
    res.status(400).send("Invalid signature");
    return;
  }
  if(!allowedClients.has(query.client_id)) {
    res.status(400).send("Invalid client_id");
    return;
  }
  res.cookie("paper", JSON.stringify(cookieObj), {
    expires: cookie_expires,
    //httpOnly: true,
    secure: true,
    sameSite: "lax",
    signed: true,
    path: "/localhost_redirect/back"
  });
  const url_params = new URLSearchParams(query);
  const url = `${clientEndpoint}?${url_params}`;
  console.log(url);
  res.redirect(301, url);
})

function parseObject(obj) {
  if (typeof obj === "object") {
    return obj;
  }
  try {
    return JSON.parse(obj);
  } catch (e) {
    return null;
  }
}

app.get("/back", (req, res) => {
  const cookie = req.signedCookies.paper;
  if (!cookie) {
    res.status(400).send("No cookie found");
    return;
  }
  const parsedObj = parseObject(cookie);
  if (!parsedObj) {
    res.status(400).send("Invalid cookie");
    return;
  }
  const { client_uri, provider, expires_at } = parsedObj;
  req.query["provider"] = provider;
  const now = (new Date()).getTime();
  if (expires_at < now) {
    res.status(400).send("Cookie expired");
    return;
  }
  res.redirect(301, `${client_uri}?${new URLSearchParams(req.query)}`);
})

/*app.get("/oauth/:provider", (req, res) => {
  const query = req.query;
  console.log(query);
  console.log(req.url);
  var newQuery = {
    ...query,
    provider: req.params.provider,
  }
  res.redirect(
    301,
    `http://localhost/auth/signin?${new URLSearchParams(newQuery)}`
  );
});*/

/*app.get("/oauth/port_redirect/:port/:provider", (req, res) => {
  const query = req.query;
  console.log(query);
  console.log(req.url);
  var newQuery = {
    ...query,
    provider: req.params.provider,
  }
  res.redirect(
    301,
    `http://localhost:${req.params.port}/auth/signin?${new URLSearchParams(newQuery)}`
  );
});*/

app.get("/.well-known/openid-configuration", (req, res) => {
  res.json({
    issuer: "https://appweb.andreabarchietto.it",
    authorization_endpoint:
      "https://appweb.andreabarchietto.it/o/oauth2/v2/auth",
    device_authorization_endpoint:
      "https://appweb.andreabarchietto.it/device/code",
    token_endpoint: "https://appweb.andreabarchietto.it/token",
    userinfo_endpoint: "https://appweb.andreabarchietto.it/v1/userinfo",
    revocation_endpoint: "https://appweb.andreabarchietto.it/revoke",
    jwks_uri:
      "https://appweb.andreabarchietto.it/.well-known/oauth/openid/jwks/",
    response_types_supported: [
      "code",
      "token",
      "id_token",
      "code token",
      "code id_token",
      "token id_token",
      "code token id_token",
      "none",
    ],
    subject_types_supported: ["public"],
    id_token_signing_alg_values_supported: ["RS256"],
    scopes_supported: ["openid", "email", "profile"],
    token_endpoint_auth_methods_supported: [
      "client_secret_post",
      "client_secret_basic",
    ],
    claims_supported: [
      "aud",
      "email",
      "email_verified",
      "exp",
      "family_name",
      "given_name",
      "iat",
      "iss",
      "locale",
      "name",
      "picture",
      "sub",
    ],
    code_challenge_methods_supported: ["plain", "S256"],
    grant_types_supported: [
      "authorization_code",
      "refresh_token",
      "urn:ietf:params:oauth:grant-type:device_code",
      "urn:ietf:params:oauth:grant-type:jwt-bearer",
    ],
  });
});

app.get("/.well-known/oauth/openid/jwks/", (req, res) => {
    res.json({
        keys,
    });
});

app.listen(8000, () => {
  console.log("Server is running");
});
