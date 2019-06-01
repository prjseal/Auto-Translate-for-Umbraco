using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoTranslate.Services
{
    public class AzureTranslationService : ITextTranslationService
    {
        public async Task<string> MakeTranslationRequestAsync(string textToTranslate, string subscriptionKey, string uriBase, string[] languages, string translateFrom)
        {
            try
            {
                string languageString = "";

                if(!string.IsNullOrWhiteSpace(translateFrom))
                {
                    languageString += "&from=" + translateFrom;
                }

                foreach(var language in languages)
                {
                    languageString += "&to=" + language;
                }

                object[] body = new object[] { new { Text = textToTranslate } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(uriBase + languageString);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                    var response = client.SendAsync(request).Result;
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static JToken GetTranslatedValue(Task<string> result)
        {
            JArray translationRequest = JArray.Parse(result.Result);
            var translatedValue = translationRequest.First["translations"].First["text"];
            return translatedValue;
        }

        public static JToken GetTranslatedValue(Task<string> result, string culture)
        {
            JArray translationRequest = JArray.Parse(result.Result);
            var translatedValue = translationRequest.First["translations"].First["text"];
            return translatedValue;
        }
    }
}