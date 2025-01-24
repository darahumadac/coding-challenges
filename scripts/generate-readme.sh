#!/bin/bash

ROOT_DIR="$(dirname $(dirname $(realpath "$0")))" 
README_FILE="${ROOT_DIR}/README.md"

> ${README_FILE}

echo -e "# Coding Challenges\nMy solutions to https://codingchallenges.fyi/\n" >> ${README_FILE}
echo -e "## Challenges" >> ${README_FILE}

find "${ROOT_DIR}" -mindepth 2 -type f -name README.md -exec bash -c "cat {}|sed -n '2,3p'" \; \
    | while read project_name; do read project_desc;
        printf "1. ${project_name}\n\t${project_desc}\n" >> ${README_FILE};
    done