using System.Configuration;
using System.Threading.Tasks;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using AutoTranslate.Services;
using Umbraco.Core.Services;
using Umbraco.Web;
using Newtonsoft.Json.Linq;

namespace AutoTranslate.Controllers
{
    [PluginController("AutoTranslate")]
    public class UtcaBackofficeApiController : UmbracoAuthorizedJsonController
    {
        private readonly ITextService _textService;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;

        public UtcaBackofficeApiController(ITextService textService, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, ILocalizationService localizationService)
        {
            _textService = textService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _localizationService = localizationService;
        }

        [System.Web.Http.HttpPost]
        public Task<string> GetTranslatedText(ApiInstruction apiInstruction)
        {
            string subscriptionKey = ConfigurationManager.AppSettings["AzureTranslateSubscriptionKey"];
            string uriBase = ConfigurationManager.AppSettings["AzureTranslateApiUrl"];
            var contentItem = _umbracoContextAccessor.UmbracoContext.ContentCache.GetById(apiInstruction.NodeId);
            var textArea = contentItem.Value<string>("textarea", "en-US");
            var content = _contentService.GetById(apiInstruction.NodeId);
            var result = _textService.MakeTextRequestAsync(textArea, subscriptionKey, uriBase, new string[] { apiInstruction.CurrentCulture });
            JArray translationRequest = JArray.Parse(result.Result);
            content.SetValue("textarea", translationRequest.First["translations"].First["text"], apiInstruction.CurrentCulture);
            _contentService.Save(content);
            return result;
        }
        
        public class ApiInstruction
        {
            public int NodeId { get; set; }
            public string CurrentCulture { get; set; }
        }
    }
}