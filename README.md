# JwtAuthImplementation

this is a jwt auth implementation written without the microsoft identity / jwtbearer stuff. tokens
are signed and verified by hand (see Auth/JwtHandling/JwtHandler.cs), passwords are hashed with
bcrypt after a sha256 + base64 pass since bcrypt ignores anything past 72 bytes, and refresh tokens
are stored in postgres as sha256 hashes.

## structure

the api is split by feature, a feature folder owns everything it needs. Auth/ has the controller,
the service, its dtos and validators and the jwt handling, WeatherApi/ is just the demo endpoint and
its model.

the database side is separate because its generated. Entities/ is one class per table and Data/ has
the appdbcontext, both come out of `make scaffold` so dont hand edit them or move them somewhere
else, they get overwritten. Shared/ is for stuff that belongs to no feature and isnt persisted,
right now thats only Result.

## running it

make a .env file, the example has working values so you can just copy it:

```
cp .env.example .env
```

then:

```
docker compose up --build
```

then open:

```
http://localhost:5216/scalar/
```

## the happy path

### 1. register

POST /api/Auth/register

```json
{
  "username": "test1345",
  "password": "user1234"
}
```

gives back 201 and the created user:

```json
{ "id": 1, "username": "test1345", "createdAt": "2026-09-24T20:10:11.123456+00:00" }
```

### 2. login

POST /api/Auth/login with the same body. gives back 200 and both tokens:

```json
{
  "accessJwt": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOjEs...",
  "refreshToken": "kR8xW1fZ...base64url..."
}
```

copy both of these somewhere, you need them for the next steps. the access jwt lives for 10 minutes
and the refresh token for 30 days (configured in appsettings.json under Auth).

if you want to see what is inside the jwt, paste it into jwt.io. the payload has sub (user id), exp,
iat and iss, which is always JwtAuthImplementation

### 3. call a protected endpoint

GET /WeatherForecast is a test endpoint marked with [RequireAuth], so try accessing it first without
a token - it should respond with a 401.

then send it again with the `accessJwt` from the last step. in scalar, open the request, add an
`Authorization` header in the headers section with `Bearer <accessJwt>` as the value and send it
again, this time you get a 200 and the forecast.

### 4. refresh the session

the access jwt expires after 10 minutes and there is no way to renew it directly, you trade the
refresh token for a new pair instead.

POST /api/Auth/refreshToken

```json
{
  "refreshToken": "<refreshToken from step 2>"
}
```

200 with a fresh pair:

```json
{ "accessJwt": "eyJhbGciOi...", "refreshToken": "9pQ2...new one..." }
```

### 5. logout

POST /api/Auth/logout, no body, but it needs a valid access jwt since it is behind [RequireAuth].

204 and the user's refresh token row is deleted. so hitting /api/Auth/refreshToken again with the
token from step 4 responds with a 404.

note that the access jwt itself keeps working until it expires, since nothing about it is stored
server side and this implementation doesnt use a blocklist, hence they're shortlived - thats the
tradeoff.

## unit tests

TODO: need to add more tests, especially for authservice and integration tests for happy path at
least

unit tests for the token handler:

```
dotnet test
```

## updating database schema

this project treats the database as the source of truth, so the appdbcontext is scaffolded from it.

make sure the db volume is clean:

```
docker compose down -v
```

then just do:

```
make scaffold
```

## known issues

things i found while testing that arent fixed yet are written down in [bugs.md](bugs.md).
