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

        public static void FunctionCall()
        {
            Console.Write("Hello");
        }

        public static void Loop()
        {
            var sum = 0;
            for (var i = 0; i < 10; i++)
            {
                sum += i;
            }
            Console.WriteLine(sum);
        }

        public static void Branch()
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    Console.WriteLine("Weekend");
                    return;
                
                default:
                    Console.WriteLine("Weekday");
                    return;
            }
        }

        public static void Main(string[] args)
        {
            Noop();
            FunctionCall();
            //Loop();
            Branch();
        }
    }
}
