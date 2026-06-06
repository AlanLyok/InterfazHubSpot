
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using BatchSpertaAPI.Filters;
using Newtonsoft.Json;

namespace BatchSpertaAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new CustomWebApiExceptionHandlerAttribute());

            config.Filters.Add(new CustomWebApiAuthorizeAttribute());

            var jsonFormatter = config.Formatters.JsonFormatter;

            jsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
        }
    }
}



