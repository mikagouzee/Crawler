using CsQuery;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Test_Console
{
    class Program
    {
        public static List<string> endTargets = new List<string>();

        static void Main(string[] args)
        {
            string baseUrl = "https://rpg.rem.uz";
          
            //string urlModified = baseUrl + "/WOIN/";

            var web = new HtmlWeb();
            var doc = web.Load(baseUrl);
            var firstHtml = doc.DocumentNode.InnerHtml;
            //var docModified = web.Load(urlModified);

            //bool isSamePage = (doc.DocumentNode.InnerHtml == docModified.DocumentNode.InnerHtml);

            //Console.WriteLine(isSamePage);

            var target = doc.DocumentNode.SelectNodes("//td/a");

            var BuildPath = new List<string>();
            BuildPath = ExtractPath(target, BuildPath);

            foreach (var item in BuildPath)
            {
                string page = baseUrl + item;
                doc = web.Load(page);

                var truc = doc.DocumentNode.InnerHtml;
                if (truc == firstHtml)
                {
                    Console.WriteLine("Same page loaded");
                    continue;
                }                

                target = doc.DocumentNode.SelectNodes("//td/a");
                var newList = ExtractPath(target, BuildPath);
            }

            foreach (var item in endTargets)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();

            #region httpclient
            //HttpClient client = new HttpClient();

            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AcmeInc/1.0)");

            //var response = client.GetAsync(baseUrl).Result;

            //var html = response.Content.ReadAsStringAsync().Result;
            #endregion

            #region test with CsQuery
            //Console.WriteLine(html);

            //var dom = CQ.Create(html);

            ////Console.WriteLine(dom.Render());

            //var mainrow = dom["#mainrow"];

            //Console.WriteLine(mainrow);

            //var content = mainrow["#content"];

            //Console.WriteLine(content);
            #endregion
        }

        public static List<string> ExtractPath(HtmlNodeCollection target, List<string> BuildPath)
        {
            List<string> SecondLevels = new List<string>();

            foreach (var item in target)
            {
                var href = item.Attributes["href"].Value;
                
                //Console.WriteLine(item.Attributes["href"].Value);

                if (!IsEndPath(href) && !BuildPath.Contains(href) && (href != ".."))
                {
                    SecondLevels.Add(href);
                }

                if(IsEndPath(href) && !endTargets.Contains(href))
                {
                    endTargets.Add(href);
                }
            }
            return SecondLevels;
        }

        public static bool IsEndPath(string path)
        {
            bool result = false;

            if (!path.EndsWith("/"))
                result = true;

            return result;
        }

    }

}