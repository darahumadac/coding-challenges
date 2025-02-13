.PHONY: all clean build test

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

run: build
	@echo "[I] Starting up app ..."
	@echo "[I] Starting redis container ..."
	@docker start redis || { echo "Error running redis container"; exit 1; }
	@echo "[I] Running URL Shortener service ..."
	@dotnet run --project ./urlshortener.service/urlshortener.service.csproj --no-build &
	@echo "[I] Starting client ..."
	@(cd ./urlshortener.client && npm run dev)
