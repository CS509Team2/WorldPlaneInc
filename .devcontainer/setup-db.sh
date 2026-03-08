#!/usr/bin/env bash
# setup-db.sh — Import flight data SQL dumps into the MySQL server.
# Usage: ./setup-db.sh [MYSQL_HOST] [MYSQL_PORT]
#
# Defaults connect to the 'db' service defined in docker-compose.yml.

set -euo pipefail

MYSQL_HOST="${1:-db}"
MYSQL_PORT="${2:-3306}"
MYSQL_USER="root"
MYSQL_PWD="rootpassword"
DATABASE="app"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "⏳ Waiting for MySQL at ${MYSQL_HOST}:${MYSQL_PORT}..."
for i in $(seq 1 30); do
  if mysqladmin ping -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" -p"$MYSQL_PWD" --silent 2>/dev/null; then
    echo "✅ MySQL is ready."
    break
  fi
  if [ "$i" -eq 30 ]; then
    echo "❌ Timed out waiting for MySQL." >&2
    exit 1
  fi
  sleep 2
done

# Create the database if it doesn't exist
echo "📦 Ensuring database '${DATABASE}' exists..."
mysql -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" -p"$MYSQL_PWD" \
  -e "CREATE DATABASE IF NOT EXISTS \`${DATABASE}\`;"

# Import each SQL file
for sql_file in "$PROJECT_ROOT"/flightdata_*.sql; do
  filename="$(basename "$sql_file")"
  echo "📥 Importing ${filename}..."
  mysql -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" -p"$MYSQL_PWD" \
    "$DATABASE" < "$sql_file"
  echo "   ✔ ${filename} imported."
done

if [ -f "$PROJECT_ROOT/create_user.sql" ]; then
  echo "📥 Importing create_user.sql..."
  mysql -h "$MYSQL_HOST" -P "$MYSQL_PORT" -u "$MYSQL_USER" -p"$MYSQL_PWD" \
    "$DATABASE" < "$PROJECT_ROOT/create_user.sql"
  echo "   ✔ create_user.sql imported."
fi

echo ""
echo "🎉 All SQL scripts imported into '${DATABASE}' successfully."
echo "   Connect with: mysql -h ${MYSQL_HOST} -P ${MYSQL_PORT} -u ${MYSQL_USER} -p${MYSQL_PWD} ${DATABASE}"
