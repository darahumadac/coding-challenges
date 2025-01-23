#!/bin/bash

MODE="$1"

CURRENT_DATE=$(date +%m.%d.%Y)
CLEANUP_OLDER_THAN=+2
case ${MODE} in

    "clean")
        echo "[I] Cleaning publish folders modified ${CLEANUP_OLDER_THAN} days ..."
        # clean up publish directories older than 2 days
        find ./publish/ -mindepth 1 -maxdepth 1 -type d -mtime ${CLEANUP_OLDER_THAN} -exec bash -c "echo \"{}\"; rm -rf \"{}\"" \;
        ;;
    "test")
        dotnet test --logger "html;LogFileName=test-results-${CURRENT_DATE}.html" --no-build --nologo
        ;;
    "publish")
        dotnet publish ./ccwc/ccwc.csproj --output ./publish/${CURRENT_DATE} --nologo
        echo -e "\nPublish Directory    : ./publish/${CURRENT_DATE}"
        ;;
    *)
        echo "Unknown mode"
        ;;
esac
