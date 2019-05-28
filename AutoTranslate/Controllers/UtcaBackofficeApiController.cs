using System.Configuration;
using System.Threading.Tasks;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using AutoTranslate.Services;
using Umbraco.Core.Services;
using Umbraco.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
            var defaultLanguageCode = _localizationService.GetDefaultLanguageIsoCode();

            List<int> contentIds = new List<int>() { apiInstruction.NodeId };
            if (apiInstruction.IncludeDescendants)
            {
                contentIds.AddRange(GetContentIdsFromDescendants(apiInstruction.NodeId));

            }


            foreach (int id in contentIds)
            {
                TranslateContentItem(apiInstruction.CurrentCulture, subscriptionKey, uriBase, id, defaultLanguageCode);
            }
            return true;
        }



        [System.Web.Http.HttpPost]
        public bool TranslateDictionaryItems(ApiInstruction apiInstruction)
        {
            string subscriptionKey = ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
            string uriBase = ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
            var defaultLanguageCode = _localizationService.GetDefaultLanguageIsoCode();

            var dictionaryItem = _localizationService.GetDictionaryItemById(apiInstruction.NodeId);
            var defaultLanguage = _localizationService.GetLanguageIdByIsoCode(defaultLanguageCode);
            var allLanguages = _localizationService.GetAllLanguages();

            if(dictionaryItem != null)
            {
                var valueToTranslate = dictionaryItem.Translations.FirstOrDefault(x => x.LanguageId == defaultLanguage.Value)?.Value;
                
                if(valueToTranslate == null || (string.IsNullOrEmpty(valueToTranslate) && apiInstruction.FallbackToKey))
                {
                    valueToTranslate = dictionaryItem.ItemKey;
                }

                if(valueToTranslate != null && !string.IsNullOrWhiteSpace(valueToTranslate))
                {
                    if(dictionaryItem.Translations == null || !dictionaryItem.Translations.Any())
                    {
                        AddDictionaryTranslationsForAllLanguages(subscriptionKey, uriBase, dictionaryItem, defaultLanguage, allLanguages, valueToTranslate);
                    }
                    else
                    {
                        UpdateDictionaryTranslations(subscriptionKey, uriBase, dictionaryItem, allLanguages, valueToTranslate);
                    }
                    _localizationService.Save(dictionaryItem);

                }
            }
            return true;
        }

        private void UpdateDictionaryTranslations(string subscriptionKey, string uriBase, Umbraco.Core.Models.IDictionaryItem dictionaryItem, IEnumerable<Umbraco.Core.Models.ILanguage> allLanguages, string valueToTranslate)
        {
            foreach (var translation in dictionaryItem.Translations)
            {
                var cultureToTranslateTo = allLanguages.FirstOrDefault(x => x.Id == translation.LanguageId).IsoCode;

                if (string.IsNullOrWhiteSpace(translation.Value))
                {
                    var result = _textService.MakeTextRequestAsync(valueToTranslate, subscriptionKey, uriBase, new string[] { cultureToTranslateTo });
                    JToken translatedValue = GetTranslatedValue(result);
                    translation.Value = translatedValue.ToString();
                }
            }
        }

        private void AddDictionaryTranslationsForAllLanguages(string subscriptionKey, string uriBase, Umbraco.Core.Models.IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<Umbraco.Core.Models.ILanguage> allLanguages, string valueToTranslate)
        {
            _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, allLanguages.FirstOrDefault(x => x.Id == defaultLanguage), valueToTranslate);
            if (allLanguages != null && allLanguages.Any() && allLanguages.Count() > 1)
            {
                var otherLanguages = allLanguages.Where(x => x.Id != defaultLanguage);
                foreach (var language in otherLanguages)
                {
                    var result = _textService.MakeTextRequestAsync(valueToTranslate, subscriptionKey, uriBase, new string[] { language.IsoCode });
                    JToken translatedValue = GetTranslatedValue(result);
                    _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, language, translatedValue.ToString());
                }
            }
        }

        private List<int> GetContentIdsFromDescendants(int nodeId)
        {
            List<int> contentIds = new List<int>();

            var content = _contentService.GetById(nodeId);
            int pageIndex = 0;
            int pageSize = 10;
            long totalRecords = 0;
            var descendants = _contentService.GetPagedDescendants(nodeId, pageIndex, pageSize, out totalRecords);
            contentIds.AddRange(descendants.Select(x => x.Id));
            if (totalRecords > pageSize)
            {
                pageIndex++;
                while (totalRecords >= pageSize * pageIndex + 1)
                {
                    descendants = _contentService.GetPagedDescendants(nodeId, pageIndex, pageSize, out totalRecords);
                    contentIds.AddRange(descendants.Select(x => x.Id));
                    pageIndex++;
                }
            }
            return contentIds;
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
            public bool FallbackToKey { get; set; }
        }
    }
}