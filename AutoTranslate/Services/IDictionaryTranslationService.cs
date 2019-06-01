using AutoTranslate.Models;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace AutoTranslate.Services
{
    public interface IDictionaryTranslationService
    {
        void TranslateDictionaryItem(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, Umbraco.Core.Models.IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<ILanguage> allLanguages);
        void UpdateDictionaryTranslations(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, IEnumerable<ILanguage> allLanguages, string valueToTranslate, bool overwriteExistingValue);
        void AddDictionaryTranslationsForAllLanguages(string subscriptionKey, string uriBase, IDictionaryItem dictionaryItem, int? defaultLanguage, IEnumerable<ILanguage> allLanguages, string valueToTranslate);
    }
}
