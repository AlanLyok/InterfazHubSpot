"""Ejecuta scriptsSQL/*.sql en orden contra MSGestion (lee .env local)."""
from __future__ import annotations

import pathlib
import re
import sys

import pyodbc
from dotenv import load_dotenv

ROOT = pathlib.Path(__file__).resolve().parents[2]
SCRIPTS = ROOT / "scriptsSQL"
ORDER = [
    "000_Cleanup_Legacy.sql",
    "001_ProcesosSpertaHubSpot.sql",
    "002_ProcesosSpertaHubSpotLog.sql",
    "003_USER_CALZETTA_POS_Clientes_Agregar.sql",
    "004_InterfazHubSpot_Cliente_Obtener.sql",
    "005_InterfazHubSpot_CuentaCorriente_Pagina.sql",
]


def connect():
    load_dotenv(pathlib.Path(__file__).parent / ".env")
    import os

    host = os.environ["MSSQL_HOST"]
    db = os.environ["MSSQL_DATABASE"]
    driver = os.environ["MSSQL_DRIVER"]
    tc = os.environ.get("Trusted_Connection", "no").lower() in ("yes", "true", "1")
    if tc:
        cs = (
            f"DRIVER={{{driver}}};SERVER={host};DATABASE={db};"
            "Trusted_Connection=yes;TrustServerCertificate=yes"
        )
    else:
        cs = (
            f"DRIVER={{{driver}}};SERVER={host};DATABASE={db};"
            f"UID={os.environ['MSSQL_USER']};PWD={os.environ['MSSQL_PASSWORD']};"
            "TrustServerCertificate=yes"
        )
    return pyodbc.connect(cs, autocommit=True)


def split_batches(sql: str) -> list[str]:
    parts = re.split(r"^\s*GO\s*$", sql, flags=re.MULTILINE | re.IGNORECASE)
    return [p.strip() for p in parts if p.strip()]


def main() -> int:
    cn = connect()
    cur = cn.cursor()
    for name in ORDER:
        path = SCRIPTS / name
        if not path.exists():
            print(f"MISSING: {path}", file=sys.stderr)
            return 1
        print(f"=== {name} ===")
        text = path.read_text(encoding="utf-8")
        for i, batch in enumerate(split_batches(text), 1):
            try:
                cur.execute(batch)
                while True:
                    try:
                        rows = cur.fetchall()
                        if rows:
                            for row in rows:
                                print(row)
                    except pyodbc.ProgrammingError:
                        break
                    if not cur.nextset():
                        break
            except pyodbc.Error as exc:
                print(f"ERROR in {name} batch {i}: {exc}", file=sys.stderr)
                return 1
        print(f"OK: {name}")
    cn.close()
    print("Deploy scriptsSQL completado.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
