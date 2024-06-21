const express = require("express");
const app = express();
const fs = require("fs");
const { createPublicKey } = require("crypto");

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

app.get("/oauth/:provider", (req, res) => {
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
});

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
