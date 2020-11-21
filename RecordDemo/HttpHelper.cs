using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RecordDemo
{
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
}
