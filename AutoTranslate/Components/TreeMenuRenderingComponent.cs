using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Trees;

namespace AutoTranslate.Components
{
    public class TreeMenuRenderingComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;
        private readonly IDomainService _domainService;
        private readonly ILocalizationService _localizationService;

        public TreeMenuRenderingComponent(IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, IDomainService domainService, ILocalizationService localizationService)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _domainService = domainService;
            _localizationService = localizationService;
        }

        public void Initialize()
        {
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        public void Terminate()
        {
        }

        private void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias == "content")
            {
                var nodeId = e.NodeId;
                var contentItem = _umbracoContextAccessor.UmbracoContext.ContentCache.GetById(int.Parse(nodeId));

                var textArea = contentItem.Value("textarea", "en-US");
                               
                if (contentItem != null)
                {
                    var menuItem = new Umbraco.Web.Models.Trees.MenuItem("autoTranslate", "Auto Translate..");
                    menuItem.Icon = "globe-inverted-europe-africa";
                    menuItem.SeparatorBefore = true;
                    menuItem.AdditionalData.Add("actionView", "/App_Plugins/AutoTranslate/autotranslate.index.html");
                    var menuPosition = e.Menu.Items.Count - 1;
                    e.Menu.Items.Insert(menuPosition, menuItem);
                }
            }
        }
    }
}
