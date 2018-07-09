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

            #region short test list
            //endTargets = new List<string>();
            //endTargets.Add("/13th%20Age/13%20True%20Ways.pdf");
            //endTargets.Add("/13th%20Age/13th%20Age%20-%20Map.pdf");
            //endTargets.Add("/13th%20Age/13th%20Age%20Bestiary%202%20-%20Lions%20%26%20Tigers%20%26%20Owlbears.pdf");
            #endregion
            client.Timeout = TimeSpan.FromMinutes(15);
            //ensure the list-file exists
            if (!File.Exists(@listFile))
            {
                File.Create(@listFile);
            }

            try
            {
                var doc = web.Load(baseUrl);
                FilterPath(doc);
                DownloadFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine); 
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Exception : " + ex.Message);
                Console.WriteLine("Stack  : " + ex.StackTrace);
            }

            Task[] tasksArray = listOfTasks.Where(t => t != null).ToArray();
            if (tasksArray.Length > 0) Task.WaitAll(tasksArray);

            //Task.WaitAll(listOfTasks.ToArray());

            Console.WriteLine(endTargets.Count + " files have been identified");


            Console.ReadLine();

        }

        //this method will render the doc with Xpath, and search for the links in a table
        //each link found will be parsed to find out if it's a file or another link
        //sublinks are processed again, while fileLinks are added to a list.
        public static void FilterPath(HtmlDocument doc)
        {
            //var tasks = new List<Task>();
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
                            FilterPath(newDoc);
                    }
                    else
                    {
                        if (!endTargets.Contains(href))
                        {
                            //file.WriteLine(href);
                            Console.WriteLine("Adding path "+CleanName(href));
                            endTargets.Add(href);
                        }
                    }
                }
            });
        }

        private static void DownloadFiles()
        {
            int startIndex;
            string partialPath, localFolderPath;

            while (endTargets.Count > 0)
            {
                listOfTasks = new List<Task>();

                List<string> currentTargets = endTargets.GetRange(0,50);

                endTargets.RemoveRange(0, 50);

                Parallel.ForEach<string>(currentTargets, item =>
                {
                    var task = client.GetAsync(baseUrl + item)
                        .ContinueWith(async (x) =>
                        {
                            var fileName = item.Split('/').Last();
                            startIndex = item.IndexOf(fileName);
                            partialPath = item.Remove(startIndex).Replace('/', '\\');
                            localFolderPath = folderToSave + CleanName(partialPath);
                            Directory.CreateDirectory(Path.GetDirectoryName(localFolderPath));

                            if (!File.Exists(localFolderPath + fileName))
                            {
                                var streamDest = File.Create(localFolderPath + CleanName(fileName));
                                Console.WriteLine("Downloading " + CleanName(fileName));
                                var pdfFile = x.Result.Content.ReadAsStreamAsync().Result;

                                using (streamDest)
                                {
                                    pdfFile.CopyTo(streamDest);
                                }

                            }
                        });
                    if (task != null)
                    {
                        listOfTasks.Add(task);
                    }
                });

                Task[] tasksArray = listOfTasks.Where(t => t != null).ToArray();
                if (tasksArray.Length > 0) Task.WaitAll(tasksArray);
            }
        }

        #region attempt to find files with size 0k
        //static void DirSearch(string sDir)
        //{
        //    foreach (var dir in Directory.GetDirectories(sDir))
        //    {
        //        foreach (var file in Directory.GetFiles(dir))
        //        {
        //            var length = new FileInfo(file).Length;

        //            if (length == 0)

        //        }
        //        DirSearch(dir);
        //    }
        //}
        #endregion  

        public static bool IsEndPath(string path)
        {
            return !path.EndsWith("/");
        }

        public static string CleanName(string name)
        {
            return name.Split('/').Last().Replace("%20%26%20", "&").Replace("%20", " ").Replace("%27", "'").Replace("%29", "\"");
        }

    }

}