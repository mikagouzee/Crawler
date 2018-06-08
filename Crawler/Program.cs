using CsQuery;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
        const string baseUrl = "https://rpg.rem.uz";

        public static string folderToSave = @"C:\Users\mgouzee\Desktop\Crawler\Crawler\Downloaded";

        public static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            var doc = web.Load(baseUrl);

            client.Timeout = TimeSpan.FromMinutes(15);
            //var testValue = @"/13th%20Age/13th%20Age%20Monthly%20-%20Vol%201/01%20-%20Dragon%20Riding.pdf";

            //FilterPath(doc);

            string localPath, fullPath;
            int startIndex;

            endTargets = new List<string>();
            endTargets.Add("/13th%20Age/13%20True%20Ways.pdf");
            endTargets.Add("/13th%20Age/13th%20Age%20-%20Map.pdf");
            endTargets.Add("/13th%20Age/13th%20Age%20Bestiary%202%20-%20Lions%20%26%20Tigers%20%26%20Owlbears.pdf");


            Parallel.ForEach<string>(endTargets, item =>
            //foreach(var item in endTargets)
            {
                var task = client.GetAsync(baseUrl + item)
                    .ContinueWith(async (x) =>
                    {
                        var fileName = item.Split('/').Last();
                        startIndex = item.IndexOf(fileName);
                        localPath = item.Remove(startIndex).Replace('/', '\\');
                        fullPath = folderToSave + localPath;
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                        var streamDest = File.Create(fileName);

                        await x.Result.Content.CopyToAsync(streamDest);

                    });

                listOfTasks.Add(task);// client.GetAsync(baseUrl + item));
            });

            Task.WaitAll(listOfTasks.ToArray());

            Console.WriteLine(endTargets.Count + " files should have been downloaded");
            Console.ReadLine();

        }

        static async Task DownloadFile(string item)
        {
            var fileName = item.Split('/').Last();

            int startindex = item.IndexOf(fileName);

            Console.WriteLine("downloading " + fileName);

            var localPath = item.Remove(startindex).Replace('/', '\\');

            var fullPath = folderToSave + localPath;

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            //return Task downloadFile = client.GetAsync(baseUrl + item).Result.Content.CopyToAsync(File.Create(fileName));

            //await downloadFile;

        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];

            int len;

            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public static void FilterPath(HtmlDocument doc)
        {
            var target = doc.DocumentNode.SelectNodes("//td/a");

            var list = new List<Task>();

            if (target == null)
                return;

            foreach (var item in target)
            {
                var href = item.Attributes["href"].Value;

                if (href != "..")
                {
                    if (IsEndPath(href))
                    {
                        if (!endTargets.Contains(href))
                        {
                            endTargets.Add(href);
                            continue;
                        }
                    }

                    var uri = baseUrl + href;
                    //var newDoc = web.Load(uri);
                    var task = web.LoadFromWebAsync(uri).ContinueWith((x) =>
                    {
                        FilterPath(x.Result);
                    });

                    list.Add(task);
                    Console.WriteLine(list.Count);
                }
            }

            Task.WaitAll(list.ToArray());
        }

        public static bool IsEndPath(string path)
        {
            return !path.EndsWith("/");
        }

    }

}