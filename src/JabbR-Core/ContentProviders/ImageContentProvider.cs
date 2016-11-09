using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using JabbR_Core.Services;
using System.Threading.Tasks;
using JabbR_Core.UploadHandlers; 
using JabbR_Core.ContentProviders.Core;

namespace JabbR_Core.ContentProviders
{
    public class ImageContentProvider : CollapsibleContentProvider
    {
        private readonly UploadProcessor _uploadProcessor;

        public ImageContentProvider(UploadProcessor processor)
        {
            _uploadProcessor = processor;
        }

        protected override async Task<ContentProviderResult> GetCollapsibleContent(ContentProviderHttpRequest request)
        {
            string format = @"<a rel=""nofollow external"" target=""_blank"" href=""{0}""><img src=""{1}"" /></a>";
            string imageUrl = request.RequestUri.ToString();
            string href = imageUrl;

            Trace.TraceInformation("Processing image url " + imageUrl + ".");

            // Only proxy what we need to (non https images)
            if (!request.RequestUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                Trace.TraceInformation("Proxying of image url " + imageUrl + " is required.");

                //try
                //{
                //    Trace.TraceInformation("Http.GetAsync(" + request.RequestUri + ")");

                //    var response = await WebUtility.GetAsync(request.RequestUri)
                //                             .ConfigureAwait(continueOnCapturedContext: false);

                //    string fileName = Path.GetFileName(request.RequestUri.LocalPath);
                //    string contentType = GetContentType(request.RequestUri);
                //    long contentLength = response.ContentLength;

                //    Trace.TraceInformation("Status code: " + response.StatusCode);
                //    Trace.TraceInformation("response.GetResponseStream()");

                //    using (Stream stream = response.GetResponseStream())
                //    {
                //        Trace.TraceInformation("Uploading content " + imageUrl + ".");

                //        UploadResult result = await _uploadProcessor.HandleUpload(fileName, contentType, stream, contentLength)
                //                                                   .ConfigureAwait(continueOnCapturedContext: false);

                //        Trace.TraceInformation("Uploading " + imageUrl + " complete.");

                //        if (result != null)
                //        {
                //            imageUrl = result.Url;
                //        }
                //        else
                //        {
                //            Trace.TraceError("Failed to upload to blob storage. The upload result was null.");
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    // Trace the error then rethrow
                //    Trace.TraceError(ex.Message);

                //    throw;
                //}
            }
            else
            {
                Trace.TraceInformation("No proxying required for " + imageUrl + ".");
            }

            return new ContentProviderResult()
            {
                Content = String.Format(format, WebUtility.HtmlEncode(href), //Encoder.HtmlAttributeEncode(href),
                                                WebUtility.HtmlEncode(imageUrl)), //Encoder.HtmlAttributeEncode(imageUrl)),
                Title = href
            };
            //return null;
        }

        public override bool IsValidContent(Uri uri)
        {
            return IsValidImagePath(uri);
        }

        public static bool IsValidImagePath(Uri uri)
        {
            string path = uri.LocalPath.ToLowerInvariant();

            return path.EndsWith(".png") ||
                   path.EndsWith(".bmp") ||
                   path.EndsWith(".jpg") ||
                   path.EndsWith(".jpeg") ||
                   path.EndsWith(".gif");
        }

        public static string GetContentType(Uri uri)
        {
            string extension = Path.GetExtension(uri.LocalPath).ToLowerInvariant();

            switch (extension)
            {
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
            }

            return null;
        }
    }
}
