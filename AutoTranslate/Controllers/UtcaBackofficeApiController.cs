using System.Configuration;
using System.Threading.Tasks;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using AutoTranslate.Services;
using Umbraco.Core.Services;
using Umbraco.Web;
using Newtonsoft.Json.Linq;

namespace AutoTranslate.Controllers
{
    [PluginController("AutoTranslate")]
    public class UtcaBackofficeApiController : UmbracoAuthorizedJsonController
    {
        private readonly ITextService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;

        public UtcaBackofficeApiController(ITextService textService, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, ILocalizationService localizationService)
        {
            _textService = textService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _localizationService = localizationService;
        }

        [System.Web.Http.HttpPost]
        public bool GetTranslatedText(ApiInstruction apiInstruction)
        {
            string subscriptionKey = ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
            string uriBase = ConfigurationManager.AppSettings["AzureTranslateApiUrl"];

            int[] contentIds = new int[] { apiInstruction.NodeId };

            foreach(int id in contentIds)
            {
                var defaultLanguageCode = _localizationService.GetDefaultLanguageIsoCode();
                TranslateContentItem(apiInstruction.CurrentCulture, subscriptionKey, uriBase, id, defaultLanguageCode);
            }
            return true;
        }

        private void TranslateContentItem(string cultureToTranslateTo, string subscriptionKey, string uriBase, int id, string defaultLanguageCode)
        {
            var content = _contentService.GetById(id);
            foreach (var property in content.Properties)
            {
                TranslateProperty(cultureToTranslateTo, subscriptionKey, uriBase, defaultLanguageCode, content, property);
            }
            _contentService.Save(content);
        }

        private void TranslateProperty(string cultureToTranslateTo, string subscriptionKey, string uriBase, string defaultLanguageCode, Umbraco.Core.Models.IContent content, Umbraco.Core.Models.Property property)
        {
            var currentValue = content.GetValue<string>(property.Alias, cultureToTranslateTo);
            var propertyValue = content.GetValue<string>(property.Alias, defaultLanguageCode);
            if (!string.IsNullOrWhiteSpace(propertyValue))
            {
                var result = _textService.MakeTextRequestAsync(propertyValue, subscriptionKey, uriBase, new string[] { cultureToTranslateTo });
                JToken translatedValue = GetTranslatedValue(result);
                content.SetValue(property.Alias, translatedValue, cultureToTranslateTo);
            }
        }

        private static JToken GetTranslatedValue(Task<string> result)
        {
            JArray translationRequest = JArray.Parse(result.Result);
            var translatedValue = translationRequest.First["translations"].First["text"];
            return translatedValue;
        }

        public class ApiInstruction
        {
            public int NodeId { get; set; }
            public string CurrentCulture { get; set; }
            public bool OverwriteExistingValues { get; set; }
            public bool IncludeDescendants { get; set; }
        }
    }
}