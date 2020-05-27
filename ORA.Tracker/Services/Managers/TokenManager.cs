using System;
using System.Collections.Generic;
using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Managers
{
    public class TokenManager
    {
        public const int TokenSize = 32;
        private const double tokenLifetimeInMinutes = 90;

        private Dictionary<Node, string> tokens;
        private Dictionary<string, Node> nodes;
        private Dictionary<string, long> tokenExpirations;

        public TokenManager()
        {
            this.tokens = new Dictionary<Node, string>();
            this.nodes = new Dictionary<string, Node>();
            this.tokenExpirations = new Dictionary<string, long>();
        }

        public string NewToken() => generateToken(TokenSize);

        public void RegisterToken(Node node, string token)
        {
            if (this.IsRegistered(node))
                throw new ArgumentException("User already registered.");

            if (this.IsTokenRegistered(token) && this.nodes[token] != node)
                throw new ArgumentException("A user with a different id is already registered with this token.");

            this.tokens.Add(node, token);
            this.nodes.Add(token, node);
            this.tokenExpirations.Add(token, this.GetExpirationDateFromNow());
        }

        public void RefreshToken(string token)
        {
            this.tokenExpirations[token] = this.GetExpirationDateFromNow();
        }

        public void UpdateToken(Node node, string token)
        {
            this.nodes.Remove(token);
            this.tokenExpirations.Remove(token);

            string newToken = this.NewToken();
            this.tokens[node] = newToken;
            this.nodes[newToken] = node;
            this.tokenExpirations[newToken] = this.GetExpirationDateFromNow();
        }

        public void RemoveToken(string token)
        {
            foreach (Node node in this.tokens.Keys)
                if (this.tokens[node].Equals(token))
                    this.tokens.Remove(node);
            this.nodes.Remove(token);
            this.tokenExpirations.Remove(token);
        }

        public string GetTokenFromNode(Node node) => this.tokens[node];
        public Node GetNodeFromToken(string token) => this.nodes[token];

        public bool IsRegistered(Node node) => this.tokens.ContainsKey(node);

        public bool IsValidToken(string token) => this.IsTokenRegistered(token) && !this.IsTokenExpired(token);
        public bool IsTokenRegistered(string token) => this.nodes.ContainsKey(token);
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
