using AutoTranslate.Models;
using Umbraco.Core.Models;

namespace AutoTranslate.Services
{
    public interface IContentTranslationService
    {
        long TranslatePageOfContentItems(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, int pageIndex, int pageSize);
        void TranslateContentItem(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, IContent content, ILanguage defaultLanguage);
        void TranslateName(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content);
        void TranslateProperty(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content, Property property);
    }
}
