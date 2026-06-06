"""Prueba local de conexion ODBC para mssql-mcp-server (usa .env en esta carpeta).

No imprime usuario ni contrasena.
Ejemplo: python test-connection.py
"""
from pathlib import Path
import os
import sys

from dotenv import load_dotenv
from pyodbc import connect

load_dotenv(Path(__file__).resolve().parent / ".env")


def main() -> int:
    required = ("MSSQL_USER", "MSSQL_PASSWORD", "MSSQL_DATABASE")
    for k in required:
        if not os.getenv(k):
            print("FALTA en .env:", k)
            return 2

    driver = os.getenv("MSSQL_DRIVER", "SQL Server")
    server = os.getenv("MSSQL_HOST", "localhost")
    user = os.getenv("MSSQL_USER")
    password = os.getenv("MSSQL_PASSWORD")
    database = os.getenv("MSSQL_DATABASE")
    tsc = os.getenv("TrustServerCertificate", "yes")
    tc = os.getenv("Trusted_Connection", "no")

    # Igual que mssql_mcp_server/server.py:get_db_config
    cs = (
        f"Driver={driver};Server={server};UID={user};PWD={password};"
        f"Database={database};TrustServerCertificate={tsc};Trusted_Connection={tc};"
    )
    try:
        with connect(cs, timeout=15) as conn:
            cur = conn.cursor()
            cur.execute("SELECT 1 AS ok, DB_NAME() AS dbname")
            _, dbname = cur.fetchone()
            cur.execute(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
            )
            (ntables,) = cur.fetchone()
        print("OK: conexion SQL Server establecida.")
        print("    Base efectiva:", dbname)
        print(
            "    Tablas (INFORMATION_SCHEMA, BASE TABLE aprox.):",
            ntables,
        )
        return 0
    except Exception as e:
        print("ERROR:", e)
        return 1


if __name__ == "__main__":
    sys.exit(main())
