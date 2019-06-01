using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Trees;

namespace AutoTranslate.Components
{
    public class TreeMenuRenderingComponent : IComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public TreeMenuRenderingComponent(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
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
                
                if (contentItem != null)
                {
                    var menuItem = new Umbraco.Web.Models.Trees.MenuItem("autoTranslate", "Auto Translate..");
                    menuItem.Icon = "globe-inverted-europe-africa";
                    menuItem.SeparatorBefore = true;
                    menuItem.AdditionalData.Add("actionView", "/App_Plugins/AutoTranslate/autotranslate.content.html");
                    var menuPosition = e.Menu.Items.Count - 1;
                    e.Menu.Items.Insert(menuPosition, menuItem);
                }
            }
            else if (sender.TreeAlias == "dictionary")
            {
                var menuItem = new Umbraco.Web.Models.Trees.MenuItem("autoTranslate", "Auto Translate..");
                menuItem.Icon = "globe-inverted-europe-africa";
                menuItem.SeparatorBefore = true;
                menuItem.AdditionalData.Add("actionView", "/App_Plugins/AutoTranslate/autotranslate.dictionary.html");
                var menuPosition = e.Menu.Items.Count - 1;
                e.Menu.Items.Insert(menuPosition, menuItem);
            }
        }
    }
}
