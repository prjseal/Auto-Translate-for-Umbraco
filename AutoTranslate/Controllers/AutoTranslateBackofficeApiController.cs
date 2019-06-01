using AutoTranslate.Models;
using AutoTranslate.Services;
using System.Configuration;
using System.Linq;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace AutoTranslate.Controllers
{
    [PluginController("AutoTranslate")]
    public class AutoTranslateBackofficeApiController : UmbracoAuthorizedJsonController
    {
        private readonly ITranslationService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;
        private readonly IContentTranslationService _contentTranslationService;
        private readonly IDictionaryTranslationService _dictionaryTranslationService;

        private string _subscriptionKey => ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
        private string _uriBase => ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
        private string _defaultLanguageCode => _localizationService.GetDefaultLanguageIsoCode();

        public AutoTranslateBackofficeApiController(ITranslationService textService, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, 
            ILocalizationService localizationService, IDictionaryTranslationService dictionaryTranslationService, IContentTranslationService contentTranslationService)
        {
            _textService = textService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _localizationService = localizationService;
            _contentTranslationService = contentTranslationService;
            _dictionaryTranslationService = dictionaryTranslationService;
        }

        [System.Web.Http.HttpPost]
        public bool SubmitTranslateContent(ApiInstruction apiInstruction)
        {
            var content = _contentService.GetById(apiInstruction.NodeId);

            if(content != null)
            {
                _contentTranslationService.TranslateContentItem(apiInstruction.CurrentCulture, _subscriptionKey, _uriBase, content, _defaultLanguageCode, apiInstruction.OverwriteExistingValues);

                if (apiInstruction.IncludeDescendants)
                {
                    int pageIndex = 0;
                    int pageSize = 10;
                    long totalRecords = 0;
                    totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, _defaultLanguageCode, pageIndex, pageSize);

                    if (totalRecords > pageSize)
                    {
                        pageIndex++;
                        while (totalRecords >= pageSize * pageIndex + 1)
                        {
                            totalRecords = _contentTranslationService.TranslatePageOfContentItems(apiInstruction, _subscriptionKey, _uriBase, _defaultLanguageCode, pageIndex, pageSize);
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
            var defaultLanguage = _localizationService.GetLanguageIdByIsoCode(_defaultLanguageCode);
            var allLanguages = _localizationService.GetAllLanguages();

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