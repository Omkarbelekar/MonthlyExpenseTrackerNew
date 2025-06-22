using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using SplitwiseCloneWebAPI;

namespace SplitwiseCloneWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //var cors = new EnableCorsAttribute("*", "*", "*"); // Allow all — for dev
            //config.EnableCors(cors);

            var cors = new EnableCorsAttribute("https://nice-sea-044c05c1e.6.azurestaticapps.net", "*", "*");
            config.EnableCors(cors);
            config.MessageHandlers.Add(new PreflightRequestsHandler());



            // Web API routes
            config.MapHttpAttributeRoutes();
            


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
