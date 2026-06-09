using System.Web;
using System.Web.Mvc;

using InterfazHubSpot.Filters;

namespace InterfazHubSpot
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomExceptionHandlerAttribute());
            filters.Add(new CustomAuthorizeAttribute());
        }
    }
}


