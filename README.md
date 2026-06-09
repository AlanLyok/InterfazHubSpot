# InterfazHubSpot

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.5.2-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![HubSpot CRM](https://img.shields.io/badge/HubSpot-CRM%20v3-FF7A59?logo=hubspot)](https://developers.hubspot.com/docs/api/crm/understanding-the-crm)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-MSGestion-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Tests](https://img.shields.io/badge/tests-xUnit-2EA44F)](SolucionInterfazHubSpot/InterfazHubSpot.Tests.Unit/)

Integración **ERP Mastersoft → HubSpot CRM** (Calzetta): batch flujos **2A** (cola clientes) y **2B** (cuenta corriente) + consola MVC para desarrollo.

Datos vía **stored procedures** en MSGestion. HubSpot con **Private App Token**. Sin SpertaAPI en runtime.

---

## Inicio rápido

**[`docs/QUICKSTART.md`](docs/QUICKSTART.md)** — clone, config, build, verify.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Verify-InterfazHubSpot.ps1
```

---

## Documentación

**Índice maestro (Diátaxis, sin duplicar): [`docs/README.md`](docs/README.md)**

| Enlace directo | Contenido |
|----------------|-----------|
| [`docs/PRD_Integracion_HubSpot_2A_2B.md`](docs/PRD_Integracion_HubSpot_2A_2B.md) | Requisitos funcionales |
| [`docs/explanation/arquitectura.md`](docs/explanation/arquitectura.md) | Arquitectura |
| [`docs/explanation/dominio.md`](docs/explanation/dominio.md) | Modelo de dominio |
| [`docs/reference/configuracion.md`](docs/reference/configuracion.md) | Web.config / App.config |
| [`docs/reference/base-datos.md`](docs/reference/base-datos.md) | SQL, cola, SPs |
| [`docs/BatchProcess_Desarrollo_e_Implementacion.md`](docs/BatchProcess_Desarrollo_e_Implementacion.md) | Servicio Windows |
| [`docs/TESTING.md`](docs/TESTING.md) | Tests y cobertura ≥90% |
| [`docs/agents/INDEX.md`](docs/agents/INDEX.md) | Enrutamiento agentes AI |
| [`AGENTS.md`](AGENTS.md) | Guía agentes (comandos + reglas) |

---

## Flujos (resumen)

| Flujo | Job |
|-------|-----|
| **2A** | `ProcesarColaIntegracionesHubSpotJob` — cola → company + contactos |
| **2B** | `HubSpotSincronizarCuentaCorrienteJob` — cuenta corriente batch |

Diagrama y secuencia: [`docs/explanation/flujos-2a-2b.md`](docs/explanation/flujos-2a-2b.md).

---

## Stack

.NET Framework 4.5.2 · ASP.NET MVC · IScheduler batch · SQL Server MSGestion · HubSpot CRM v3 · xUnit.

Detalle: [`docs/explanation/arquitectura.md`](docs/explanation/arquitectura.md).

---

## Estructura repo

```
INTERFAZHUBSPOT/
├── SolucionInterfazHubSpot/   # Código .NET
├── scriptsPS1/                # Build, test, verify
├── docs/                      # Documentación indexada
├── implementacion/            # Servicio Windows
├── scriptsSQL/                # Deploy MSGestion
└── sql/                       # Copias versionadas SQL
```

---

## Licencia

Uso interno Calzetta / Mastersoft.
