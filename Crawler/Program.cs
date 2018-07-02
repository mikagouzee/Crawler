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
        public static List<Task> listOfTasks = new List<Task>();
        public static HtmlWeb web = new HtmlWeb();
        public static string baseUrl = ConfigurationManager.AppSettings["baseUrl"];
        public static string listFile = ConfigurationManager.AppSettings["LogFolder"];


        public static string folderToSave = ConfigurationManager.AppSettings["SaveFolder"];

        public static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {

            //ItemFormatExample = @"/13th%20Age/13th%20Age%20Monthly%20-%20Vol%201/01%20-%20Dragon%20Riding.pdf";

            client.Timeout = TimeSpan.FromMinutes(15);
            string localPath, fullPath;
            int startIndex;

            var doc = web.Load(baseUrl);


            if (!File.Exists(@listFile))
            {
                File.Create(@listFile);
            }


            #region short test list
            endTargets = new List<string>();
            endTargets.Add("/13th%20Age/13%20True%20Ways.pdf");
            endTargets.Add("/13th%20Age/13th%20Age%20-%20Map.pdf");
            endTargets.Add("/13th%20Age/13th%20Age%20Bestiary%202%20-%20Lions%20%26%20Tigers%20%26%20Owlbears.pdf");
            #endregion


            using (StreamWriter file = new StreamWriter(listFile, true))
            {
                foreach (var item in endTargets)
                {
                    file.WriteLine(item);
                }
            }

            #region  download each files
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

            //              if (!File.Exists(fullPath + fileName))
            //              {
            //            var streamDest = File.Create(fullPath + fileName);
            //            Console.WriteLine("Downloading " + fileName);
            //            await x.Result.Content.CopyToAsync(streamDest);
            //              }
            //        });

            //    listOfTasks.Add(task);// client.GetAsync(baseUrl + item));
            //});
            #endregion

            Task.WaitAll(listOfTasks.ToArray());

            Console.WriteLine(endTargets.Count + " files should have been downloaded");
            Console.ReadLine();

        }

        public static Task FilterPath(HtmlDocument doc)
        {
            //this method will render the doc with Xpath, and search for the links in a table
            //each link found will be parsed to find out if it's a file or another link
            //sublinks are processed again, while fileLinks are added to "endTargets"

            var tasks = new List<Task>();
            var target = doc.DocumentNode.SelectNodes("//td/a");

            Parallel.ForEach<HtmlNode>(target, item =>
            {
                var href = item.Attributes["href"].Value;

                if (href != "..") //goes back to higher level, we need to avoid those
                {
                    if (!IsEndPath(href))
                    {
                        var uri = baseUrl + href;
                        var newDoc = web.Load(uri);
                        tasks.Add(FilterPath(newDoc));
                    }
                    else
                    {
                        if (!endTargets.Contains(href))
                        {
                            Console.WriteLine("Found path to file : " + href.Replace("%20%", " "));
                            endTargets.Add(href);
                        }
                    }
                }
            });

            return Task.WhenAll(tasks);
        }

        public static bool IsEndPath(string path)
        {
            return !path.EndsWith("/");
        }

    }

}