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
        private static readonly string missingKey = new Error("Missing key").ToString();
        private static readonly string invalidKeyStructure = new Error("Invalid key structure").ToString();

        public Authentication(IServiceCollection services)
            : base(services) { }

        protected override byte[] post(HttpRequest request, HttpListenerResponse response)
        {
            if (!request.HasEntityBody)
                throw new HttpListenerException(400, missingKey);

            byte[] publicKey = Convert.FromBase64String(Encoding.Default.GetString(request.Body));

            if (publicKey.Length < 16)
                throw new HttpListenerException(400, invalidKeyStructure);

            string id = new Guid(publicKey.Take(16).ToArray()).ToString();
            string token;
            byte[] encryptedToken;

                if (this.services.TokenManager.IsValidToken(id))
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
                throw new HttpListenerException(400, invalidKeyStructure);
            }

            if (!this.services.TokenManager.IsRegistered(id))
                this.services.TokenManager.RegisterToken(id, token);
            // Refresh token if id is registered ?

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(encryptedToken));
        }
    }
}
