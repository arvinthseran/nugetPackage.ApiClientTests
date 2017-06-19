using System;
using System.Reflection;

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
