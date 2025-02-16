.PHONY: all clean build test dev package run-package deploy

all: clean build test

clean:
	@echo "[I] Cleaning app ..."
	@dotnet clean

build:
	@echo "[I] Building app ..."
	@dotnet build

test: build
	@echo "[I] Running tests ..."
	@dotnet test --no-build

dev: build
	@echo "[I] Starting up app locally ..."
	@echo "[I] Starting redis container ..."
	@docker start redis || { echo "Error running redis container"; exit 1; }
	@echo "[I] Running URL Shortener service ..."
	@dotnet run --project ./urlshortener.service/urlshortener.service.csproj --no-build &
	@echo "[I] Starting client ..."
	@(cd ./urlshortener.client && npm run dev)

package: 
	@echo "Packaging app ..."
	@echo "[I] Packaging frontend ..."
	@(cd ./urlshortener.client && npm run build)
	@echo "[I] Publishing urlshortener service ..."
	@dotnet publish ./urlshortener.service/urlshortener.service.csproj -c Release -o ./urlshortener.service/publish
	@echo "[I] Done packaging app code"

# run-package runs the packaged code using Development settings. Uses local sql server and redis 
run-package:
	@echo "[I] Running packaged app ..."
	@echo "[I] Starting redis container ..."
	@docker start redis -d
	@echo "[I] Executing db scripts locally ..."
	@sqlcmd -i ./urlshortener.service/Scripts/create_database_02142025.sql
	@echo "[I] Starting urlshortener service ..."
	@(export ASPNETCORE_ENVIRONMENT=Development && \
		export ASPNETCORE_URLS=http://localhost:5163 && \
		export SHORTLY_DOCKER_SQLDB_USER=$$DOCKER_SQLDB_USER && \
		export SHORTLY_DOCKER_SQLDB_PASSWORD=$$DOCKER_SQLDB_PASSWORD && \
		echo "Environment: $$ASPNETCORE_ENVIRONMENT" && \
		cd urlshortener.service/publish && dotnet urlshortener.service.dll &)
	@echo "[I] Starting ui ..."
	@http-server ./urlshortener.client/dist -p 5173

deploy:
	@echo "[I] Deploying app ..."
	@docker compose up --build -d
	@echo "[I] Deployment done"