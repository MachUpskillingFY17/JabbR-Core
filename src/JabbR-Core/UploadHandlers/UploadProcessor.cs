using System.IO;
using System.Linq;
using JabbR_Core.Services;
using System.Threading.Tasks;
using JabbR_Core.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace JabbR_Core.UploadHandlers
{
    public class UploadProcessor
    {
        private readonly ApplicationSettings _appSettings;

        public UploadProcessor(IOptions<ApplicationSettings> settings)
        {
            _appSettings = settings.Value; 
        }

        //public async Task<UploadResult> HandleUpload(string fileName, string contentType, Stream stream, long contentLength)
        //{
        //    if (contentLength > _appSettings.MaxFileUploadBytes)
        //    {
        //        return new UploadResult { UploadTooLarge = true, MaxUploadSize = _appSettings.MaxFileUploadBytes };
        //    }

        //    string fileNameSlug = fileName.ToFileNameSlug();

        //    IUploadHandler handler = _fileUploadHandlers.FirstOrDefault(c => c.IsValid(fileNameSlug, contentType));

        //    if (handler == null)
        //    {
        //        return null;
        //    }

        //    return await handler.UploadFile(fileNameSlug, contentType, stream);
        //}

        //private static IList<IUploadHandler> GetUploadHandlers()
        //{
        //    // Use MEF to locate the content providers in this assembly
        //    var compositionContainer = new CompositionContainer(new AssemblyCatalog(typeof(UploadProcessor).Assembly));
        //    compositionContainer.ComposeExportedValue<IKernel>(kernel);
        //    return compositionContainer.GetExportedValues<IUploadHandler>().ToList();
        //}
    }
}