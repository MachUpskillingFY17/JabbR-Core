using System;
using System.Threading.Tasks;

namespace JabbR_Core.ContentProviders.Core
{
    public interface IContentProvider
    {
        Task<ContentProviderResult> GetContent(ContentProviderHttpRequest request);
        bool IsValidContent(Uri uri);
    }
}