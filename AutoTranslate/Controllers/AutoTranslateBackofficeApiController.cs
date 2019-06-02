using System.Collections.Generic;
using AutoTranslate.Models;
using AutoTranslate.Services;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace AutoTranslate.Controllers
{
    [PluginController("AutoTranslate")]
    public class AutoTranslateBackofficeApiController : UmbracoAuthorizedJsonController
    {
        private readonly ILocalizationService _localizationService;
        private readonly IContentTranslationService _contentTranslationService;
        private readonly IDictionaryTranslationService _dictionaryTranslationService;
        private readonly IContentService _contentService;

        private string SubscriptionKey => ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
        private string UriBase => ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
        private string DefaultLanguageCode => _localizationService.GetDefaultLanguageIsoCode();

        public AutoTranslateBackofficeApiController(ILocalizationService localizationService, 
            IDictionaryTranslationService dictionaryTranslationService, 
            IContentTranslationService contentTranslationService,
            IContentService contentService)
        {
            _localizationService = localizationService;
            _contentTranslationService = contentTranslationService;
            _dictionaryTranslationService = dictionaryTranslationService;
            _contentService = contentService;
        }

        [HttpPost]
        public bool SubmitTranslateContent(ContentApiInstruction apiInstruction)
        {
            var content = _contentService.GetById(apiInstruction.NodeId);
            var allLanguages = _localizationService.GetAllLanguages();
            var defaultLanguage = allLanguages.FirstOrDefault(x => x.IsoCode == DefaultLanguageCode);

            if (content != null)
            {
                _contentTranslationService.TranslateContentItem(apiInstruction, SubscriptionKey, UriBase, content, defaultLanguage);

                if (apiInstruction.IncludeDescendants)
                {
                    int pageIndex = 0;
                    int pageSize = 10;
                    var totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, SubscriptionKey, UriBase, defaultLanguage, pageIndex, pageSize);

                    if (totalRecords > pageSize)
                    {
                        pageIndex++;
                        while (totalRecords >= pageSize * pageIndex + 1)
                        {
                            totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, SubscriptionKey, UriBase, defaultLanguage, pageIndex, pageSize);
                            pageIndex++;
                        }
                    }
                }
            }

            return true;
        }

        [HttpPost]
        public bool SubmitTranslateDictionary(DictionaryApiInstruction apiInstruction)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(apiInstruction.NodeId);
            var allLanguages = _localizationService.GetAllLanguages();
            var allLanguagesList = allLanguages.ToList();
            var defaultLanguage = allLanguagesList.FirstOrDefault(x => x.IsoCode == DefaultLanguageCode);

            _dictionaryTranslationService.TranslateDictionaryItem(apiInstruction, SubscriptionKey, UriBase, dictionaryItem, defaultLanguage, allLanguagesList);

            if (apiInstruction.IncludeDescendants)
            {
                var dictionaryDescendants = _localizationService.GetDictionaryItemDescendants(dictionaryItem.Key);
                var descendantDictionaryItems = dictionaryDescendants.ToList();
                if(descendantDictionaryItems.Any())
                {
                    foreach(var descendantDictionaryItem in descendantDictionaryItems)
                    {
                        _dictionaryTranslationService.TranslateDictionaryItem(apiInstruction, SubscriptionKey, UriBase, descendantDictionaryItem, defaultLanguage, allLanguagesList);
                    }
                }
            }
            
            return true;
        }

        [HttpGet]
        public List<string> GetEditorAliasesFromContentItem(int contentId)
        {
            var contentItem = _contentService.GetById(contentId);
            return contentItem?.Properties.Select(x => x.PropertyType.PropertyEditorAlias).Distinct().ToList();
        }
    }
}