using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        public static string text
        {
            get { return "It's text test"; }
        }
    }
}
