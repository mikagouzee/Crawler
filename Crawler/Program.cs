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

        public static string folderToSave = "YOUR FOLDER";

        public static WebClient client = new WebClient();

        static void Main(string[] args)
        {
            var doc = web.Load(baseUrl);

            //var testValue = @"/13th%20Age/13th%20Age%20Monthly%20-%20Vol%201/01%20-%20Dragon%20Riding.pdf";

            FilterPath(doc);


            foreach (var item in endTargets)
            {
                //todo : copy file instead of writeline
                DownloadFile(item);
            }
            
            Console.WriteLine(endTargets.Count +" files downloaded");
            Console.ReadLine();

        }

        public static void DownloadFile(string item)
        {
            var fileName = item.Split('/').Last();

            int startindex = item.IndexOf(fileName);

            var localPath = item.Remove(startindex).Replace('/', '\\');

            var fullPath = folderToSave + localPath;

            Console.WriteLine(fullPath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            client.DownloadFile(baseUrl+item, (fullPath + fileName));
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
           return !path.EndsWith("/");
        }

    }

}