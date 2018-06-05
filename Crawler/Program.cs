using CsQuery;
using System;
using System.Net.Http;

namespace Test_Console
{
    class Program
    {

        static void Main(string[] args)
        {
            string baseUrl = "https://rpg.rem.uz/";

            var dom = CQ.CreateFromUrl(baseUrl);

            string htmlTarget = dom["#view"].Render();

            Console.WriteLine(htmlTarget);

            Console.ReadLine();
        }
    }

}