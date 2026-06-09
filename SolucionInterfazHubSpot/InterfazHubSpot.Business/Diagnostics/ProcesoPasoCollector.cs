using System;
using System.Collections.Generic;

namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Acumula pasos en memoria durante una corrida (thread-safe ante future paralelismo).</summary>
    public sealed class ProcesoPasoCollector : IProcesoPasoReporter
    {
        private readonly List<ProcesoPasoDto> _pasos = new List<ProcesoPasoDto>();

        private readonly object _lock = new object();

        public IList<ProcesoPasoDto> ObtenerPasos()
        {
            lock (_lock)
            {
                return new List<ProcesoPasoDto>(_pasos);
            }
        }

        public void RegistrarPaso(ProcesoPasoSeverity severidad, ProcesoPasoCategoria categoria, string codigo, string mensaje, object datos)
        {
            var dto = new ProcesoPasoDto
            {
                FechaUtc = DateTime.UtcNow,
                Severidad = severidad,
                Categoria = categoria,
                Codigo = codigo ?? string.Empty,
                Mensaje = mensaje ?? string.Empty,
                Datos = datos,
            };

            lock (_lock)
            {
                _pasos.Add(dto);
            }
        }
    }
}
