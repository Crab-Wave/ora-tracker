using System;
using System.Text.Json;
using System.Collections.Generic;
using Jose;

namespace ORA.Tracker.Models
{
    public class Token
    {
        private static readonly byte[] secretKey = generateSecretKey();

        public string token { get; set; }

        public Token(int expiresIn, string id)
        {
            var payload = new Dictionary<string, object>()
            {
                { "exp", DateTime.UtcNow.AddMinutes(expiresIn).Ticks },
                { "id", id }
            };

            this.token = JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
        }

        public Token(string token)
        {
            this.token = token;
        }

        public string GetPayload()
        {
            return JWT.Decode(this.token, secretKey, JwsAlgorithm.HS256);
        }

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes<Token>(this, new JsonSerializerOptions { WriteIndented = true });

        public Token Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<Token>(jsonBytes);

        private static byte[] generateSecretKey()
        {
            var rng = new Random();
            var secretKey = new byte[32];

            for (int i = 0; i < 32; i++)
                secretKey[i] = (byte) rng.Next(256);

            return secretKey;
        }
    }
}
