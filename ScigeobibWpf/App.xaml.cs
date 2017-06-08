using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ScigeobibWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
#if DLLS_IN_RESOURCES
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
#endif
        }

#if DLLS_IN_RESOURCES
        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fullName = args.Name;
            string name = fullName.Split(',')[0];

            var assem = Assembly.GetExecutingAssembly();
            String resourceName = assem.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith(".dlls." + name + ".dll"));
            if (resourceName == null) return null;
            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
#endif
    }
}
