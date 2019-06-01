using AutoTranslate.Services;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace AutoTranslate.Composers
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RegisterServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<ITranslationService, AzureTranslationService>();
            composition.Register<IContentTranslationService, ContentTranslationService>();
            composition.Register<IDictionaryTranslationService, DictionaryTranslationService>();
        }
    }
}
