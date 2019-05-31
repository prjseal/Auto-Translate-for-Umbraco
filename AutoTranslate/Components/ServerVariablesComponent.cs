using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.JavaScript;
using AutoTranslate.Controllers;

namespace AutoTranslate.Components
{
    public class ServerVariablesComponent : IComponent
    {
        public void Initialize()
        {
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        public void Terminate()
        {
        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            var urlHelper =
                new UrlHelper(
                    new RequestContext(
                        new HttpContextWrapper(
                            HttpContext.Current),
                        new RouteData()));

            if (!e.ContainsKey("AutoTranslate"))
                e.Add("AutoTranslate", new Dictionary<string, object>
                {
                    {
                        "ApiUrl",
                        urlHelper.GetUmbracoApiServiceBaseUrl<AutoTranslateBackofficeApiController>(
                            controller => controller.SubmitTranslateContent(new AutoTranslateBackofficeApiController.ApiInstruction { CurrentCulture = string.Empty, NodeId = 0 }))
                    }
                });
        }
    }
}
