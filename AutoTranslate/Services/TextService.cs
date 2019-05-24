using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoTranslate.Services
{
    public class TextService : ITextService
    {
        public async Task<string> MakeTextRequestAsync(string textToTranslate, string subscriptionKey, string uriBase, string[] languages)
        {
            try
            {
                string languageString = "";
                foreach(var language in languages)
                {
                    languageString += "&to=" + language;
                }

                System.Object[] body = new System.Object[] { new { Text = textToTranslate } };
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
    }
}