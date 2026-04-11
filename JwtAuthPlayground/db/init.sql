CREATE TABLE users(
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE CHECK (btrim(username) <> ''),
    password_hash VARCHAR(300) NOT NULL CHECK (btrim(password_hash) <> ''),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
