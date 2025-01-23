#!/bin/bash

MODE="$1"
CONFIG="$2"

if [[ $CONFIG == "" ]]; then
    CONFIG="Debug"
fi

CURRENT_DATE=$(date +%m.%d.%Y)
CLEANUP_DAYS=+2
PUBLISH_ROOTDIR="./publish"
PUBLISH_LOCATION="${PUBLISH_ROOTDIR}/${CONFIG}/"
case ${MODE} in

    "clean")
        if [[ -d "${PUBLISH_LOCATION}" ]]; then
            # clean up publish directories
            echo "[I] Cleaning publish folders modified ${CLEANUP_DAYS} days ago ..."
            find "${PUBLISH_LOCATION}" -depth -type d -mtime ${CLEANUP_DAYS} -exec bash -c "echo \"{}\"; rm -rf \"{}\"" \;
            find "${PUBLISH_ROOTDIR}" -type d -empty -delete
        else
            echo "[I] No ${PUBLISH_LOCATION} folders to clean up"
        fi
        ;;
    "test")
        dotnet test --logger "html;LogFileName=TestResults-${CONFIG}_${CURRENT_DATE}.html" --no-build --nologo --configuration ${CONFIG}
        ;;
    "publish")
        PUBLISH_DIR="${PUBLISH_LOCATION}${CURRENT_DATE}"
        dotnet publish ./ccwc/ccwc.csproj --output "${PUBLISH_DIR}" --no-build --nologo --configuration ${CONFIG}
        echo -e "\nPublish Directory    : ${PUBLISH_DIR}"
        ;;
    *)
        echo "Unknown mode. Valid modes: 'clean', 'test', 'publish'"
        ;;
esac
