using Umbraco.Core;
using Umbraco.Core.Composing;
using AutoTranslate.Services;

namespace AutoTranslate.Composers
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class RegisterServicesComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<ITextService, TextService>();
        }
    }
}
