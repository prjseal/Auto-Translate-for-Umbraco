using System.Threading.Tasks;

namespace AutoTranslate.Services
{
    public interface ITranslationService
    {
        Task<string> MakeTranslationRequestAsync(string textToTranslate, string subscriptionKey, string uriBase, string[] languages);
    }
}