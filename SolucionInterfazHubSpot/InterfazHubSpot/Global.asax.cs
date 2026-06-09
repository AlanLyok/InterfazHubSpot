using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using InterfazHubSpot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InterfazHubSpot
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //Autofac Configuration
            var builder = new Autofac.ContainerBuilder();

            builder.RegisterApiControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();

            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();

            builder.RegisterType<MSContextProvider>().As<IMSContextProvider>().InstancePerRequest();

            builder.RegisterAssemblyTypes(Assembly.Load("InterfazHubSpot.Business"))
                   .Where(t => t.Name.EndsWith("Manager"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}


