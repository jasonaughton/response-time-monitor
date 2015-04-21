// <copyright file="RequestRunner.cs" company="Bauer Consumer Media">
//     Copyright (c) 2015 Bauer Consumer Media. All rights reserved.
// </copyright>

namespace ResponseTimeMonitor
{
    #region Usings

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    #endregion

    /// <summary>
    /// Handles the running of requests
    /// </summary>
    public sealed class RequestRunner
    {
        #region Constants

        /// <summary>
        /// The requests per URL
        /// </summary>
        private const int RequestsPerUrl = 10;

        #endregion

        #region Fields

        /// <summary>
        /// The request urls
        /// </summary>
        private readonly List<string> requestUrls = new List<string>();

        /// <summary>
        /// The response times
        /// </summary>
        private readonly ConcurrentBag<KeyValuePair<string, long>> responseTimes = new ConcurrentBag<KeyValuePair<string, long>>();

        /// <summary>
        /// The environment
        /// </summary>
        private string environment = "local";

        #endregion

        #region Methods

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute(IEnumerable<string> args)
        {
            if (args.Any())
            {
                this.environment = args.First();
            }

            this.LoadUrls();
            this.ShuffleUrls();
            this.RunRequests();
            this.WriteResults();
        }

        /// <summary>
        /// Writes the results.
        /// </summary>
        private void WriteResults()
        {
            IOrderedEnumerable<KeyValuePair<string, long>> sortedResults = this.responseTimes.OrderBy(item => item.Key);;
            string fileName = this.environment + "-result-" + Guid.NewGuid() + ".csv";

            using (StreamWriter writer = File.CreateText(Path.Combine(@"C:\temp\responsemonitor\", fileName)))
            {
                int urlIndex = 0;
                long runningTotal = 0;

                foreach (var result in sortedResults)
                {
                    urlIndex++;
                    runningTotal += result.Value;

                    if (urlIndex == RequestsPerUrl)
                    {
                        long mean = runningTotal / RequestsPerUrl;
                        writer.WriteLine(String.Concat(result.Key, ",", result.Value, ", ", mean));
                        runningTotal = 0;
                        urlIndex = 0;
                    }
                    else
                    {
                        writer.WriteLine(String.Concat(result.Key, ",", result.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Shuffles the urls.
        /// </summary>
        private void ShuffleUrls()
        {
            Random random = new Random();
            int length = this.requestUrls.Count;

            while (length > 1)
            {
                length--;
                int randomNumber = random.Next(length + 1);
                string value = this.requestUrls[randomNumber];
                this.requestUrls[randomNumber] = this.requestUrls[length];
                this.requestUrls[length] = value;
            }
        }

        /// <summary>
        /// Runs the requests.
        /// </summary>
        private void RunRequests()
        {
            //Console.WriteLine("Parallel");
            //Parallel.ForEach(
            //    this.requestUrls,
            //    this.ExecuteRequest);

            Console.WriteLine("Serial");
            foreach (var url in this.requestUrls)
            {
                this.ExecuteRequest(url);
            }
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="url">The URL.</param>
        private void ExecuteRequest(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    client.DownloadData(url);
                    timer.Stop();
                    Console.WriteLine(url + ": " + timer.ElapsedMilliseconds);
                    this.responseTimes.Add(new KeyValuePair<string, long>(url, timer.ElapsedMilliseconds));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// Loads the urls.
        /// </summary>
        /// <returns>dynamic.</returns>
        private void LoadUrls()
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(this.environment + ".json"));

            foreach (var url in data.Urls)
            {
                for (int index = 0; index < RequestsPerUrl; index++)
                {
                    this.requestUrls.Add((string)url);
                }
            }
        }

        #endregion
    }
}
