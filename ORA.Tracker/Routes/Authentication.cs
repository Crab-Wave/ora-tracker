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

            if (this.services.TokenManager.IsRegistered(id))
                token = this.services.TokenManager.GetTokenFromId(id);
            else
                token = this.services.TokenManager.NewToken();

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

            if (!this.services.TokenManager.IsRegistered(token))
                this.services.TokenManager.RegisterToken(id, token);
            else if (this.services.TokenManager.IsTokenExpired(token))
                this.services.TokenManager.UpdateToken(id, token);

            this.services.NodeManager.Put(id, request.Ip.ToString());

            response.Close(Encoding.UTF8.GetBytes(Convert.ToBase64String(encryptedToken)), true);
        }
    }
}
