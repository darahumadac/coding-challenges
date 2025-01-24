#!/bin/bash
 
README_FILE="README.md"
> ${README_FILE}

echo -e "# Coding Challenges\nMy solutions to https://codingchallenges.fyi/\n" >> ${README_FILE}
echo -e "## Challenges" >> ${README_FILE}

find . -mindepth 2 -type f -name README.md -exec bash -c "cat {}|sed -n '2,3p'" \; \
    | while read line; do read secondline;
        printf "1. ${line}\n\t${secondline}\n" >> ${README_FILE};
    done