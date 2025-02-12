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
	@echo "[I] Starting app ..."
	@echo "[I] Starting redis via docker and running URL Shortener service ..."
	@docker start redis >/dev/null && dotnet run --project ./urlshortener.service/urlshortener.service.csproj --no-build &
	@echo "[I] Starting client ..."
	@(cd ./urlshortener.client && npm run dev)
