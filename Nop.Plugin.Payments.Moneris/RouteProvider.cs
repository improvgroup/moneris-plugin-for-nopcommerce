using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;

namespace Nop.Plugin.Payments.Moneris
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Success
            routeBuilder.MapRoute("Plugin.Payments.Moneris.SuccessCallbackHandler",
                 "Plugins/PaymentMoneris/SuccessCallbackHandler",
                 new { controller = "PaymentMoneris", action = "SuccessCallbackHandler" });

            //Fail
            routeBuilder.MapRoute("Plugin.Payments.Moneris.FailCallbackHandler",
                 "Plugins/PaymentMoneris/FailCallbackHandler",
                 new { controller = "PaymentMoneris", action = "FailCallbackHandler" });
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
