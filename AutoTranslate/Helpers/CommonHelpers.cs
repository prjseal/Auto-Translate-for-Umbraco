using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace AutoTranslate.Helpers
{
    public static class CommonHelpers
    {
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
