using System.Collections.Specialized;
using System.Net;

using ORA.Tracker.Models;

namespace ORA.Tracker.Services
{
    public class Authorization
    {
        private static readonly string missingCredentials = new Error("Missing Credentials").ToString();
        private static readonly string invalidCredentialsType = new Error("Invalid Credentials Type").ToString();

        public static string GetToken(NameValueCollection headers)
        {
            string[] authorizationValues = headers.GetValues("Authorization")?[0]?.Split(" ")
                ?? throw new HttpListenerException(401, missingCredentials);

            if (authorizationValues.Length < 2)
                throw new HttpListenerException(401, missingCredentials);
            if (authorizationValues[0] != "Bearer")
                throw new HttpListenerException(400, invalidCredentialsType);

            return authorizationValues[1];
        }
    }
}
