using AutoTranslate.Models;
using Umbraco.Core.Models;

namespace AutoTranslate.Services
{
    public interface IContentTranslationService
    {
        long TranslatePageOfContentItems(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, int pageIndex, int pageSize);
        void TranslateContentItem(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, IContent content, ILanguage defaultLanguage);
        void TranslateName(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content);
        void TranslateProperty(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content, Property property);
    }
}
