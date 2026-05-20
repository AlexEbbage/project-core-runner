namespace CoreRacer.Services.Save
{
    public interface ISaveStorage
    {
        bool Exists(string key);
        string Load(string key);
        void Save(string key, string value);
        void Delete(string key);
    }
}
