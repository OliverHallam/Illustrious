using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLibrary
{
    public class TestClass
    {
        public static void Noop()
        {
        }

        public static void Hello()
        {
            Console.Write("Hello ");
        }

        public static void Main(string[] args)
        {
            Noop();
            Hello();
            Noop();
            Console.WriteLine("World.");
            Noop();
        }
    }
}
