using System;
using System.Linq;
using JabbR_Core.Services;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace JabbR_Core.ContentProviders.Core
{
    public class ResourceProcessor : IResourceProcessor
    {
        private readonly IList<IContentProvider> _contentProviders;
        private readonly ApplicationSettings _settings;

        public ResourceProcessor(IList<IContentProvider> contentProviders,
                                 IOptions<ApplicationSettings> settings)
        {
            _contentProviders = contentProviders;
            _settings = settings.Value;
        }

        public Task<ContentProviderResult> ExtractResource(string url)
        {
            Uri resultUrl;
            if (Uri.TryCreate(url, UriKind.Absolute, out resultUrl))
            {
                var request = new ContentProviderHttpRequest(resultUrl);
                return ExtractContent(request);
            }

            return TaskAsyncHelper.FromResult<ContentProviderResult>(null);
        }

        private Task<ContentProviderResult> ExtractContent(ContentProviderHttpRequest request)
        {
            var val = GetActiveContentProviders();
            var validProviders =val.Where(c => c.IsValidContent(request.RequestUri))
                                                  .ToList();

            if (validProviders.Count == 0)
            {
                return TaskAsyncHelper.FromResult<ContentProviderResult>(null);
            }

            var tasks = validProviders.Select(c => c.GetContent(request)).ToArray();

            var tcs = new TaskCompletionSource<ContentProviderResult>();

            Task.Factory.ContinueWhenAll(tasks, completedTasks =>
            {
                var faulted = completedTasks.FirstOrDefault(t => t.IsFaulted);
                if (faulted != null)
                {
                    tcs.SetException(faulted.Exception);
                }
                else if (completedTasks.Any(t => t.IsCanceled))
                {
                    tcs.SetCanceled();
                }
                else
                {
                    ContentProviderResult result = completedTasks.Select(t => t.Result)
                                                                 .FirstOrDefault(content => content != null);
                    tcs.SetResult(result);
                }
            });

            return tcs.Task;
        }

        private IList<IContentProvider> GetActiveContentProviders()
        {
             return _contentProviders
                .Where(cp => !_settings.DisabledContentProviders.Contains(cp.GetType().Name))
                .ToList();
        }
    }
}