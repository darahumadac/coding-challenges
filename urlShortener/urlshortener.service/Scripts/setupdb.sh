#!/bin/bash

# Run the command
echo "Setting up db ..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "${MSSQL_SA_PASSWORD}" -l 60 -C \
    -i create_database_02142025.sql -o output.txt &

# Start SQL Server
/opt/mssql/bin/sqlservr



