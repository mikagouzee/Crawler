using CsQuery;
using HtmlAgilityPack;
using System;
using System.Net.Http;

namespace Test_Console
{
    class Program
    {

        static void Main(string[] args)
        {
            string baseUrl = "https://rpg.rem.uz/";

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AcmeInc/1.0)");

            var response = client.GetAsync(baseUrl).Result;

            var html = response.Content.ReadAsStringAsync().Result;

            #region test with CsQuery
            //Console.WriteLine(html);

            //var dom = CQ.Create(html);

            ////Console.WriteLine(dom.Render());

            //var mainrow = dom["#mainrow"];

            //Console.WriteLine(mainrow);

            //var content = mainrow["#content"];

            //Console.WriteLine(content);
            #endregion

            var web = new HtmlWeb();
            var doc = web.Load(baseUrl);
            var target = doc.DocumentNode.SelectNodes("//td/a");

            foreach (var item in target)
            {
                Console.WriteLine(item.Attributes["href"].Value);
            }

            Console.ReadLine();
        }
    }

}