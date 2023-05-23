using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HmPMer.Pay
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("OrderNotity", "OrderNotity/{code}/{id}", new
            {
                controller = "Order",
                action = "Notity",
                id = UrlParameter.Optional
            });
            routes.MapRoute("OrderReturn", "OrderReturn/{code}/{id}", new
            {
                controller = "Order",
                action = "Return",
                id = UrlParameter.Optional
            });
            routes.MapRoute("PayAuth", "PayAuth/{supplierCode}/{id}", new
            {
                controller = "PayAuth",
                action = "Gateway",
                id = UrlParameter.Optional
            });
            routes.MapRoute("Default", "{controller}/{action}/{id}", new
            {
                controller = "Bank",
                action = "Index",
                id = UrlParameter.Optional
            });
        }
    }
}
