using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDotNetFrameworkConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            int timesToRun = int.Parse(args[0]);
            int count = 1;
            do
            {
                Console.WriteLine($"Running {count}/{timesToRun}");
                Task.Delay(1000).Wait();
            } while (++count <= timesToRun);
            Console.WriteLine("Complete");
        }
    }
}
