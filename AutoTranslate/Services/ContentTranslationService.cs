using AutoTranslate.Helpers;
using AutoTranslate.Models;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace AutoTranslate.Services
{
    public class ContentTranslationService : IContentTranslationService
    {
        private readonly ITranslationService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;

        public ContentTranslationService(ITranslationService textService, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, ILocalizationService localizationService)
        {
            _textService = textService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _localizationService = localizationService;
        }

        public long TranslatePageOfContentItems(ApiInstruction apiInstruction, string subscriptionKey, string uriBase, string defaultLanguageCode, int pageIndex, int pageSize)
        {
            long totalRecords;
            var descendants = _contentService.GetPagedDescendants(apiInstruction.NodeId, pageIndex, pageSize, out totalRecords);
            foreach (var contentItem in descendants)
            {
                TranslateContentItem(apiInstruction.CurrentCulture, subscriptionKey, uriBase, contentItem, defaultLanguageCode, apiInstruction.OverwriteExistingValues);
            }

            return totalRecords;
        }

        public void TranslateContentItem(string cultureToTranslateTo, string subscriptionKey, string uriBase, IContent content, string defaultLanguageCode, bool overwriteExistingValue)
        {
            TranslateName(cultureToTranslateTo, subscriptionKey, uriBase, defaultLanguageCode, content, overwriteExistingValue);
            foreach (var property in content.Properties)
            {
                TranslateProperty(cultureToTranslateTo, subscriptionKey, uriBase, defaultLanguageCode, content, property, overwriteExistingValue);
            }
            _contentService.Save(content);
        }

        public void TranslateName(string cultureToTranslateTo, string subscriptionKey, string uriBase, string defaultLanguageCode, IContent content, bool overwriteExistingValue)
        {
            var currentCultureNameValue = content.GetCultureName(cultureToTranslateTo);
            var defaultCultureNameValue = content.GetCultureName(defaultLanguageCode);
            if (!string.IsNullOrWhiteSpace(defaultCultureNameValue)
                && (overwriteExistingValue || string.IsNullOrWhiteSpace(currentCultureNameValue)))
            {
                var result = _textService.MakeTranslationRequestAsync(defaultCultureNameValue, subscriptionKey, uriBase, new string[] { cultureToTranslateTo });
                JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                content.SetCultureName(translatedValue.ToString(), cultureToTranslateTo);
            }
        }

        public void TranslateProperty(string cultureToTranslateTo, string subscriptionKey, string uriBase, string defaultLanguageCode, IContent content, Property property, bool overwriteExistingValue)
        {
            var currentValue = content.GetValue<string>(property.Alias, cultureToTranslateTo);
            var propertyValue = content.GetValue<string>(property.Alias, defaultLanguageCode);
            if (!string.IsNullOrWhiteSpace(propertyValue)
                && (overwriteExistingValue || string.IsNullOrWhiteSpace(currentValue)))
            {
                var result = _textService.MakeTranslationRequestAsync(propertyValue, subscriptionKey, uriBase, new string[] { cultureToTranslateTo });
                JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                content.SetValue(property.Alias, translatedValue, cultureToTranslateTo);
            }
        }
    }
}
