using System;
using System.Collections.Generic;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Interfaces.Managers;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Encola correos de error contextualizados para flujos HubSpot 2A/2B y jobs batch.</summary>
    public sealed class IntegracionErrorNotifier
    {
        private readonly IEmailsManager _emails;

        public IntegracionErrorNotifier(IEmailsManager emails)
        {
            _emails = emails ?? throw new ArgumentNullException(nameof(emails));
        }

        private IntegracionErrorNotifier(MSContext contexto)
            : this(new EmailsManager(contexto))
        {
        }

        public static IntegracionErrorNotifier Create(MSContext contexto)
        {
            return new IntegracionErrorNotifier(contexto);
        }

        public void NotificarErrorFila2A(long procesoId, int clienteId, string fase, Exception ex)
        {
            var asunto = $"[HubSpot 2A] Error ProcesoId={procesoId} ClienteId={clienteId} Fase={fase ?? "desconocida"}";
            var proceso = "HubSpot integración 2A";
            _emails.GrabarEmailErrores(asunto, proceso, ConstruirLineasError(fase, ex));
        }

        public void NotificarErrorBatch2B(int lote, Exception ex)
        {
            var asunto = $"[HubSpot 2B] Error batch lote {lote}";
            var proceso = "HubSpot integración 2B";
            _emails.GrabarEmailErrores(asunto, proceso, ConstruirLineasError($"batch lote {lote}", ex));
        }

        public void NotificarErrorFatalJob(string jobName, Exception ex)
        {
            var nombre = string.IsNullOrWhiteSpace(jobName) ? "desconocido" : jobName;
            var asunto = $"[HubSpot] Error fatal — {nombre}";
            _emails.GrabarEmailErrores(asunto, nombre, ConstruirLineasError("error fatal del job", ex));
        }

        public void NotificarErrorAuth(Exception ex)
        {
            const string asunto = "[HubSpot] Error autenticación";
            const string proceso = "HubSpot autenticación";
            _emails.GrabarEmailErrores(asunto, proceso, ConstruirLineasError("401 / token inválido", ex));
        }

        internal static IEnumerable<string> ConstruirLineasError(string contexto, Exception ex)
        {
            var lineas = new List<string>();
            if (!string.IsNullOrWhiteSpace(contexto))
                lineas.Add("Contexto: " + contexto);

            if (ex == null)
                return lineas;

            lineas.Add("Mensaje: " + ex.Message);
            if (ex.InnerException != null)
                lineas.Add("Inner: " + ex.InnerException.Message);

            return lineas;
        }
    }
}
