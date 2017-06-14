using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    public class Class1
    {
        public bool Run()
        {
            return true;
        }

        public bool IsValid(string ukprn)
        {
            return true;
        }

        public bool IsValid(int ukprn)
        {
            return true;
        }
        public bool IsValid(long ukprn)
        {
            return true;
        }

        public SomeClass Something(string id)
        {
            return new SomeClass() { id = id, name = "somebody", type = 1 };
        }

    }

    public class SomeClass
    {
        public string id { get; set; }
        public string name { get; set; }

        public int type { get; set; }
    }
}
