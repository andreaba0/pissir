/**
 * This code is in production on address https://appweb.andreabarchietto.it
 * Its purpose is to route requests directed to a oauth provider
 * It acts as a proxy between client frontend and oauth provider
 * Redirection from oauth provider are routed to a localhost address
 * Request are signed with hmacsha256 and an anti replay mechanism is in place
 * 
 * This method to sign request is not 100% reliable if used in a distributed environment
 * reason: multiple request can be sent at the same time, buy may arrive in different order
 * and this may cause errors in this scenario:
 * request 1 timestamp now     : ----------------------> arrives after request 2
 * request 2 timestamp now + 1 : --------------> arrives before request 1
 * server will store timestamp of request 2 and reject request 1
 * This solution is not suitable for a distributed environment
 */






const express = require("express");
const app = express();
const fs = require("fs");
const { createPublicKey } = require("crypto");
const { ProviderMapper } = require("./provider_mapper");
const { TokenMemoryStore } = require("./memory");
const base64url = require("base64url");
const crypto = require("crypto");
const cookieParser = require('cookie-parser');
require("dotenv").config();

app.use(cookieParser(process.env.COOKIE_SECRET));

const secret = process.env.SECRET;

const tokenMemoryStore = new TokenMemoryStore();
const allowedClients = new Set()
var index = 1;
while (process.env[`CLIENT_ID_${index}`]) {
  allowedClients.add(process.env[`CLIENT_ID_${index}`]);
  index++;
}
const allowedUsers = new Set()
index = 1;
while (process.env[`USER_ID_${index}`]) {
  allowedUsers.add(process.env[`USER_ID_${index}`]);
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


/**
 * This endpoint redirects user to the oauth provider uri with the query parameters received
 * hmacsha256 of client_id is used to authenticate requests and prevent unauthorized access
 * Only a limited list of client_id are allowed to use this service
 */
app.get("/oauth", (req, res) => {
  res.setHeader('Cache-Control', 'no-store');
  const query = req.query;
  const provider = req.query.provider;
  const providerMapper = new ProviderMapper(provider);
  const clientEndpoint = providerMapper.getUri();


  // list of custom parameters that are not part of the oauth standard
  const signature = query.signature;
  const client_uri = query.client_uri;
  const user_id = query.user_id;
  let request_timestamp = query.request_timestamp;
  delete query.signature;
  delete query.client_uri;
  delete query.provider;
  delete query.user_id;
  delete query.request_timestamp; // anti replay
  if (!signature || !client_uri || !user_id || !request_timestamp) {
    res.status(400).send("Missing parameters");
    return;
  }


  request_timestamp = parseInt(request_timestamp);
  if (isNaN(request_timestamp)) {
    res.status(400).send("Invalid timestamp");
    return;
  }

  const string_to_sign = `${query.client_id}${provider}${user_id}${request_timestamp}`;
  const hashed_client = crypto.createHmac('sha256', secret).update(string_to_sign).digest('base64');
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
  if(!allowedUsers.has(user_id)) {
    res.status(400).send("Invalid user_id");
    return;
  }
  const error = tokenMemoryStore.store(query.client_id, request_timestamp);
  if (error) {
    res.status(400).send(error);
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


/**
 * This endpoint is used to redirect the user back to the client_uri
 */
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





// This endpoint is used just to create a fake custom oauth provider
// Token generated manually must be signed with one of the private keys in the jwks
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
