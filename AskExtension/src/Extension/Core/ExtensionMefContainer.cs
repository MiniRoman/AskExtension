using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Extension.StackOverflow.Common;

namespace AskExtension.Core
{
    static class ExtensionMefContainer
    {
        private static ICompositionService _container;
        public static ICompositionService Service
        {
            get
            {
                if (_container == null)
                {
                    var assemblies = new[]
                    {
                        Assembly.GetExecutingAssembly(),
                        typeof(Authentication).Assembly
                    }.Select(x=>new AssemblyCatalog(x));
                    var catalog = new AggregateCatalog(assemblies);
                    _container = new CompositionContainer(catalog);
                }
                return _container;
            }
        }

        [Export("TextTest")]
        public static string Text => "It's text test";
    }
}
