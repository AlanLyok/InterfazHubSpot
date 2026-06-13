using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Web;
using InterfazHubSpot.Interfaces.Managers;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Interfaces;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business.Managers
{
    public class EmailsManager : IEmailsManager, IDisposable
    {
        internal const string StoredProcedureAgregar = "dbo.MSEMails_Agregar";
        internal const int AsuntoMaxLength = 100;

        private readonly MSContext _ctx;
        private IUnitOfWorkAsync _unitOfWork;
        private readonly string _templatePath;

        public EmailsManager(MSContext contexto)
        {
            _ctx = contexto;
            _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "error_template.html");
        }

        private IUnitOfWorkAsync EnsureUnitOfWork()
        {
            if (_unitOfWork == null)
                _unitOfWork = new UnitOfWork(_ctx, new MSGestionContext(_ctx));
            return _unitOfWork;
        }

        public void GrabarEmailErroresProcesamiento(string proceso, IEnumerable<string> errores = null)
        {
            var asunto = $"{proceso} - Errores en procesamiento";
            GrabarEmailErrores(asunto, proceso, errores);
        }

        public void GrabarEmailErrores(string asunto, string proceso, IEnumerable<string> errores = null, string procesoId = null)
        {
            var emailPara = ConfigurationManager.AppSettings["EmailErrPara"];
            if (string.IsNullOrWhiteSpace(emailPara))
                return;

            var cuerpo = ConstruirHtmlErrores(proceso, errores, procesoId: procesoId);

            var emailDE = ResolverRemitente(ConfigurationManager.AppSettings["EmailErrDE"]);
            var emailCc = ConfigurationManager.AppSettings["EmailErrCc"] ?? string.Empty;

            AgregarEmail(emailDE, emailPara.Trim(), emailCc, TruncarAsunto(asunto), "S", cuerpo);
        }

        internal static string ResolverRemitente(string emailErrDe)
        {
            if (!string.IsNullOrWhiteSpace(emailErrDe))
                return emailErrDe.Trim();

            var fallback = ConfigurationManager.AppSettings["EmailDe"];
            return string.IsNullOrWhiteSpace(fallback) ? string.Empty : fallback.Trim();
        }

        internal static string TruncarAsunto(string asunto)
        {
            if (string.IsNullOrEmpty(asunto))
                return string.Empty;

            return asunto.Length <= AsuntoMaxLength
                ? asunto
                : asunto.Substring(0, AsuntoMaxLength);
        }

        private void AgregarEmail(string correoRemitente, string correoPara, string correoCC,
            string asunto, string esHtml, string cuerpoMensaje)
        {
            if (string.IsNullOrWhiteSpace(correoPara))
                return;

            var idParam = new SqlParameter("@Id", SqlDbType.Int) { Direction = ParameterDirection.Output };

            EnsureUnitOfWork().ExecuteSqlCommand(
                "EXEC " + StoredProcedureAgregar + " @De, @Para, @Cc, @Cco, @Adjuntos, @Asunto, @EsHtml, @Mensaje, @Id OUTPUT",
                new SqlParameter("@De", (object)correoRemitente ?? string.Empty),
                new SqlParameter("@Para", correoPara),
                new SqlParameter("@Cc", correoCC ?? string.Empty),
                new SqlParameter("@Cco", string.Empty),
                new SqlParameter("@Adjuntos", string.Empty),
                new SqlParameter("@Asunto", asunto ?? string.Empty),
                new SqlParameter("@EsHtml", esHtml ?? "S"),
                new SqlParameter("@Mensaje", cuerpoMensaje ?? string.Empty),
                idParam);
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        private string LoadTemplateOrFallback()
        {
            if (File.Exists(_templatePath))
                return File.ReadAllText(_templatePath);

            return "<html><body><h2>Error en {{Proceso}}</h2><p>{{Fecha}}</p>{{Detalle}}</body></html>";
        }

        public string ConstruirHtmlErrores(string proceso, IEnumerable<string> errores = null, DateTime? fecha = null, string procesoId = null)
        {
            var template = LoadTemplateOrFallback();
            var procesoSafe = HttpUtility.HtmlEncode(proceso ?? "Desconocido");
            string detalle;

            if (errores?.Any() == true)
            {
                var items = string.Concat(errores.Select(e => $"<li>{HttpUtility.HtmlEncode(e)}</li>"));
                detalle = $"<b>Detalle:</b><ul>{items}</ul>";
            }
            else
            {
                detalle = "<div>No se pudieron obtener los detalles del error.</div>";
            }

            var procesoIdBlock = string.IsNullOrWhiteSpace(procesoId)
                ? string.Empty
                : $"<div class=\"proceso-id\">Proceso ID: <span class=\"highlight\">{HttpUtility.HtmlEncode(procesoId)}</span></div>";

            var fechaStr = (fecha ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm");

            return template
                .Replace("{{Proceso}}", procesoSafe)
                .Replace("{{ProcesoIdBlock}}", procesoIdBlock)
                .Replace("{{Detalle}}", detalle)
                .Replace("{{Fecha}}", HttpUtility.HtmlEncode(fechaStr));
        }
    }
}
