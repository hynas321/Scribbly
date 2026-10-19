#!/usr/bin/env bash
set -euo pipefail

if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
  SQLCMD=/opt/mssql-tools18/bin/sqlcmd
elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
  SQLCMD=/opt/mssql-tools/bin/sqlcmd
else
  echo "sqlcmd was not found in the init container" >&2
  exit 1
fi

COMMON=(-S mssql -U sa -C -b)
export SQLCMDPASSWORD

echo "Waiting for SQL Server authentication..."
ready=0
for _ in $(seq 1 60); do
  if "$SQLCMD" "${COMMON[@]}" -Q "SELECT 1" >/dev/null 2>&1; then
    ready=1
    break
  fi
  sleep 2
done
if [[ "$ready" -ne 1 ]]; then
  echo "SQL Server did not become ready for authenticated connections." >&2
  exit 1
fi

echo "Initializing Scribbly SQL database..."
"$SQLCMD" "${COMMON[@]}" -i /docker/init.sql
for migration in /docker/migrations/*.sql; do
  [[ -e "$migration" ]] || continue
  echo "Applying $(basename "$migration")..."
  "$SQLCMD" "${COMMON[@]}" -d MyDatabase -i "$migration"
done
echo "Scribbly SQL database ready."
