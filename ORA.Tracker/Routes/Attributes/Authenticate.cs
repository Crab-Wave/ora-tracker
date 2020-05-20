using System;
using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Models;
using ORA.Tracker.Services;

namespace ORA.Tracker.Routes.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Authenticate : Attribute
    {
        private static readonly byte[] missingCredentials = new Error("Missing credentials").ToBytes();
        private static readonly byte[] invalidCredentialsType = new Error("Invalid credentials type").ToBytes();
        private static readonly byte[] InvalidToken = new Error("Invalid token").ToBytes();

        public Authenticate() { }

        public Action<HttpRequest, HttpListenerResponse, HttpRequestHandler> GetHandler(IServiceCollection services)
        {
            return (request, response, next) =>
            {
                string[] authorizationValues = request.Headers.GetValues("Authorization")?[0]?.Split(" ");
                if (authorizationValues == null || authorizationValues.Length < 2)
                {
                    response.BadRequest(missingCredentials);
                    return;
                }

                if (authorizationValues[0] != "Bearer")
                {
                    response.BadRequest(invalidCredentialsType);
                    return;
                }

                request.Token = authorizationValues[1];

                if (!services.TokenManager.IsValidToken(request.Token))
                {
                    response.BadRequest(InvalidToken);
                    return;
                }

                next.Handle(request, response);
            };
        }
    }
}
