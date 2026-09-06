include .env
export

scaffold:
	docker compose up -d --wait db

	dotnet ef dbcontext scaffold "$$Db__ConnString" Npgsql.EntityFrameworkCore.PostgreSQL \
		--context AppDbContext \
		--output-dir Models \
		--context-dir Data \
		--force
