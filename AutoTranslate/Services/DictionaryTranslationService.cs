using AutoTranslate.Helpers;
using AutoTranslate.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace AutoTranslate.Services
{
    public class DictionaryTranslationService : IDictionaryTranslationService
    {
        private readonly ITranslationService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;

        public DictionaryTranslationService(ITranslationService textService, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, ILocalizationService localizationService)
        {
            _textService = textService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _localizationService = localizationService;
        }

        public void TranslateDictionaryItem(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<ILanguage> allLanguages)
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
                        UpdateDictionaryTranslations(subscriptionKey, uriBase, dictionaryItem, allLanguages, valueToTranslate, apiInstruction.OverwriteExistingValues);
                    }
                    _localizationService.Save(dictionaryItem);
                }
            }
        }

        public void UpdateDictionaryTranslations(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, IEnumerable<ILanguage> allLanguages, string valueToTranslate, bool overwriteExistingValue)
        {
            foreach (var translation in dictionaryItem.Translations)
            {
                var cultureToTranslateTo = allLanguages.FirstOrDefault(x => x.Id == translation.LanguageId).IsoCode;

                if (string.IsNullOrWhiteSpace(translation.Value) || overwriteExistingValue)
                {
                    var result = _textService.MakeTranslationRequestAsync(valueToTranslate, subscriptionKey, uriBase, new string[] { cultureToTranslateTo });
                    JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                    translation.Value = translatedValue.ToString();
                }
            }
        }

        public void AddDictionaryTranslationsForAllLanguages(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate)
        {
            _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, allLanguages.FirstOrDefault(x => x.Id == defaultLanguage), valueToTranslate);
            if (allLanguages != null && allLanguages.Any() && allLanguages.Count() > 1)
            {
                var otherLanguages = allLanguages.Where(x => x.Id != defaultLanguage);
                foreach (var language in otherLanguages)
                {
                    var result = _textService.MakeTranslationRequestAsync(valueToTranslate, subscriptionKey, uriBase, new string[] { language.IsoCode });
                    JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                    _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, language, translatedValue.ToString());
                }
            }
        }
    }
}
