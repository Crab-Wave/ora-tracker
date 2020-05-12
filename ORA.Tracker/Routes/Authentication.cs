using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
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

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response,
            Dictionary<string, string> urlParams = null)
        {
            if (!request.HasEntityBody)
                throw new HttpListenerException(400, missingKey);

            byte[] publicKey = Convert.FromBase64String(Encoding.Default.GetString(this.getBody(request)));

            if (publicKey.Length < 16)
                throw new HttpListenerException(400, invalidKeyStructure);

            string id = new Guid(publicKey.Take(16).ToArray()).ToString();
            string token;
            byte[] encryptedToken;

            if (TokenManager.Instance.IsValidToken(id))
                token = TokenManager.Instance.GetTokenFromId(id);
            else
                token = TokenManager.Instance.NewToken();

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

            if (!TokenManager.Instance.IsRegistered(id))
                TokenManager.Instance.RegisterToken(id, token);
            // Refresh token if id is registered ?

            return Encoding.UTF8.GetBytes(Convert.ToBase64String(encryptedToken));
        }
    }
}
