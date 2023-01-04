using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleHostTest
{
    public abstract class DemoBase
    {
        protected DemoBase()
        {
            Console.WriteLine("DemoBase::Called");
            Perform();
        }
        protected virtual void Perform()
        {
            Console.WriteLine("DemoBase::Perform Called");
        }
    }
    public class Demo : DemoBase
    {
        public Demo()
        {
            Console.WriteLine("Demo::Called");
        }
        protected override void Perform()
        {
            Console.WriteLine("Demo::Perform Called");
        }

    }
}
