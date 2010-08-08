using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using LogJam.Util;
using LogJam.ViewModels;

namespace LogJam
{
  //LogManager.GetLog = type => new DebugLogger(type);
  //LogManager.GetLog = type => new Log4netLogger(type);
  //LogManager.GetLog = type => new NLogLogger(type);
  public class MyBootStrapper : Bootstrapper<ShellViewModel>
  {
    static MyBootStrapper()
    {
      LogManager.GetLog = type => new NLogLogger(type);
    }
  }
}
