﻿using AutoTranslate.Models;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace AutoTranslate.Services
{
    public interface IDictionaryTranslationService
    {
        void TranslateDictionaryItem(DictionaryApiInstruction apiInstruction, string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages);
        void UpdateDictionaryTranslations(DictionaryApiInstruction apiInstruction, string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate);
        void AddDictionaryTranslationsForAllLanguages(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, ILanguage defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate);
    }
}
