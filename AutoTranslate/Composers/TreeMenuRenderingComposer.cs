using Umbraco.Core;
using Umbraco.Core.Composing;
using AutoTranslate.Components;

namespace AutoTranslate.Composers
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class TreeMenuRenderingComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<TreeMenuRenderingComponent>();
        }
    }
}
