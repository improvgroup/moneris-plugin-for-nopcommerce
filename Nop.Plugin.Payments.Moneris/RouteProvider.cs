using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Moneris
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //Success
            routes.MapRoute("Plugin.Payments.Moneris.SuccessCallbackHandler",
                 "Plugins/PaymentMoneris/SuccessCallbackHandler",
                 new { controller = "PaymentMoneris", action = "SuccessCallbackHandler" },
                 new[] { "Nop.Plugin.Payments.Moneris.Controllers" }
            );

            //Fail
            routes.MapRoute("Plugin.Payments.Moneris.FailCallbackHandler",
                 "Plugins/PaymentMoneris/FailCallbackHandler",
                 new { controller = "PaymentMoneris", action = "FailCallbackHandler" },
                 new[] { "Nop.Plugin.Payments.Moneris.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
