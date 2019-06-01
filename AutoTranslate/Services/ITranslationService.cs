using System.Threading.Tasks;

namespace AutoTranslate.Services
{
    public interface ITextTranslationService
    {
        Task<string> MakeTranslationRequestAsync(string textToTranslate, string subscriptionKey, string uriBase, string[] languages, string translateFrom);
    }
}