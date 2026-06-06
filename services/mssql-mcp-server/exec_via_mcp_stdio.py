"""
Ejecuta la herramienta MCP `execute_sql` contra el servidor stdio MsGestion
(misma ruta que CallMcpTool en Cursor).

Uso:
  python exec_via_mcp_stdio.py <ruta.sql | ruta-args.json>

Si el archivo es JSON debe tener { "query": "..." }.
La configuración se lee desde ~/.cursor/mcp.json bloque `mssql-mcp-msgestion`
(o fallback `user-mssql-mcp-msgestion`).
"""
from __future__ import annotations

import json
import os
import pathlib
import sys

import anyio
from mcp.client.session import ClientSession
from mcp.client.stdio import StdioServerParameters, get_default_environment, stdio_client


def load_mcp_block() -> dict[str, object]:
    cfg = pathlib.Path.home() / ".cursor" / "mcp.json"
    if not cfg.is_file():
        raise SystemExit(f"No se encontró {cfg}")
    data = json.loads(cfg.read_text(encoding="utf-8"))
    servers = data.get("mcpServers") or {}
    srv = servers.get("mssql-mcp-msgestion") or servers.get("user-mssql-mcp-msgestion")
    if not srv:
        raise SystemExit("mcp.json sin mssql-mcp-msgestion ni user-mssql-mcp-msgestion")

    cmd = pathlib.Path(os.path.expandvars(str(srv["command"]))).expanduser()
    raw_args = srv.get("args", [])
    cwd = pathlib.Path(os.path.expandvars(str(srv.get("cwd", "")))).expanduser()
    env_blk = srv.get("env") or {}

    args = [os.path.expandvars(str(a)) for a in raw_args]
    cwd_str = str(cwd) if str(cwd) else None
    env = {str(k): os.path.expandvars(str(v)) for k, v in env_blk.items()}
    return {"command": str(cmd), "args": args, "cwd": cwd_str, "env": env}


def load_query(arg_path: pathlib.Path) -> str:
    text = arg_path.read_text(encoding="utf-8")
    if arg_path.suffix.lower() == ".json":
        obj = json.loads(text)
        q = obj.get("query")
        if not isinstance(q, str) or not q.strip():
            raise SystemExit('El JSON debe contener una clave string no vacía "query".')
        return q
    return text


async def run_tool(query: str) -> None:
    blk = load_mcp_block()
    env_inherit = get_default_environment() | (blk["env"] if isinstance(blk["env"], dict) else {})

    cwd = blk.get("cwd")

    params = StdioServerParameters(
        command=str(blk["command"]),
        args=list(blk["args"]) if isinstance(blk["args"], list) else [],
        cwd=cwd if isinstance(cwd, str) else None,
        env=env_inherit,
    )
    async with stdio_client(params) as (read_stream, write_stream):
        async with ClientSession(read_stream, write_stream) as session:
            await session.initialize()
            result = await session.call_tool("execute_sql", {"query": query})
            if result.isError:
                sys.stderr.write("La herramienta execute_sql devolvió isError=true\n")
                sys.exit(2)
            for block in result.content:
                txt = getattr(block, "text", None)
                if txt:
                    print(txt)


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("Uso: python exec_via_mcp_stdio.py <ruta.sql|.json>")
    path = pathlib.Path(sys.argv[1]).resolve()
    query = load_query(path)
    anyio.run(run_tool, query)


if __name__ == "__main__":
    main()
