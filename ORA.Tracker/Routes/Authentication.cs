using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Authentication : Route
    {
        private static readonly byte[] missingKey = new Error("Missing key").ToBytes();
        private static readonly byte[] invalidKeyStructure = new Error("Invalid key structure").ToBytes();

        public Authentication(IServiceCollection services)
            : base(services) { }

        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            if (!request.HasEntityBody)
            {
                response.BadRequest(missingKey);
                return;
            }

            byte[] publicKey = Convert.FromBase64String(Encoding.Default.GetString(request.Body));

            if (publicKey.Length < 16)
            {
                response.BadRequest(invalidKeyStructure);
                return;
            }

            string id = new Guid(publicKey.Take(16).ToArray()).ToString();
            string token;
            byte[] encryptedToken;

            if (this.services.TokenManager.IsNodeRegistered(id, request.Ip))
                token = this.services.TokenManager.GetToken(id, request.Ip);
            else
                token = this.services.TokenManager.RegisterNode(id, request.Ip);

            if (this.services.TokenManager.IsTokenExpired(token))
                token = this.services.TokenManager.UpdateToken(id, request.Ip);

            try
            {
                var csp = new RSACryptoServiceProvider();
                csp.ImportRSAPublicKey(publicKey, out int _);
                encryptedToken = csp.Encrypt(Encoding.UTF8.GetBytes(token), true);
            }
            catch (CryptographicException)
            {
                response.BadRequest(invalidKeyStructure);
                return;
            }

            response.Close(Encoding.UTF8.GetBytes(Convert.ToBase64String(encryptedToken)), true);
        }
    }
}
