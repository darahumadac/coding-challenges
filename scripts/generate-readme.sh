#!/bin/bash

ROOT_DIR="$(dirname $(dirname $(realpath "$0")))" 
README_FILE="${ROOT_DIR}/README.md"

> ${README_FILE}

echo -e "# Coding Challenges\nMy solutions to https://codingchallenges.fyi/\n" >> ${README_FILE}
echo -e "## Challenges" >> ${README_FILE}

find "${ROOT_DIR}" -mindepth 2 -maxdepth 2 -type f -name README.md \
    | while read readme_file; do
        PROJECT_NAME=$(cat ${readme_file} | sed -n '2p')
        PROJECT_DESC=$(cat ${readme_file} | sed -n '3p')
        CONTENTS=$(cat ${readme_file} | sed -n '4,8p' | xargs -I {} echo "\t{}")
        printf "1. ${PROJECT_NAME} — ${PROJECT_DESC}\n${CONTENTS}\n" >> ${README_FILE};
    done