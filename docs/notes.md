TODO : NEED TO TEST JWTSERVICE AND OTHERS

# What is a JWT?

JSON Web Token (JWT) is a standard for securely transmitting information between parties as a JSON
object, defined by [RFC 7519](https://datatracker.ietf.org/doc/html/rfc7519).

# Problem

You have some data that you want to transfer between 2 parties through an insecure channel. The data
may be tampered with so the receiver needs to somehow verify on their own that the data hasn't been
altered by someone in the middle.

# JWT structure

An example JWT token:

- eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30

The token itself is made of 3 parts (seperated by dots).

## Header

The first part is a JSON header expressed in Base64Url format:

```
{
  "alg": "HS256",
  "typ": "JWT"
}
```

`alg` is the algorithm used to hash the signature and `typ` is always JWT.

After converting this JSON string into bytes and then expressing those bytes as a Base64Url we get:

- eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9

Note that Base64Url is not encrypting, but encoding. This means that anyone that has the token can
just decode it and read the header. You can encrypt JWT but it's not necessary for authorization
which is the most common usecase.

## Payload

The second part is a JSON payload expressed in Base64Url format:

```
{
  "sub": "1234567890",
  "name": "John Doe",
  "admin": true
}
```

This json can be whatever you want, it's the message you want to transfer.

After converting this JSON string into bytes then expressing those bytes as a Base64Url we get:

- eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0

This payload can also be decoded and read by anyone that has the token just like the header.

## Signature

The third part is the string that results from:

```
HMACSHA256(
  base64UrlEncode(headerJSON) + "." +
  base64UrlEncode(payloadJSON),
  secret)
```

Where `secret` is a secret string that the issuer decides on. Anyone that knows the value of
`secret` can create a token themselves and sign it and the issuer would treat it as trusted.

# Most common usecase

The most common usecase of JWT signed tokens is authorization.

HTTP is stateless, meaning that when a client sends a request to the server, the server doesn't know
anything about the client besides their public ip and what's included in the request. This means
that in order for the server to know what resources the client should have access to, the client
needs to send that info with their requests. It sounds kind of crazy to let the client do that, but
signed JWT tokens make this possible since the server can just issue a signed JWT that contains
user's role/permissions in the payload, client stores the JWT in their local storage/cookies and
sends it back as a header on subsequent requests. If the client ever tries to tamper with the token,
they won't be able to sign it with a verified signature because only server knows the secret. This
way backend can take the JWT from request header, verify the signature, if it's valid then the
backend can be sure that the payload wasn't tampered with and thus it can read the permissions that
it wrote there when the token was issued. So in this case JWT is basically a way to make stateless
TTP have a per client state by using the client to send you back the info you gave them earlier.

# Validating vs Verifying tokens

# Final note

# Sources

- https://www.jwt.io/ is a pretty cool website to play around and get a better practical
  understanding of what a JWT actually is.
- https://www.jwt.io/introduction#what-is-json-web-token
- https://datatracker.ietf.org/doc/html/rfc7519
