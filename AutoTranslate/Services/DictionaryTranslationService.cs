﻿using AutoTranslate.Helpers;
using AutoTranslate.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AutoTranslate.Services
{
    public class DictionaryTranslationService : IDictionaryTranslationService
    {
        private readonly ITextTranslationService _textService;
        private readonly ILocalizationService _localizationService;

        public DictionaryTranslationService(ITextTranslationService textService, ILocalizationService localizationService)
        {
            _textService = textService;
            _localizationService = localizationService;
        }

        public void TranslateDictionaryItem(DictionaryApiInstruction apiInstruction, string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages)
        {
            if (dictionaryItem != null)
            {
                var valueToTranslate = dictionaryItem.Translations.FirstOrDefault(x => x.LanguageId == defaultLanguage.Id)?.Value;

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
                        UpdateDictionaryTranslations(apiInstruction, subscriptionKey, uriBase, dictionaryItem, defaultLanguage, allLanguages, valueToTranslate);
                    }
                    _localizationService.Save(dictionaryItem);
                }
            }
        }

        public void UpdateDictionaryTranslations(DictionaryApiInstruction apiInstruction, string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate)
        {
            var allLanguagesList = allLanguages.ToList();
            foreach (var translation in dictionaryItem.Translations)
            {
                var cultureToTranslateTo = allLanguagesList.FirstOrDefault(x => x.Id == translation.LanguageId)?.IsoCode;

                if (string.IsNullOrWhiteSpace(translation.Value) || apiInstruction.OverwriteExistingValues)
                {
                    var result = _textService.MakeTranslationRequestAsync(valueToTranslate, subscriptionKey, uriBase, new[] { cultureToTranslateTo }, defaultLanguage.IsoCode);
                    JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                    translation.Value = translatedValue.ToString();
                }
            }
        }

        public void AddDictionaryTranslationsForAllLanguages(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate)
        {
            var allLanguagesList = allLanguages.ToList();
            _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, allLanguagesList.FirstOrDefault(x => x.Id == defaultLanguage.Id), valueToTranslate);
            if (allLanguagesList.Any() && allLanguagesList.Count() > 1)
            {
                var otherLanguages = allLanguagesList.Where(x => x.Id != defaultLanguage.Id);
                foreach (var language in otherLanguages)
                {
                    var result = _textService.MakeTranslationRequestAsync(valueToTranslate, subscriptionKey, uriBase, new[] { language.IsoCode }, defaultLanguage.IsoCode);
                    JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                    _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, language, translatedValue.ToString());
                }
            }
        }
    }
}
