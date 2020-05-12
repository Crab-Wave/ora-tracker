using System;
using System.Collections.Generic;

namespace ORA.Tracker.Services.Managers
{
    public class TokenManager
    {
        public const int TokenSize = 32;
        private const double tokenLifetimeInMinutes = 20;

        private Dictionary<string, string> tokens;
        private Dictionary<string, string> ids;
        private Dictionary<string, long> tokenExpirations;

        public TokenManager()
        {
            this.tokens = new Dictionary<string, string>();
            this.ids = new Dictionary<string, string>();
            this.tokenExpirations = new Dictionary<string, long>();
        }

        public string NewToken() => generateToken(TokenSize);

        public void RegisterToken(string id, string token)
        {
            if (this.IsRegistered(id))
                throw new ArgumentException("User already registered.");

            if (this.IsTokenRegistered(token) && this.ids[token] != id)
                throw new ArgumentException("A user with a different id is already registered with this token.");

            this.tokens.Add(id, token);
            this.ids.Add(token, id);
            this.tokenExpirations.Add(token, DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks);
        }

        public void RefreshToken(string token)
        {
            this.tokenExpirations[token] = DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks;
        }

        public string GetTokenFromId(string id) => this.tokens[id];
        public string GetIdFromToken(string token) => this.ids[token];

        public bool IsRegistered(string id) => this.tokens.ContainsKey(id);

        public bool IsValidToken(string token) => this.IsTokenRegistered(token) && !this.IsTokenExpired(token);
        public bool IsTokenRegistered(string token) => this.ids.ContainsKey(token);
        public bool IsTokenExpired(string token) => DateTime.UtcNow.Ticks > this.tokenExpirations[token];

        private static string generateToken(int size)
        {
            var rng = new Random();
            var token = new byte[size];

            for (int i = 0; i < size; i++)
                token[i] = (byte) rng.Next(256);

            return Convert.ToBase64String(token);
        }
    }
}
