
.PHONY: all clean build test publish restore

all: clean build test publish

restore:
	@echo -e "\n[I]Restoring dependencies ..."
	@dotnet restore ccwc.sln

clean:
	@echo -e "\n[I] Cleaning solution ..."
	@dotnet clean ccwc.sln --nologo
	@bash ./build.sh clean
	
build:
	@echo -e "\n[I] Building application ..."
	@dotnet build ccwc.sln --nologo

test:
	@echo -e "\n[I] Running unit tests ..."
	@bash ./build.sh test
	

publish:
	@echo -e "\n[I] Publishing application ..."
	@bash ./build.sh publish

