namespace Eventing
{
  internal interface IConfigService
  {
    bool IsDirty { get; set; }
    void Load();
    void Save();
    void SetValue(string key, string value);
    string GetValue(string key);
    void RemoveKey(string key);
  }
}