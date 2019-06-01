using AutoTranslate.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;

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
