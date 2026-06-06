using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BatchSpertaAPI.Core;
using Mastersoft.Framework.WebRenderMvc.HtmlHelpers;

namespace BatchSpertaAPI.Filters
{
    public class CustomWebApiAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var oMsContext = Util.GetMSContext();

            if (oMsContext.PerfilId == 0)
            {
                return true;
            }

            var autorize = false;

            var link = "/";

            var uri = actionContext.Request.RequestUri.AbsoluteUri.ToLower();

            var partes = uri.Split('/');

            var esApi = false;

            for (int i = 0; i < partes.Length - 1; i++)
            {
                if (partes[i] == "api")
                {
                    esApi = true;
                }
                else if (esApi)
                {
                    link += partes[i];
                    break;
                }
            }

            if (link.StartsWith("/bus"))
            {
                autorize = true;
            }
            else
            {
                if (link.EndsWith("api"))
                {
                    link = link.Substring(0, link.Length - 3);
                }

                autorize = HtmlMenu.HayPermiso(link);
            }

            return autorize;
        }
    }
}

