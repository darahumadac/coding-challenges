# Notes

# Makefile and build script
- `.PHONY` Prevent Conflicts with Files: If a file with the same name as a target exists in the directory, make might mistakenly treat the file as up-to-date and skip executing the target. Declaring the target as .PHONY ensures make always executes it.
- Makefile has **targets**, like build, clean, deploy, etc.
  - Default target is *all*
  - To define targets to run by default, specify it in the *all* target, like: `all: clean build test deploy`
- starting a recipe with `@` suppresses echoing. Normally, `make` prints each line of the recipe before it is executed.
- I encountered issues running bash commands in the Makefile.
  - **Issue**: Makefile has issues running bash commands
  - **Resolution**: To run bash commands, just write a bash script and call it from the Makefile target like `@bash ./myscript.sh`. 
    - It may also help to set the SHELL variable of the Makefile to `SHELL=/bin/bash`, but in my case, it did not help.
- When using `-exec` option in `find`, make sure to add space between `-exec *command* {}` and `\;` . For example, `-exec rm -rf {} \;` instead of `-exec rm -rf {}\;`  
- can run multiple bash commands using `find` by using `find . -exec bash "*command_1 {}*; command_2 {}" \;`
- make sure to wrap folders or filenames with quotes `"` when using them in bash script because bash splits spaces by default