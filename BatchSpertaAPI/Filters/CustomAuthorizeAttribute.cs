using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BatchSpertaAPI.Business;
using BatchSpertaAPI.Core;
using BatchSpertaAPI.Entities;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;
using Mastersoft.Framework.WebRenderMvc.HtmlHelpers;

namespace BatchSpertaAPI.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        //--------------------------------------------------
        //  Metodos Protegidos
        //--------------------------------------------------

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var autorize = false;

            try
            {
                var currentPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Replace("~", "").ToLower();

                if (currentPath == @"/")
                {
                    autorize = true;
                }
                else if (base.AuthorizeCore(httpContext))
                {
                    var oMsContext = Util.GetMSContext();

                    if (oMsContext.PerfilId == 0)
                    {
                        autorize = true;
                    }
                    else if (currentPath == @"/" ||
                             currentPath == @"/home" ||
                             currentPath == @"/home/error" ||
                             currentPath.StartsWith(@"/areas/") ||
                             currentPath.StartsWith(@"/account/") ||
                             currentPath.StartsWith(@"/download/") ||
                             currentPath.StartsWith(@"/gridlayouts/") ||
                             currentPath.StartsWith(@"/pivotlayouts/"))
                    {
                        autorize = true;
                    }
                    else
                    {
                        var link = currentPath;

                        var partes = link.Split('/');

                        if (partes.Length > 2)
                        {
                            link = "";

                            for (int i = 0; i < partes.Length - 1; i++)
                            {
                                if (partes[i].Length > 0)
                                {
                                    link += "/";
                                    link += partes[i];
                                }
                            }
                        }

                        autorize = HtmlMenu.HayPermiso(link);
                    }
                }

            }
            catch (Exception)
            {
                autorize = false;
            }

            return autorize;
        }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.HttpContext.Request.Url.AbsolutePath.Trim().ToLower().EndsWith("/help"))
            {
                if (!filterContext.HttpContext.Request.IsLocal)
                {
                    filterContext.Result = new RedirectToRouteResult(
                                               new RouteValueDictionary
                                               {
                                                    { "controller", "Home" },
                                                    { "action", "Error" },
                                                    { "area", null }
                                               });
                }
            }
        }
    }
}

