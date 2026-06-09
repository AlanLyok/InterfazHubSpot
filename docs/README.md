# Documentación InterfazHubSpot

Índice maestro del repositorio. Organizado con [Diátaxis](https://diataxis.fr/): cada documento tiene **un solo rol**; el detalle vive en un solo lugar y el resto **referencia**.

> **Agentes AI:** empezar en [`agents/INDEX.md`](agents/INDEX.md) — enrutamiento por tarea sin cargar docs innecesarios.

---

## Por intención (Diátaxis)

| Cuadrante | Para qué | Documentos |
|-----------|----------|------------|
| **Tutorial** | Aprender haciendo (primer contacto) | [`QUICKSTART.md`](QUICKSTART.md) |
| **How-to** | Resolver una tarea concreta | [`how-to/`](how-to/) |
| **Reference** | Consultar datos técnicos (tablas, claves, rutas) | [`reference/`](reference/) |
| **Explanation** | Entender el sistema (por qué, arquitectura) | [`explanation/`](explanation/) |

---

## Documentos canónicos (fuente única)

| Tema | Documento | No duplicar en |
|------|-----------|----------------|
| Requisitos funcionales 2A/2B, contrato cola, criterios UAT | [`PRD_Integracion_HubSpot_2A_2B.md`](PRD_Integracion_HubSpot_2A_2B.md) | README, AGENTS, CLAUDE |
| Batch + servicio Windows (3 capas, Config.xml, TLS, emails) | [`BatchProcess_Desarrollo_e_Implementacion.md`](BatchProcess_Desarrollo_e_Implementacion.md) | README raíz |
| Tests, categorías xUnit, cobertura ≥90% | [`TESTING.md`](TESTING.md) | scriptsPS1/README |
| Scripts PowerShell (parámetros) | [`../scriptsPS1/README.md`](../scriptsPS1/README.md) | TESTING, QUICKSTART |
| Código .NET (proyectos sln) | [`../SolucionInterfazHubSpot/README.md`](../SolucionInterfazHubSpot/README.md) | reference/mapas-codigo |
| Paquete servicio Windows | [`../implementacion/README.md`](../implementacion/README.md) | how-to/desplegar |
| Planificación GSD (fases, estado) | [`.planning/ROADMAP.md`](../.planning/ROADMAP.md), [`.planning/STATE.md`](../.planning/STATE.md) | docs técnicos |
| Agentes Cursor (rules, skills, MCP) | [`agents/INDEX.md`](agents/INDEX.md), [`../AGENTS.md`](../AGENTS.md) | CLAUDE (solo punteros) |

---

## Índice por tema (búsqueda rápida)

| Busco… | Ir a |
|--------|------|
| Clone, build, verify en 5 min | [`QUICKSTART.md`](QUICKSTART.md) |
| Arquitectura capas y componentes | [`explanation/arquitectura.md`](explanation/arquitectura.md) |
| Modelo de dominio (entidades, límites) | [`explanation/dominio.md`](explanation/dominio.md) |
| Flujo 2A (cola → HubSpot) / 2B (cuenta corriente) | [`explanation/flujos-2a-2b.md`](explanation/flujos-2a-2b.md) → detalle PRD |
| `Web.config` / `App.config`, claves HubSpot | [`reference/configuracion.md`](reference/configuracion.md) |
| Tabla cola, SPs, scripts SQL | [`reference/base-datos.md`](reference/base-datos.md) |
| Endpoints HubSpot CRM v3 usados, rate limit | [`reference/hubspot-crm.md`](reference/hubspot-crm.md) |
| Endpoints MVC traza / jobs manuales | [`reference/consola-mvc.md`](reference/consola-mvc.md) |
| Clases, namespaces, runners | [`reference/mapas-codigo.md`](reference/mapas-codigo.md) |
| Depurar sync con trazas JSON | [`how-to/debug-integracion.md`](how-to/debug-integracion.md) |
| Config dev local + mock HubSpot | [`how-to/configurar-desarrollo-local.md`](how-to/configurar-desarrollo-local.md) |
| Deploy servicio Windows Calzetta | [`how-to/desplegar-servicio-windows.md`](how-to/desplegar-servicio-windows.md) |
| Tarea como agente AI | [`agents/INDEX.md`](agents/INDEX.md) |

---

## Entradas del repositorio

| Archivo | Audiencia | Contenido |
|---------|-----------|-----------|
| [`../README.md`](../README.md) | Humanos | Visión general + enlaces (sin tablas largas) |
| [`../AGENTS.md`](../AGENTS.md) | Agentes Cursor | Comandos, reglas críticas, puntero a este índice |
| [`../CLAUDE.md`](../CLAUDE.md) | Claude Code | Comandos build/test, puntero a docs |

---

## Histórico / no canónico

| Documento | Nota |
|-----------|------|
| [`RequerimientosIniciales/integracion_hubspot_mastersoft.md`](RequerimientosIniciales/integracion_hubspot_mastersoft.md) | Notas iniciales; superseded por PRD |
| [`RequerimientosIniciales/eliminar-legacy-sperta.md`](RequerimientosIniciales/eliminar-legacy-sperta.md) | Plan legacy SpertaAPI |
| [`.planning/research/`](../.planning/research/) | Investigación GSD; verificar contra código antes de usar |
