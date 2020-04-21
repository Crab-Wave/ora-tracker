using System;
using System.Net;
using System.Security.Cryptography;

using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Authentication : Route
    {
        private static readonly string missingIdParameter = new Error("Missing id parameter").ToString();
        private static readonly string missingKey = new Error("Missing key").ToString();
        private static readonly string invalidKeyStructure = new Error("Invalid key structure").ToString();

        public Authentication()
            : base() { }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            string[] idValues = request.QueryString.GetValues("id");
            if (idValues == null || idValues.Length < 1)
                throw new HttpListenerException(400, missingIdParameter);

            if (!request.HasEntityBody)
                throw new HttpListenerException(400, missingKey);

            byte[] publicKey = this.getBody(request);
            string id = idValues[0];

            byte[] token = generateToken(32);

            RSACryptoServiceProvider csp;
            try
            {
                csp = new RSACryptoServiceProvider(publicKey.Length);
                csp.ImportRSAPublicKey(publicKey, out int _);
            }
            catch (CryptographicException)
            {
                throw new HttpListenerException(400, invalidKeyStructure);
            }

            var encryptedToken = csp.Encrypt(token, true);

            // TODO: hashmap id -> token
            // TODO: hashmap token -> long    timestamp

            return encryptedToken;
        }

        private static byte[] generateToken(int length)
        {
            var rng = new Random();
            var token = new byte[length];

            for (int i = 0; i < length; i++)
                token[i] = (byte) rng.Next(256);

            return token;
        }
    }
}
