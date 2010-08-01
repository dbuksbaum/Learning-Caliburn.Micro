using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using HelloMef.Contracts;

namespace HelloMef
{
  public class MefBootStrapper : Bootstrapper<IShell>
  {
    #region Fields
    private CompositionContainer _container;
    #endregion

    #region Overrides
    protected override void Configure()
    { //  configure container
#if SILVERLIGHT
      _container = CompositionHost.Initialize(
        new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
#else
      _container = new CompositionContainer(
        new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
#endif

      var batch = new CompositionBatch();

      batch.AddExportedValue<IWindowManager>(new WindowManager());
      batch.AddExportedValue<IEventAggregator>(new EventAggregator());
      batch.AddExportedValue(_container);

      _container.Compose(batch);
    }
    protected override object GetInstance(Type serviceType, string key)
    {
      string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
      var exports = _container.GetExportedValues<object>(contract);

      if (exports.Count() > 0)
        return exports.First();

      throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
    }
    protected override IEnumerable<object> GetAllInstances(Type serviceType)
    {
      return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
    }
    protected override void BuildUp(object instance)
    {
      _container.SatisfyImportsOnce(instance);
    }
    #endregion
  }
}