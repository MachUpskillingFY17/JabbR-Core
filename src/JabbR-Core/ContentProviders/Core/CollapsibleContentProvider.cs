using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JabbR_Core.ContentProviders.Core
{
    public abstract class CollapsibleContentProvider : IContentProvider
    {
        public virtual Task<ContentProviderResult> GetContent(ContentProviderHttpRequest request)
        {

            return GetCollapsibleContent(request).Then(result =>
            {
                if (GetIsCollapsible(request.RequestUri) && result != null)
                {
                    string contentTitle = String.Format(LanguageResources.Content_HeaderAndToggle, WebUtility.HtmlEncode(result.Title));
                    result.Content = String.Format(ContentFormat, contentTitle, result.Content);
                }

                return result;
            });
        }

        protected virtual Regex ParameterExtractionRegex
        {
            get
            {
                return new Regex(@"(\d+)");

            }
        }

        protected virtual Regex GetParameterExtractionRegex(Uri responseUri)
        {
            return ParameterExtractionRegex;
        }

        protected virtual IList<string> ExtractParameters(Uri responseUri)
        {
            var parameterExtractionRegex = GetParameterExtractionRegex(responseUri);
            if (parameterExtractionRegex == null)
            {
                return null;
            }

            return parameterExtractionRegex.Match(responseUri.AbsoluteUri)
                                .Groups
                                .Cast<Group>()
                                .Skip(1)
                                .Select(g => g.Value)
                                .Where(v => !String.IsNullOrEmpty(v)).ToList();

        }
        protected abstract Task<ContentProviderResult> GetCollapsibleContent(ContentProviderHttpRequest request);

        public virtual bool IsValidContent(Uri uri)
        {
            return false;
        }

        protected virtual bool GetIsCollapsible(Uri responseUri)
        {
            return IsCollapsible;
        }

        protected virtual bool IsCollapsible { get { return true; } }

        private const string ContentFormat = @"<div class=""collapsible_content""><h3 class=""collapsible_title"">{0}</h3><div class=""collapsible_box"">{1}</div></div>";
    }
}