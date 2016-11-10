using System;

namespace JabbR_Core.ContentProviders.Core
{
    public class ContentProviderHttpRequest
    {        
        public ContentProviderHttpRequest(Uri url)
        {
            RequestUri = url;
        }

        public Uri RequestUri { get; private set; }
    }
}