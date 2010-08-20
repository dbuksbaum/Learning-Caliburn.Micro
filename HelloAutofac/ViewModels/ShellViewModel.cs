using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace HelloAutofac.ViewModels
{
  public class ShellViewModel : PropertyChangedBase
  {
    public string ImportantText
    {
      get { return "Super Important Text! So Read It!"; }
    }
  }
}
