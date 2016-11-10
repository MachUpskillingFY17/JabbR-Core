using System.Threading.Tasks;

namespace JabbR_Core.ContentProviders.Core
{
    public interface IResourceProcessor
    {
        Task<ContentProviderResult> ExtractResource(string url);
    }
}
