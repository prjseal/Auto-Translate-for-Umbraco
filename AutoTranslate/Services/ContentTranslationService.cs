﻿using System.Linq;
using AutoTranslate.Helpers;
using AutoTranslate.Models;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace AutoTranslate.Services
{
    public class ContentTranslationService : IContentTranslationService
    {
        private readonly ITextTranslationService _textTranslationService;
        private readonly IContentService _contentService;

        public ContentTranslationService(ITextTranslationService textTranslationService, IContentService contentService)
        {
            _textTranslationService = textTranslationService;
            _contentService = contentService;
        }

        public long TranslatePageOfContentItems(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, int pageIndex, int pageSize)
        {
            var descendants = _contentService.GetPagedDescendants(apiInstruction.NodeId, pageIndex, pageSize, out var totalRecords);
            foreach (var contentItem in descendants)
            {
                TranslateContentItem(apiInstruction, subscriptionKey, uriBase, contentItem, defaultLanguage);
            }

            return totalRecords;
        }

        public void TranslateContentItem(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, IContent content, ILanguage defaultLanguage)
        {
            TranslateName(apiInstruction, subscriptionKey, uriBase, defaultLanguage, content);

            var allowedPropertyEditors = apiInstruction.AllowedPropertyEditors.Split(',');

            foreach (var property in content.Properties)
            {
                if (allowedPropertyEditors.Contains(property.PropertyType.PropertyEditorAlias))
                {
                    TranslateProperty(apiInstruction, subscriptionKey, uriBase, defaultLanguage, content, property);
                }
            }
            _contentService.Save(content);
        }

        public void TranslateName(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content)
        {
            var currentCultureNameValue = content.GetCultureName(apiInstruction.CurrentCulture);
            var defaultCultureNameValue = content.GetCultureName(defaultLanguage.IsoCode);
            if (!string.IsNullOrWhiteSpace(defaultCultureNameValue)
                && (apiInstruction.OverwriteExistingValues || string.IsNullOrWhiteSpace(currentCultureNameValue)))
            {
                var result = _textTranslationService.MakeTranslationRequestAsync(defaultCultureNameValue, subscriptionKey, uriBase, new[] { apiInstruction.CurrentCulture }, defaultLanguage.IsoCode);
                JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                content.SetCultureName(translatedValue.ToString(), apiInstruction.CurrentCulture);
            }
        }

        public void TranslateProperty(ContentApiInstruction apiInstruction, string subscriptionKey, string uriBase, ILanguage defaultLanguage, IContent content, Property property)
        {
            var currentValue = content.GetValue<string>(property.Alias, apiInstruction.CurrentCulture);
            var propertyValue = content.GetValue<string>(property.Alias, defaultLanguage.IsoCode);
            if (!string.IsNullOrWhiteSpace(propertyValue)
                && (apiInstruction.OverwriteExistingValues || string.IsNullOrWhiteSpace(currentValue)))
            {
                var result = _textTranslationService.MakeTranslationRequestAsync(propertyValue, subscriptionKey, uriBase, new[] { apiInstruction.CurrentCulture }, defaultLanguage.IsoCode);
                JToken translatedValue = CommonHelpers.GetTranslatedValue(result);
                content.SetValue(property.Alias, translatedValue, apiInstruction.CurrentCulture);
            }
        }
    }
}
