using System;
using System.Collections.Generic;

namespace ORA.Tracker.Services
{
    public class TokenManager
    {
        public const int TokenSize = 32;
        private const double tokenLifetimeInMinutes = 20;

        private static TokenManager instance = new TokenManager();
        public static TokenManager Instance { get => instance; }

        private Dictionary<string, byte[]> tokens;
        private Dictionary<byte[], long> tokenExpirations;


        static TokenManager() { }
        private TokenManager()
        {
            this.tokens = new Dictionary<string, byte[]>();
            this.tokenExpirations = new Dictionary<byte[], long>();
        }

        public byte[] NewToken()
        {
            return generateToken(TokenSize);
        }

        public void RegisterToken(string id, byte[] token)
        {
            this.tokens.Add(id, token);
            this.tokenExpirations.Add(token, DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks);
        }

        public void RefrehToken(byte[] token)
        {
            this.tokenExpirations[token] = DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks;
        }

        private static byte[] generateToken(int size)
        {
            var rng = new Random();
            var token = new byte[size];

            for (int i = 0; i < size; i++)
                token[i] = (byte) rng.Next(256);

            return token;
        }
    }
}
