# Project Description
[**Build Your Own wc Tool**](/ccwc)
Build your own version of the Unix command line tool wc
- **Languages:** C#
- **Technology / Frameworks:** .NET Core
- **Libraries / Tools:** make, Git Hooks
- **Databases:** None used
- **Testing:** nUnit, Moq

## Design

## Building the solution
- run `make` to clean, build, test, and publish the application
- default configuration `CONFIG` is set to `Debug`. If for Release configuration, run make as `make CONFIG=Release`
  - targets:
    - `clean` - cleans the solution and removes publish folders older than 2 days
    - `build` - builds the application based on configuration `CONFIG`
    - `test` - runs unit tests
    - `publish` - publishes the application based on  configuration `CONFIG`

## Running the ccwc tool
- Usage: `ccwc.exe [OPTION]... [FILE]`
- Prints newline, word, and byte counts for a FILE
- By default, prints the counts in the following order: newline, word, byte count.
- Supports the following OPTIONs:
  - `-l`,   prints the newline counts
  - `-w`,   prints the word counts
  - `-c`,   prints the byte counts
- Sample usage
```c#
ccwc.exe test.txt
//Output: 7145 58164 342190 test.txt

ccwc.exe -w test.txt
//Ouptut: 58164 test.txt
```
