using System;
using Caliburn.Micro;

namespace LogJam.ViewModels
{
  /// <summary>
  /// A view model for use in a Caliburn.Micro WPF project.
  /// </summary>
  public class ShellViewModel : PropertyChangedBase
  {	//	to notify of a property change, call NotifyOfPropertyChange(() => Property);
    private readonly ILog _log = LogManager.GetLog(typeof(ShellViewModel));

    public bool CanGenerateLogMessage(LogType logTypes, string logMessage)
    {
      return !string.IsNullOrWhiteSpace(logMessage);
    }

    public void GenerateLogMessage(LogType logTypes, string logMessage)
    {
      switch (logTypes)
      {
        case LogType.Info:
          _log.Info(logMessage);
          break;
        case LogType.Warn:
          _log.Warn(logMessage);
          break;
        case LogType.Error:
          _log.Error(new Exception(logMessage));
          break;
      }
    }
  }
}