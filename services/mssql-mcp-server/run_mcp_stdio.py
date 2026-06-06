"""
Arranque stdio del servidor oficial mssql-mcp-server (PyPI no expone `python -m mssql_mcp_server`).

Cursor debe pasar `MSSQL_*` y opciones ODBC en el bloque `env` del servidor MCP (.cursor/mcp.json).
"""
from __future__ import annotations

import asyncio
import sys

from mssql_mcp_server.server import main


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        sys.exit(130)
