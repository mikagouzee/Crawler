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
        //public static List<Task> listOfTasks = new List<Task>();
        public static HtmlWeb web = new HtmlWeb();
        const string baseUrl = "https://rpg.rem.uz";

        public static string folderToSave = "YOUR FOLDER";

        public static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            var doc = web.Load(baseUrl);

            client.Timeout = TimeSpan.FromMinutes(15);
            //var testValue = @"/13th%20Age/13th%20Age%20Monthly%20-%20Vol%201/01%20-%20Dragon%20Riding.pdf";

            FilterPath(doc);


            foreach (var item in endTargets)
            {
                DownloadFile(item);
            }
            
            Console.WriteLine(endTargets.Count +" files downloaded");
            Console.ReadLine();

            #region asyncprep
            //List<Stream> allStreams = await Task.WhenAll(listOfTasks);

            //foreach (var task in listOfTasks)
            //{

            //}

            #endregion

        }

        public static void DownloadFile(string item)
        {
            var fileName = item.Split('/').Last();

            int startindex = item.IndexOf(fileName);

            var localPath = item.Remove(startindex).Replace('/', '\\');

            var fullPath = folderToSave + localPath;
            
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            Stream downloadedFile = client.GetAsync(baseUrl + item).Result.Content.ReadAsStreamAsync().Result;
            
            using (Stream file = File.Create(fileName))
            {
                Console.WriteLine("downloading " + fileName);
                CopyStream(downloadedFile, file);
            }

            #region async preparation 
            //Task<Stream> downloadedFile = client.GetAsync(baseUrl + item).Result.Content.ReadAsStreamAsync();

            //listOfTasks.Add(downloadedFile);
            #endregion

        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];

            int len;

            while((len= input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
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