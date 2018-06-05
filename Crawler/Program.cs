using CsQuery;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Test_Console
{
    class Program
    {
        public static List<string> endTargets = new List<string>();
        public static HtmlWeb web = new HtmlWeb();
        const string baseUrl = "https://rpg.rem.uz";

        const string folderToSave = "YOUR FOLDER";

        static void Main(string[] args)
        {
            var BuildPath = new List<string>();
                        
            var doc = web.Load(baseUrl);

            FilterPath(doc);

            WebClient client = new WebClient();

            foreach (var item in endTargets)
            {
                Console.WriteLine(baseUrl + item);
                //todo : copy file instead of writeline
                //client.DownloadFile(baseUrl + item, folderToSave);
            }

            Console.WriteLine(endTargets.Count +" files downloaded");
            Console.ReadLine();

        }

        public static void FilterPath(HtmlDocument doc)
        {
            var target = doc.DocumentNode.SelectNodes("//td/a");

            foreach (var item in target)
            {
                var href = item.Attributes["href"].Value;

                if (href == "..")
                    continue;

                if (IsEndPath(href))
                {
                    if (!endTargets.Contains(href))
                    {
                        endTargets.Add(href);
                        continue;
                    }
                }

                var uri = baseUrl + href;
                var newDoc = web.Load(uri);

                FilterPath(newDoc);

            }
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