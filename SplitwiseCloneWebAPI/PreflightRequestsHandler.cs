using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SplitwiseCloneWebAPI
{
    public class PreflightRequestsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                //response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200");
                response.Headers.Add("Access-Control-Allow-Origin", "https://nice-sea-044c05c1e.6.azurestaticapps.net");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                return Task.FromResult(response);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}