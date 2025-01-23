# Building the solution
- run `make` to clean, build, test, and publish the application
- default configuration `CONFIG` is set to `Debug`. If for Release configuration, run make as `make CONFIG=Release`
  - targets:
    - `clean` - cleans the solution and removes publish folders older than 2 days
    - `build` - builds the application based on configuration `CONFIG`
    - `test` - runs unit tests
    - `publish` - publishes the application based on  configuration `CONFIG`