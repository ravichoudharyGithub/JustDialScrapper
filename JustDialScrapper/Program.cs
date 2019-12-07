using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JustDialScrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Processing...");
            using (var justDial = new LoadJustDialData())
            {
                justDial.ProcessForJustDialData();
            }
            Console.WriteLine("End Processing...");
        }
    }
}
