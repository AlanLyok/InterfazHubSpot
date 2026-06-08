"""Verifica esquema greenfield HubSpot en MSGestion."""
from __future__ import annotations

import pathlib

import pyodbc
from dotenv import load_dotenv


def main() -> None:
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
    cn = pyodbc.connect(cs)
    cur = cn.cursor()
    print("DB:", db)
    for t in ["ProcesosSpertaHubSpot", "ProcesosSpertaHubSpotLog", "IntegracionEjecucionLog"]:
        cur.execute("SELECT CASE WHEN OBJECT_ID(?) IS NOT NULL THEN 1 ELSE 0 END", f"dbo.{t}")
        print(f"TABLE {t}:", "EXISTS" if cur.fetchone()[0] else "MISSING")
    for tbl in ["ProcesosSpertaHubSpot", "ProcesosSpertaHubSpotLog"]:
        cur.execute(
            "SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID(?)",
            f"dbo.{tbl}",
        )
        cols = [r[0] for r in cur.fetchall()]
        print(f"COLUMNS {tbl}:", ", ".join(cols))
    cur.execute("SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.USER_POS_Clientes_Agregar'))")
    defn = cur.fetchone()[0] or ""
    print("SP USER_POS_Clientes_Agregar EXISTS:", bool(defn))
    print("  GETDATE():", "GETDATE()" in defn)
    print("  FechaCreacion:", "FechaCreacion" in defn)
    print("  Identificador:", "Identificador" in defn)
    print("  FechaCreacionUtc:", "FechaCreacionUtc" in defn)
    cn.close()


if __name__ == "__main__":
    main()
