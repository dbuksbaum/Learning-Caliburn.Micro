using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;

namespace HelloWorld.ViewModels
{
  public class MainViewModel : PropertyChangedBase  
  {
    private string _name;
    private string _helloString;
    
    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        NotifyOfPropertyChange(() => Name);
        NotifyOfPropertyChange(() => CanSayHello);
      }
    }
    
    public string HelloString
    {
      get { return _helloString; }
      private set
      {
        _helloString = value;
        NotifyOfPropertyChange(() => HelloString);
      }
    }

    public bool CanSayHello
    {
      get { return !string.IsNullOrWhiteSpace(Name); }
    }  

    public void SayHello(string name)
    {
      HelloString = string.Format("Hello {0}.", Name);
    }
  }
}