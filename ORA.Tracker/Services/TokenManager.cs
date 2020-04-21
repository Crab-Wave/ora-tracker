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
        private Dictionary<byte[], string> ids;
        private Dictionary<byte[], long> tokenExpirations;


        static TokenManager() { }
        private TokenManager()
        {
            this.tokens = new Dictionary<string, byte[]>();
            this.ids = new Dictionary<byte[], string>();
            this.tokenExpirations = new Dictionary<byte[], long>();
        }

        public byte[] NewToken() => generateToken(TokenSize);

        public void RegisterToken(string id, byte[] token)
        {
            this.tokens.Add(id, token);
            this.ids.Add(token, id);
            this.tokenExpirations.Add(token, DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks);
        }

        public void RefreshToken(byte[] token)
        {
            this.tokenExpirations[token] = DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks;
        }

        public byte[] GetTokenFromId(string id) => this.tokens[id];
        public string GetIdFromToken(byte[] token) => this.ids[token];

        public bool IsValidToken(byte[] token) => this.IsTokenRegistered(token) && !this.IsTokenExpired(token);
        public bool IsTokenRegistered(byte[] token) => this.tokenExpirations.ContainsKey(token);
        public bool IsTokenExpired(byte[] token) => DateTime.UtcNow.Ticks <= this.tokenExpirations[token];

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
