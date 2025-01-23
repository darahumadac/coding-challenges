
.PHONY: all clean build test publish restore

SOLUTION_FILE = ccwc.sln
CONFIG ?= Debug

$(info [I] Current configuration:	$(CONFIG))
$(info [I] Solution File:		$(SOLUTION_FILE))

all: clean build test publish

restore:
	@echo -e "\n[I]Restoring dependencies ..."
	@dotnet restore $(SOLUTION_FILE)

clean:
	@echo -e "\n[I] Cleaning solution ..."
	@dotnet clean $(SOLUTION_FILE) --nologo --configuration $(CONFIG)
	@bash ./build.sh clean $(CONFIG)
	
build:
	@echo -e "\n[I] Building application ..."
	@dotnet build $(SOLUTION_FILE) --nologo --configuration $(CONFIG)

test: build
	@echo -e "\n[I] Running unit tests ..."
	@bash ./build.sh test $(CONFIG)

publish: test
	@echo -e "\n[I] Publishing application ..."
	@bash ./build.sh publish $(CONFIG)

