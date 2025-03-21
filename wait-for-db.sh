#!/bin/sh

set -e

host="db"
port=1433
timeout=60

echo "Waiting for database to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S "$host" -U "sa" -P "YourStrong!Passw0rd" -Q "SELECT 1" > /dev/null 2>&1; do
  timeout=$((timeout - 1))
  if [ $timeout -le 0 ]; then
    echo "Timeout waiting for database to be ready."
    exit 1
  fi
  sleep 1
done

echo "Database is ready. Starting application..."
exec "$@"