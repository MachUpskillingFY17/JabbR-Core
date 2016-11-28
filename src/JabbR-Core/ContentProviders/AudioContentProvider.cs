using System;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using JabbR_Core.ContentProviders.Core;
using System.Net;

namespace JabbR_Core.ContentProviders
{
    public class AudioContentProvider : IContentProvider
    {
        public bool IsValidContent(Uri uri)
        {
            return uri.AbsolutePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                   uri.AbsolutePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                   uri.AbsolutePath.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase);
        }

        public Task<ContentProviderResult> GetContent(ContentProviderHttpRequest request)
        {
            string url = request.RequestUri.ToString();
            return TaskAsyncHelper.FromResult(new ContentProviderResult()
            {
                Content = String.Format(@"<audio controls=""controls"" src=""{1}"">{0}</audio>", LanguageResources.AudioTagSupportRequired, WebUtility.HtmlEncode(url)), //.HtmlAttributeEncode(url)),
                Title = request.RequestUri.AbsoluteUri
            });
        }
    }
}