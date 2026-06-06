using System.Web;
using System.Web.Mvc;

using BatchSpertaAPI.Filters;

namespace BatchSpertaAPI
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


