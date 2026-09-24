# known issues

stuff i found while testing the api by hand. writing it down so i don't forget, roughly in the order
i want to fix it.

## to fix

- usernames are case sensitive so "bob" and "Bob" can both register as separate accounts since the
  unique index is on the raw value.
- a duplicate registration logs a full stack trace

## smaller stuff

- the jwt signature is compared with a normal string comparison, should be
  CryptographicOperations.FixedTimeEquals on the bytes
- JwtPayload has an iss claim but VerifyJwt never checks it, so right now it does nothing
- UseHttpsRedirection does nothing in docker since only http is configured
- only one refresh token per user user_id is unique in refresh_tokens and new tokens are upserted,
  so logging in on a second device kills the first session, a simplification i decided to make
