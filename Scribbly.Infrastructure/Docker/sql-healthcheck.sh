#!/usr/bin/env bash
set -euo pipefail

export SQLCMDPASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"

if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -Q "SELECT 1" -b >/dev/null
elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -C -Q "SELECT 1" -b >/dev/null
else
  timeout 3 bash -c '</dev/tcp/127.0.0.1/1433'
fi
