using System;
using System.Diagnostics;
using Caliburn.Micro;

namespace LogJam.Util
{
  class NLogLogger : ILog
  {
    #region Fields
    private readonly NLog.Logger _innerLogger;
    #endregion

    #region Constructors
    public NLogLogger(Type type)
    {
      _innerLogger = NLog.LogManager.GetLogger(type.Name);
    }
    #endregion

    #region ILog Members
    public void Error(Exception exception)
    {
      _innerLogger.ErrorException(exception.Message, exception);
    }
    public void Info(string format, params object[] args)
    {
      _innerLogger.Info(format, args);
    }
    public void Warn(string format, params object[] args)
    {
      _innerLogger.Warn(format, args);
    }
    #endregion
  }
}