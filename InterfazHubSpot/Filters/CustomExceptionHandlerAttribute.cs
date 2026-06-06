
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using InterfazHubSpot.Business;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Filters
{
    public class CustomExceptionHandlerAttribute : FilterAttribute, IExceptionFilter
    {
        //--------------------------------------------------
        //  Constantes Privadas
        //--------------------------------------------------

        private const string DefaultCNPrefix = "MsGestion";

        //--------------------------------------------------
        //  Metodos Publicos
        //--------------------------------------------------

        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                try
                {
                    var oErrorRegister = new ErrorRegister();

                    var oErrorData = oErrorRegister.GetFullErrorData(filterContext.Exception);

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
                        FullException = oErrorData.FullException,
                        //TenantId = "TECHINT"
                    };

                    var oMSContext = new MSContext()
                    {
                        CNPrefix = DefaultCNPrefix,
                        EmpresaId = 0,
                        //TenantId = "TECHINT"
                    };

                    var oErroresManager = new ErroresManager(oMSContext);

                    oErroresManager.Grabar(oErrores);

                    if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var json = oErrorRegister.GetJsonExceptionMessage(filterContext.Exception);

                        filterContext.Result = new ContentResult { Content = json, ContentType = "application/json" };
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(
                                               new RouteValueDictionary
                                               {
                                                    { "controller", "Home" },
                                                    { "action", "Error" }
                                               });
                    }
                }
                catch (Exception)
                {
                    var oErrorRegister = new ErrorRegister();

                    if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var json = oErrorRegister.GetJsonExceptionMessage(filterContext.Exception);

                        filterContext.Result = new ContentResult { Content = json, ContentType = "application/json" };
                    }
                    else
                    {
                        filterContext.Result = new RedirectToRouteResult(
                                               new RouteValueDictionary
                                               {
                                                    { "controller", "Home" },
                                                    { "action", "Error" }
                                               });
                    }
                }

                filterContext.ExceptionHandled = true;
            }
        }
    }
}



