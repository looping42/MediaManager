using Dapper;
using MediaManager.Domain.Interface;

namespace MediaManager.Infrastructure.Persistence
{
    public class SettingsRepository : ISettingsRepository
    {
        public void Save(string key, string value)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
                INSERT INTO Settings (Key, Value)
                VALUES (@Key, @Value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
            ";

            connection.Execute(sql, new { Key = key, Value = value });
        }

        public string? Get(string key)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            return connection.QueryFirstOrDefault<string>(
                "SELECT Value FROM Settings WHERE Key = @Key",
                new { Key = key }
            );
        }

        public Dictionary<string, string> GetAll()
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            var rows = connection.Query<(string Key, string Value)>("SELECT Key, Value FROM Settings");
            return rows.ToDictionary(r => r.Key, r => r.Value);
        }

        /// <summary>
        /// Sauvegarde une liste de chemins de scan
        /// </summary>
        public void SavePaths(string key, List<string> paths)
        {
            var serialized = string.Join("|", paths); // on sépare par un caractère unique
            Save(key, serialized);
        }

        /// <summary>
        /// Récupère une liste de chemins de scan
        /// </summary>
        public List<string> GetPaths(string key)
        {
            var serialized = Get(key);
            if (string.IsNullOrWhiteSpace(serialized))
                return new List<string>();

            return serialized.Split('|').Select(p => p.Trim()).ToList();
        }
    }
}