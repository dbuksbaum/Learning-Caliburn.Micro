using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Eventing.Messages;
using Newtonsoft.Json;

namespace Eventing
{
  /// <summary>
  /// A super simplistic config service that believes everything just works so it does no error checking. :)
  /// </summary>
  class ConfigService : IConfigService
  {
    #region Constants
    private const string ConfigFileName = "MySample.Config";
    #endregion
    
    #region Fields
    private readonly IEventAggregator _messageBus;
    private IDictionary<string, string> _configuration = new Dictionary<string, string>();
    #endregion

    #region Properties
    public bool IsDirty { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object"/> class.
    /// </summary>
    public ConfigService([Import(typeof(IEventAggregator))] IEventAggregator messageBus)
    {
      IsDirty = false;
      _messageBus = messageBus;
    }
    #endregion

    public void Load()
    {
      if (!File.Exists(ConfigFileName))
        SaveConfiguration(ConfigFileName, new Dictionary<string, string>()
                                            {
                                              { "one", "value1"},
                                              { "two", "value2"}
                                            });

      _configuration = LoadConfiguration(ConfigFileName);
      IsDirty = false;
      //  notify everyone that a config was loaded
      _messageBus.Publish(new ConfigLoaded());
    }
    public void Save()
    {
      SaveConfiguration(ConfigFileName, _configuration);
      IsDirty = false;
      //  notify everyone that a config was saved
      _messageBus.Publish(new ConfigSaved());
    }
    public void SetValue(string key, string value)
    {
      _configuration[key] = value;
      IsDirty = true;
      //  notify everyone that a config key was changed
      _messageBus.Publish(new ConfigValueChanged { Key = key, Value = value });
    }
    public string GetValue(string key)
    {
      string value;
      if (!_configuration.TryGetValue(key, out value))
        value = string.Empty;
      return value;
    }
    public void RemoveKey(string key)
    {
      if (_configuration.ContainsKey(key))
        _configuration.Remove(key);
    }

    #region Helper Methods
    private IDictionary<string, string> LoadConfiguration(string configFileName)
    {
      using (var file = File.OpenText(configFileName))
      {
        var data = file.ReadToEnd();
        return JsonConvert.DeserializeObject(data) as IDictionary<string,string>;
      }
    }
    private void SaveConfiguration(string configFileName, IDictionary<string, string> configData)
    {
      using (var file = File.CreateText(configFileName))
      {
        var data = JsonConvert.SerializeObject(configData);
        file.Write(data);
      }
    }
    #endregion
  }
}
