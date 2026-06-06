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
        private readonly MSContext _ctx;
        private readonly IUnitOfWorkAsync _unitOfWork;
        private readonly string _templatePath;

        public EmailsManager(MSContext contexto)
        {
            _ctx = contexto;
            _unitOfWork = new UnitOfWork(contexto, new MSGestionContext(contexto));
            _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "error_template.html");
        }

        public void GrabarEmailErroresProcesamiento(string proceso, IEnumerable<string> errores = null)
        {
            var asunto = $"{proceso} - Errores en procesamiento";
            var cuerpo = ConstruirHtmlErrores(proceso, errores);

            var emailDE = ConfigurationManager.AppSettings["EmailErrDE"];
            var emailPara = ConfigurationManager.AppSettings["EmailErrPara"];
            var emailCc = ConfigurationManager.AppSettings["EmailErrCc"] ?? string.Empty;

            AgregarEmail(emailDE, emailPara, emailCc, asunto, "S", cuerpo);
        }

        private void AgregarEmail(string correoRemitente, string correoPara, string correoCC,
            string asunto, string esHtml, string cuerpoMensaje)
        {

            _unitOfWork.ExecuteSqlCommand(
                         "EXEC dbo.Emails_Agregar " +
                         "@Enviado, @De, @Para , @Cc , " +
                         "@Cco, @Adjuntos, @Asunto, @EsHtml, @Mensaje ",

                        new SqlParameter("@Enviado", "N"),
                        new SqlParameter("@De", correoRemitente),
                        new SqlParameter("@Para", correoPara),
                        new SqlParameter("@Cc", correoCC),
                        new SqlParameter("@Cco", ""),
                        new SqlParameter("@Adjuntos", ""),
                        new SqlParameter("@Asunto", asunto),
                        new SqlParameter("@EsHtml", "S"),
                        new SqlParameter("@Mensaje", cuerpoMensaje)
                        );


        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        private string LoadTemplate()
        {
            if (!File.Exists(_templatePath))
                throw new FileNotFoundException("No se encontró la plantilla de email.", _templatePath);
            return File.ReadAllText(_templatePath);
        }

        public string ConstruirHtmlErrores(string proceso, IEnumerable<string> errores = null, DateTime? fecha = null)
        {
            var template = LoadTemplate();
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

            var fechaStr = (fecha ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm");

            return template
                .Replace("{{Proceso}}", procesoSafe)
                .Replace("{{Detalle}}", detalle)
                .Replace("{{Fecha}}", HttpUtility.HtmlEncode(fechaStr));
        }
    }
}
