using System;
using System.Linq;
using System.Collections.Generic;

namespace ORA.Tracker.Services.Managers
{
    public class TokenManager
    {
        public const int TokenSize = 32;
        private const double tokenLifetimeInMinutes = 90;

        private Dictionary<Tuple<string, string>, string> tokens;
        private Dictionary<string, string> ids;
        private Dictionary<string, List<string>> ips;
        private Dictionary<string, long> tokenExpirations;

        public TokenManager()
        {
            this.tokens = new Dictionary<Tuple<string, string>, string>();
            this.ids = new Dictionary<string, string>();
            this.ips = new Dictionary<string, List<string>>();
            this.tokenExpirations = new Dictionary<string, long>();
        }

        public string NewToken() => generateToken(TokenSize);

        public string RegisterNode(string id, string ip)
        {
            string token = this.NewToken();

            if (!this.ips.ContainsKey(id))
                this.ips.Add(id, new List<string>());

            this.tokens.Add(new Tuple<string, string>(id, ip), token);
            this.ids.Add(ip, id);
            this.ips[id].Add(ip);
            this.tokenExpirations.Add(token, this.GetExpirationDateFromNow());

            return token;
        }

        public string UpdateToken(string id, string ip)
        {
            var node = new Tuple<string, string>(id, ip);
            this.tokenExpirations.Remove(this.tokens[node]);

            string newToken = this.NewToken();
            this.tokens[node] = newToken;
            this.tokenExpirations[newToken] = this.GetExpirationDateFromNow();

            return newToken;
        }

        public void UpdateIp(string token, string newIp)
        {
            string id = "";
            string oldIp = "";
            foreach (var k in this.tokens.Keys)
            {
                if (this.tokens[k] == token)
                {
                    id = k.Item1;
                    oldIp = k.Item2;
                    break;
                }
            }

            this.tokens.Remove(new Tuple<string, string>(id, oldIp));
            this.tokens[new Tuple<string, string>(id, newIp)] = token;
            this.ips[id].Remove(oldIp);
            this.ips[id].Add(newIp);
            this.ids.Remove(oldIp);
            this.ids.Add(newIp, id);
        }

        public void RefreshToken(string token)
        {
            this.tokenExpirations[token] = this.GetExpirationDateFromNow();
        }

        public string GetIdFromIp(string ip) => this.ids[ip];
        public string GetToken(string id, string ip) => this.tokens[new Tuple<string, string>(id, ip)];

        public void RemoveToken(string token)
        {
            this.tokenExpirations.Remove(token);

            foreach (var k in this.tokens.Keys)
            {
                if (this.tokens[k] == token)
                {
                    this.tokens.Remove(k);
                    return;
                }
            }
        }

        public bool IsNodeRegistered(string id, string ip) => this.tokens.ContainsKey(new Tuple<string, string>(id, ip));

        public bool IsValidToken(string token) => this.IsTokenRegistered(token) && !this.IsTokenExpired(token);
        public bool IsTokenRegistered(string token) => this.tokens.ContainsValue(token);
        public bool IsTokenExpired(string token) => DateTime.UtcNow.Ticks > this.tokenExpirations[token];

        private long GetExpirationDateFromNow()
            => DateTime.UtcNow.AddMinutes(tokenLifetimeInMinutes).Ticks;

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
