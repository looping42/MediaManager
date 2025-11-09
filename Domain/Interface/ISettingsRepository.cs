using MediaManager.Domain;

namespace MediaManager.Domain.Interface
{
    public interface ISettingsRepository
    {
        void Save(string key, string value);

        string? Get(string key);

        Dictionary<string, string> GetAll();

        void SavePaths(string key, List<string> paths);

        List<string> GetPaths(string key);
    }
}