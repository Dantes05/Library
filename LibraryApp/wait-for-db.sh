#!/bin/bash
set -e

host="db"
port="1433"
timeout=60

echo "Waiting for database to be ready..."
until nc -z $host $port; do
  timeout=$((timeout - 1))
  if [ $timeout -eq 0 ]; then
    echo "Database is not ready, exiting."
    exit 1
  fi
  sleep 1
done

echo "Database is ready!"