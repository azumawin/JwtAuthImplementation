create a .env file by the example of env.example

run the project by doing:

```
docker compose up --build
```

then open http://localhost:5216/scalar/ and u can play around with the endpoints

# updating database schema

this project treats the database as the source of truth, so the appdbcontext is scaffolded from it.

make sure the db volume is clean:

```
docker compose down -v
```

then just do:

```
make scaffold
```
