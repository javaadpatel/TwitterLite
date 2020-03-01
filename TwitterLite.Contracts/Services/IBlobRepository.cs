using System;
using System.IO;
using System.Threading.Tasks;

namespace TwitterLite.Contracts.Services
{
    public interface IBlobRepository
    {
        Task<string> DownloadBlobAsync(string fileName);
        Task<byte[]> DownloadBlobAsByteArrayAsync(string fileName);
        Task DownloadBlobAsFileAsync(string fileName);
        Task<bool> UploadToBlobAsync(string fileName, Stream stream = null);
        Task<bool> DeleteFileFromBlobAsync(string fileName);
    }
}
