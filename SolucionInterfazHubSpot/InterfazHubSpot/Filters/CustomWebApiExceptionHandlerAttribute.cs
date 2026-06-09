
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using InterfazHubSpot.Business;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Filters
{
    public class CustomWebApiExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        //--------------------------------------------------
        //  Constantes Privadas
        //--------------------------------------------------

        private const string DefaultCNPrefix = "InterfazHubSpot";

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                try
                {
                    var oErrorRegister = new ErrorRegister();

                    var oErrorData = oErrorRegister.GetFullErrorData(context.Exception);

                    var message = oErrorData.Message;

                    if (message.Length > 200)
                    {
                        message = message.Substring(0, 200);
                    }

                    var oErrores = new Errores()
                    {
                        ErrorDateTime = DateTime.Now,
                        MachineName = oErrorData.MachineName,
                        AppDomainName = oErrorData.AppDomainName,
                        WindowsIdentity = oErrorData.WindowsIdentity,
                        Message = message,
                        FullException = oErrorData.FullException
                    };

                    var oMSContext = new MSContext()
                    {
                        CNPrefix = DefaultCNPrefix,
                        EmpresaId = 0
                    };

                    var oErroresManager = new ErroresManager(oMSContext);

                    oErroresManager.Grabar(oErrores);

                    var json = oErrorRegister.GetJsonExceptionMessage(context.Exception);

                    context.Response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }
                catch (Exception)
                {
                    var oErrorRegister = new ErrorRegister();

                    var json = oErrorRegister.GetJsonExceptionMessage(context.Exception);

                    context.Response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                    };
                }
            }
        }
    }
}


