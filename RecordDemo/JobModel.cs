using Newtonsoft.Json;

namespace RecordDemo
{
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
