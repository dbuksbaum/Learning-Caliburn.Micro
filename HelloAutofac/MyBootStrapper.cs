using System;
using System.Collections.Generic;
using Caliburn.Micro;
using HelloAutofac.Util;
using HelloAutofac.ViewModels;

namespace HelloAutofac
{
  public class MyBootStrapper : TypedAutofacBootStrapper<ShellViewModel>
  {
    #region Fields
    private readonly ILog _logger = LogManager.GetLog(typeof(MyBootStrapper));
    #endregion

    #region Constructor
    static MyBootStrapper()
    {
      LogManager.GetLog = type => new DebugLogger(typeof(MyBootStrapper));
    }
    #endregion

    #region Overrides
    protected override void ConfigureContainer(Autofac.ContainerBuilder builder)
    {
      _logger.Info("Configuring Container.");
      base.ConfigureContainer(builder);
      
      //  good place to register application types or custom modules
      //builder.RegisterModule<RegistrationModule>();
    }
    #endregion
  }
}