using System.Web;
using System.Web.Mvc;

namespace PROYECTO_E_COMERCE
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
