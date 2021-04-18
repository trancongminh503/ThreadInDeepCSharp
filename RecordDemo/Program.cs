using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using static RecordDemo.HttpHelper;

namespace RecordDemo
{
    class Program
    {
        public static HttpClient Client = new HttpClient();
        public static List<Task<HttpResponseMessage>> TaskList
        {
            get
            {
                return new List<Task<HttpResponseMessage>>()
                {
                   Client.GetAsync("https://jobs.github.com/positions.json?page=0"),
                   Client.GetAsync("https://jobs.github.com/positions.json?page=1"),
                   Client.GetAsync("https://jobs.github.com/positions/6efab349-9691-4a34-a6e6-96a7b544e60f.json"),
                   Client.GetAsync("https://jobs.github.com/positions.json?page=2"),
                   Client.GetAsync("https://jobs.github.com/positions.json?page=3"),
                   Client.GetAsync("https://jobs.github.com/positions.json?page=4"),
                };
            }
        }
        public static float TaskCompleteCount = 0;

        public static Task<TResult> DoTaskInAnotherThread<TResult>(Task<HttpResponseMessage> task)
        {
            var getApiThreadTask = Task.Run(async () =>
            {
                var response = await task;
                var stream = await response.Content.ReadAsStreamAsync();
                if (response.IsSuccessStatusCode && typeof(TResult) != typeof(JobModel))
                {
                    var result = DeserializeJsonFromStream<TResult>(stream);
                    var per = (++TaskCompleteCount / TaskList.Count) * 100;
                    Console.WriteLine("Loading {0:F2}%...", per);
                    return result;
                }
                else
                {
                    var content = await StreamToStringAsync(stream);
                    throw new ApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content
                    };
                }
            });

            getApiThreadTask.ContinueWith(t =>
            {
                ColorConsole.WriteError(string.Format("ERROR: {0} to load {1}", t.Status, typeof(TResult)));
            }, TaskContinuationOptions.OnlyOnFaulted);

            return getApiThreadTask;
        }

        static void Main(string[] args)
        {
            ColorConsole.WriteWrappedHeader(string.Format(
               "***Start program. Thread Id: {0}***", Thread.CurrentThread.ManagedThreadId));
            var jobList = new List<JobModel>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Task[] tasks = { DoTaskInAnotherThread<List<JobModel>>(TaskList[0]).ContinueWith(d => jobList.AddRange(d.Result))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[1]).ContinueWith(d => jobList.AddRange(d.Result))
                        , DoTaskInAnotherThread<JobModel>(TaskList[2]).ContinueWith(d => jobList.Add(d.Result))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[3]).ContinueWith(d => jobList.AddRange(d.Result))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[4]).ContinueWith(d => jobList.AddRange(d.Result))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[5]).ContinueWith(d => jobList.AddRange(d.Result)) };

            Task.WhenAll(tasks).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted)
                {
                    ColorConsole.WriteError(string.Format("ERROR: One or more task is faulted."));
                }

                Console.WriteLine(">>> Total jobs loaded: {0}", jobList.Count);
                #region Stopwatch
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;
                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                 ts.Hours, ts.Minutes, ts.Seconds,
                 ts.Milliseconds / 10);
                Console.WriteLine(">>> RunTime " + elapsedTime);
                #endregion
                Console.Write("Press any key to exit...");
            });
            Console.ReadLine();
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
    }

    public class HttpHelper
    {
        public static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        public static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }

        public static async Task<T> DeserializeFromStreamCallAsync<T>(string url, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                    return DeserializeJsonFromStream<T>(stream);

                var content = await StreamToStringAsync(stream);
                throw new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
        }
    }

    public class JobModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("company_url")]
        public string CompanyUrl { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("how_to_apply")]
        public string HowToApply { get; set; }

        [JsonProperty("company_logo")]
        public string CompanyLogo { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }
}
