using System.IO;
using System.Threading.Tasks;

namespace JabbR_Core.UploadHandlers
{
   //InheritedExport]
    public interface IUploadHandler
    {
        bool IsValid(string fileName, string contentType);
        Task<UploadResult> UploadFile(string fileName, string contentType, Stream stream);
    }
}