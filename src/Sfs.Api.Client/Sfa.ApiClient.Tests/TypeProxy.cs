using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sfa.ApiClient.Tests
{
    public class TypeProxy : MarshalByRefObject
    {
        public Assembly LoadFromAssembly(string assemblyPath)
        {
            try
            {
                return Assembly.LoadFile(assemblyPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
    }
}
