﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using static RecordDemo.HttpHelper;

namespace RecordDemo
{
   class Program
   {
      public static HttpClient client = new HttpClient();
      public static List<Task<HttpResponseMessage>> TaskList
      {
         get
         {
            return new List<Task<HttpResponseMessage>>()
            {
               client.GetAsync("https://jobs.github.com/positions.json?page=0"),
               client.GetAsync("https://jobs.github.com/positions.json?page=1"),
               client.GetAsync("https://jobs.github.com/positions.json?page=2"),
               client.GetAsync("https://jobs.github.com/positions.json?page=3"),
               client.GetAsync("https://jobs.github.com/positions.json?page=4"),
               client.GetAsync("https://jobs.github.com/positions/6efab349-9691-4a34-a6e6-96a7b544e60f.json"),
            };
         }
      }
      public static float TaskCompleteCount = 0;

      public static Task DoTaskInAnotherThread<TResult>(Task<HttpResponseMessage> task, Action<TResult> action)
      {
         return Task.Run(async () =>
         {
            var response = await task;
            var stream = await response.Content.ReadAsStreamAsync();
            if (response.IsSuccessStatusCode/* && typeof(TResult) != typeof(JobModel)*/)
            {
               action(DeserializeJsonFromStream<TResult>(stream));
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

            var per = (++TaskCompleteCount / TaskList.Count) * 100;
            Console.WriteLine("Loading {0:F2}%...", per);
         });
      }

      static void Main(string[] args)
      {
         try
         {
            Console.WriteLine("***Start program. Thread Id: {0}***", Thread.CurrentThread.ManagedThreadId);
            var jobList = new List<JobModel>();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Task.WhenAll(DoTaskInAnotherThread<List<JobModel>>(TaskList[0], d => jobList.AddRange(d))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[1], d => jobList.AddRange(d))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[2], d => jobList.AddRange(d))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[3], d => jobList.AddRange(d))
                        , DoTaskInAnotherThread<List<JobModel>>(TaskList[4], d => jobList.AddRange(d))
                        , DoTaskInAnotherThread<JobModel>(TaskList[5], d => jobList.Add(d)))
               .ContinueWith(t =>
            {
               Console.WriteLine("Total jobs loaded: {0}", jobList.Count);

               #region Stopwatch
               stopWatch.Stop();
               // Get the elapsed time as a TimeSpan value.
               TimeSpan ts = stopWatch.Elapsed;
               // Format and display the TimeSpan value.
               string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                   ts.Hours, ts.Minutes, ts.Seconds,
                   ts.Milliseconds / 10);
               Console.WriteLine("RunTime " + elapsedTime);
               #endregion
               //Console.Write("Press any key to exit...");
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            // capture exception in thread
            //Task.WhenAny(DoTaskInAnotherThread<List<JobModel>>(TaskList[0], d => jobList.AddRange(d))
            //            , DoTaskInAnotherThread<List<JobModel>>(TaskList[1], d => jobList.AddRange(d))
            //            , DoTaskInAnotherThread<List<JobModel>>(TaskList[2], d => jobList.AddRange(d))
            //            , DoTaskInAnotherThread<List<JobModel>>(TaskList[3], d => jobList.AddRange(d))
            //            , DoTaskInAnotherThread<List<JobModel>>(TaskList[4], d => jobList.AddRange(d))
            //            , DoTaskInAnotherThread<JobModel>(TaskList[5], d => jobList.Add(d))).ContinueWith(t =>
            //{
            //   Console.WriteLine("Some thing went wrong: {0}", t.Exception.Message);
            //}, TaskContinuationOptions.OnlyOnFaulted);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Some thing went wrong: {0}", ex.Message);
         }
         Console.ReadLine();
      }
   }
}
