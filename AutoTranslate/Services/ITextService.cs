using System.Threading.Tasks;

namespace AutoTranslate.Services
{
    public interface ITextService
    {
        Task<string> MakeTextRequestAsync(string textToTranslate, string subscriptionKey, string uriBase, string[] languages);
    }
}