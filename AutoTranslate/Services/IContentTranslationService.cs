using AutoTranslate.Models;
using Umbraco.Core.Models;

namespace AutoTranslate.Services
{
    public interface IContentTranslationService
    {
        long TranslatePageOfContentItems(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, string defaultLanguageCode, int pageIndex, int pageSize);
        void TranslateContentItem(string cultureToTranslateTo, string subscriptionKey, string uriBase, IContent content, string defaultLanguageCode, bool overwriteExistingValue);
        void TranslateName(string cultureToTranslateTo, string subscriptionKey, string uriBase, string defaultLanguageCode, IContent content, bool overwriteExistingValue);
        void TranslateProperty(string cultureToTranslateTo, string subscriptionKey, string uriBase, string defaultLanguageCode, IContent content, Property property, bool overwriteExistingValue);
    }
}
