using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

using ORA.Tracker.Models;
using ORA.Tracker.Services;

namespace ORA.Tracker.Routes
{
    public class Authentication : Route
    {
        private static readonly string missingKey = new Error("Missing key").ToString();
        private static readonly string invalidKeyStructure = new Error("Invalid key structure").ToString();

        public Authentication()
            : base() { }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.HasEntityBody)
                throw new HttpListenerException(400, missingKey);

            byte[] publicKey = this.getBody(request);
            if (publicKey.Length < 16)
                throw new HttpListenerException(400, invalidKeyStructure);

            string id = new Guid(publicKey.Take(16).ToArray()).ToString();

            string token = TokenManager.Instance.NewToken();
            byte[] encryptedToken;

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

            TokenManager.Instance.RegisterToken(id, token);

            return encryptedToken;
        }
    }
}
