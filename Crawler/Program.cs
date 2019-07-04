using CsQuery;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Test_Console
{
    class Program
    {
        public static List<string> endTargets = new List<string>();
        //public static List<string> folder = new List<string>();
        public static List<Task> listOfTasks = new List<Task>();

        public static HtmlWeb web = new HtmlWeb();
        public static string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

        public static string logFile = ConfigurationManager.AppSettings["logFolder"];

        public static string folderToSave = ConfigurationManager.AppSettings["SaveFolder"];

        public static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            client.Timeout = TimeSpan.FromMinutes(15);

            string localPath, fullPath;
            int startIndex;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var doc = web.Load(baseUrl);
            Console.WriteLine(doc.DocumentNode.InnerHtml.ToString());
            FilterPath(doc).Wait();
            //ItemFormat = @"/13th%20Age/13th%20Age%20Monthly%20-%20Vol%201/01%20-%20Dragon%20Riding.pdf";

            //#region short test list
            //endTargets.Add("Achtung! Cthulhu Skirmish - Campaign - Rise of the Black Sun.pdf");
            //endTargets.Add("Achtung! Cthulhu Skirmish - Campaign - Secret War Operations.pdf");
            //endTargets.Add("Achtung! Cthulhu Skirmish - Notoriety Trackers.pdf");
            //#endregion

            //download each files
            //Parallel.ForEach<string>(endTargets, item =>
            //{
            //    var task = client.GetAsync(baseUrl + item)
            //        .ContinueWith(async (x) =>
            //        {
            //            var fileName = item.Split('/').Last().Replace("%20%", " ");
            //            startIndex = item.IndexOf(fileName);
            //            localPath = item.Remove(startIndex).Replace('/', '\\');
            //            fullPath = folderToSave + localPath;
            //            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //            var streamDest = File.Create(fullPath + fileName);
            //            Console.WriteLine("Downloading "+fileName);
            //            await x.Result.Content.CopyToAsync(streamDest);

            //        });

            //    listOfTasks.Add(task);// client.GetAsync(baseUrl + item));
            //});

            //Task.WaitAll(listOfTasks.ToArray());

            Console.WriteLine(endTargets.Count + " files have been found");
            Console.ReadLine();

        }


        // To rewrite, struct is different in trove.net
        public static Task FilterPath(HtmlDocument doc)
        {
            //this method will render the doc with Xpath, and search for the links in a table
            //each link found will be parsed to find out if it's a file or another link
            //sublinks are processed again, while fileLinks are added to "endTargets"

            //Console.WriteLine(doc.DocumentNode.InnerHtml);

            var tasks = new List<Task>();
            var target = doc.DocumentNode.SelectNodes("//td/a");

            if (target != null)
            {
                //foreach (var item in target)
                Parallel.ForEach<HtmlNode>(target, item =>
                {
                    var href = item.Attributes["href"].Value;

                    if (href != "../index.html") //goes back to higher level, we need to avoid those
                    {
                        if (!IsEndPath(href))
                        {

                            var uri = baseUrl + href;
                            //Console.WriteLine($"Working on {uri}");
                            var newDoc = web.Load(uri);
                            tasks.Add(FilterPath(newDoc));
                        }
                        else
                        {
                            if (!endTargets.Contains(href))
                            {
                                Console.WriteLine("Found path to file : " + CleanName(href));
                                endTargets.Add(href);
                            }
                        }
                    }
                });

            }

            return Task.WhenAll(tasks);
        }

        public static bool IsEndPath(string path)
        {
            return path.EndsWith(".pdf");
        }

        public static string CleanName(string name)
        {
            return name.Split('/').Last().Replace("%20%26%20", "&").Replace("%20", " ").Replace("%27", "'").Replace("%29", "\"");
        }

    }

}