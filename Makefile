include .env
export

scaffold:
	docker compose up -d --wait db

	dotnet ef dbcontext scaffold "$$Db__ConnString" Npgsql.EntityFrameworkCore.PostgreSQL \
		--context AppDbContext \
		--output-dir Infrastructure \
		--context-dir Infrastructure \
		--force
