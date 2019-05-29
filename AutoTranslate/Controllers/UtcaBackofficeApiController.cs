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
using Umbraco.Core.Models;

namespace AutoTranslate.Controllers
{
    [PluginController("AutoTranslate")]
    public class UtcaBackofficeApiController : UmbracoAuthorizedJsonController
    {
        private readonly ITextService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;

        private string _subscriptionKey => ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
        private string _uriBase => ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
        private string _defaultLanguageCode => _localizationService.GetDefaultLanguageIsoCode();

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
            var content = _contentService.GetById(apiInstruction.NodeId);

            if(content != null)
            {
                TranslateContentItem(apiInstruction.CurrentCulture, _subscriptionKey, _uriBase, content, _defaultLanguageCode);

                if (apiInstruction.IncludeDescendants)
                {
                    int pageIndex = 0;
                    int pageSize = 10;
                    long totalRecords = 0;
                    totalRecords = TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, _defaultLanguageCode, pageIndex, pageSize);

                    if (totalRecords > pageSize)
                    {
                        pageIndex++;
                        while (totalRecords >= pageSize * pageIndex + 1)
                        {
                            totalRecords = TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, _defaultLanguageCode, pageIndex, pageSize);
                            pageIndex++;
                        }
                    }
                }
            }

            return true;
        }

        private long TranslatePageOfContentItems(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, string defaultLanguageCode, int pageIndex, int pageSize)
        {
            long totalRecords;
            var descendants = _contentService.GetPagedDescendants(apiInstruction.NodeId, pageIndex, pageSize, out totalRecords);
            foreach (var contentItem in descendants)
            {
                TranslateContentItem(apiInstruction.CurrentCulture, subscriptionKey, uriBase, contentItem, defaultLanguageCode);
            }

            return totalRecords;
        }

        [System.Web.Http.HttpPost]
        public bool TranslateDictionaryItems(ApiInstruction apiInstruction)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(apiInstruction.NodeId);
            var defaultLanguage = _localizationService.GetLanguageIdByIsoCode(_defaultLanguageCode);
            var allLanguages = _localizationService.GetAllLanguages();

            TranslateDictionaryItem(apiInstruction, _subscriptionKey, _uriBase, dictionaryItem, defaultLanguage, allLanguages);

            if (apiInstruction.IncludeDescendants)
            {
                var dictionaryDescendants = _localizationService.GetDictionaryItemDescendants(dictionaryItem.Key);
                if(dictionaryDescendants != null && dictionaryDescendants.Any())
                {
                    foreach(var descendantDictionaryItem in dictionaryDescendants)
                    {
                        TranslateDictionaryItem(apiInstruction, _subscriptionKey, _uriBase, descendantDictionaryItem, defaultLanguage, allLanguages);
                    }
                }
            }
            
            return true;
        }

        private void TranslateDictionaryItem(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, Umbraco.Core.Models.IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<Umbraco.Core.Models.ILanguage> allLanguages)
        {
            if (dictionaryItem != null)
            {
                var valueToTranslate = dictionaryItem.Translations.FirstOrDefault(x => x.LanguageId == defaultLanguage.Value)?.Value;

                if (valueToTranslate == null || (string.IsNullOrEmpty(valueToTranslate) && apiInstruction.FallbackToKey))
                {
                    valueToTranslate = dictionaryItem.ItemKey;
                }

                if (valueToTranslate != null && !string.IsNullOrWhiteSpace(valueToTranslate))
                {
                    if (dictionaryItem.Translations == null || !dictionaryItem.Translations.Any())
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

        private void TranslateContentItem(string cultureToTranslateTo, string subscriptionKey, string uriBase, IContent content, string defaultLanguageCode)
        {
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

        private static JToken GetTranslatedValue(Task<string> result, string culture)
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