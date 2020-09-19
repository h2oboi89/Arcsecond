using Arcsecond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scratchpad
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new SequenceOf(
                new Parser[] { 
                    new StringParser("Hello, World!"),
                    new StringParser("Goodbye, World!")
                });

            var state = parser.Run("Hello, World!Goodbye, World!");
            state = parser.Run("oh noes!");
            state = parser.Run("");

            Console.ReadLine();
        }
    }
}
