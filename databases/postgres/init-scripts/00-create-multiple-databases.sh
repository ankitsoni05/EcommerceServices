#!/bin/bash
# =====================================================
# PostgreSQL Multi-Database Initialization Script
# =====================================================
# 
# This script creates multiple databases in a single PostgreSQL instance
# Useful for microservices architecture where each service has its own database
# 
# Usage: Place in /docker-entrypoint-initdb.d/ directory
# Databases are created from POSTGRES_MULTIPLE_DATABASES environment variable
# =====================================================

set -e
set -u

function create_database() {
	local database=$1
	echo "Creating database '$database'"
	psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
	    CREATE DATABASE "$database";
	    GRANT ALL PRIVILEGES ON DATABASE "$database" TO "$POSTGRES_USER";
EOSQL
}

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
	echo "Multiple database creation requested: $POSTGRES_MULTIPLE_DATABASES"
	for db in $(echo $POSTGRES_MULTIPLE_DATABASES | tr ',' ' '); do
		create_database $db
	done
	echo "Multiple databases created successfully!"
fi
