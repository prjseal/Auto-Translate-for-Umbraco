using AutoTranslate.Models;
using AutoTranslate.Services;
using System.Configuration;
using System.Linq;
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

        private string _subscriptionKey => ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
        private string _uriBase => ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
        private string _defaultLanguageCode => _localizationService.GetDefaultLanguageIsoCode();

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

        [System.Web.Http.HttpPost]
        public bool SubmitTranslateContent(ApiInstruction apiInstruction)
        {
            var content = _contentService.GetById(apiInstruction.NodeId);
            var allLanguages = _localizationService.GetAllLanguages();
            var defaultLanguage = allLanguages.FirstOrDefault(x => x.IsoCode == _defaultLanguageCode);

            if (content != null)
            {
                _contentTranslationService.TranslateContentItem(apiInstruction, _subscriptionKey, _uriBase, content, defaultLanguage);

                if (apiInstruction.IncludeDescendants)
                {
                    int pageIndex = 0;
                    int pageSize = 10;
                    long totalRecords = 0;
                    totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, defaultLanguage, pageIndex, pageSize);

                    if (totalRecords > pageSize)
                    {
                        pageIndex++;
                        while (totalRecords >= pageSize * pageIndex + 1)
                        {
                            totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, defaultLanguage, pageIndex, pageSize);
                            pageIndex++;
                        }
                    }
                }
            }

            return true;
        }

        [System.Web.Http.HttpPost]
        public bool SubmitTranslateDictionary(ApiInstruction apiInstruction)
        {
            var dictionaryItem = _localizationService.GetDictionaryItemById(apiInstruction.NodeId);
            var allLanguages = _localizationService.GetAllLanguages();
            var defaultLanguage = allLanguages.FirstOrDefault(x => x.IsoCode == _defaultLanguageCode);

            _dictionaryTranslationService.TranslateDictionaryItem(apiInstruction, _subscriptionKey, _uriBase, dictionaryItem, defaultLanguage, allLanguages);

            if (apiInstruction.IncludeDescendants)
            {
                var dictionaryDescendants = _localizationService.GetDictionaryItemDescendants(dictionaryItem.Key);
                if(dictionaryDescendants != null && dictionaryDescendants.Any())
                {
                    foreach(var descendantDictionaryItem in dictionaryDescendants)
                    {
                        _dictionaryTranslationService.TranslateDictionaryItem(apiInstruction, _subscriptionKey, _uriBase, descendantDictionaryItem, defaultLanguage, allLanguages);
                    }
                }
            }
            
            return true;
        }
    }
}