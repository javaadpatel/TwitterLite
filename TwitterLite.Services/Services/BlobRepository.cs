using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TwitterLite.Common.Helpers;
using TwitterLite.Contracts.Services;

namespace TwitterLite.Services.Services
{
    public class BlobRepositoryBase : IBlobRepository
    {
        protected string _storageConnectionString;
        protected string containerName = "sourcefiles";
        private readonly ILogger<IBlobRepository> _logger;
        private string _path = Directory.GetCurrentDirectory();// + "\\downloadedFiles\\";

        protected CloudStorageAccount cloudStorageAccount;
        public CloudStorageAccount CloudStorageAccount
        {
            get
            {
                if (cloudStorageAccount == null)
                {
                    CloudStorageAccount.TryParse(_storageConnectionString, out cloudStorageAccount);
                }
                return cloudStorageAccount;
            }
            set { cloudStorageAccount = value; }
        }

        protected CloudBlobClient cloudBlobClient;
        public CloudBlobClient CloudBlobClient
        {
            get
            {
                cloudBlobClient = CloudStorageAccount.CreateCloudBlobClient();
                return cloudBlobClient;
            }
            set { cloudBlobClient = value; }
        }


        protected CloudBlobContainer cloudBlobContainer;
        public CloudBlobContainer CloudBlobContainer
        {
            get
            {
                cloudBlobContainer = CloudBlobClient.GetContainerReference(containerName);
                var containerCreationResult = cloudBlobContainer.CreateIfNotExistsAsync().Result;
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                };
                cloudBlobContainer.SetPermissionsAsync(permissions).GetAwaiter().GetResult();
                return cloudBlobContainer;
            }
            set { cloudBlobContainer = value; }
        }

        public BlobRepositoryBase(IConfiguration _configuration, ILogger<IBlobRepository> logger)
        {
            _storageConnectionString = _configuration["AzureBlobStorageConnectionString"];
            _logger = logger;
        }

        public async Task<string> DownloadBlobAsync(string fileName)
        {
            try
            {
                //Get a reference to the blob address and then download file
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(fileName);

                var downloadedFile = await cloudBlockBlob.DownloadTextAsync();

                return downloadedFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DownloadBlobAsync)} failed with parameter(s):" +
                      $"{Environment.NewLine}|| {nameof(fileName)}: {JsonConvert.SerializeObject(fileName)}");
                throw;
            }
        }

        public async Task DownloadBlobAsFileAsync(string fileName)
        {
            try
            {
                //Get a reference to the blob address and then download file
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(fileName);

                await cloudBlockBlob.DownloadToFileAsync($"{ _path}{fileName}", FileMode.Create);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DownloadBlobAsync)} failed with parameter(s):" +
                      $"{Environment.NewLine}|| {nameof(fileName)}: {JsonConvert.SerializeObject(fileName)}");
                throw;
            }
        }

        public async Task<byte[]> DownloadBlobAsByteArrayAsync(string fileName)
        {
            try
            {
                //Get a reference to the blob address and then download file
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(fileName);
                await cloudBlockBlob.FetchAttributesAsync();

                var fileData = new byte[cloudBlockBlob.Properties.Length];

                await cloudBlockBlob.DownloadToByteArrayAsync(fileData, 0);

                return fileData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DownloadBlobAsByteArrayAsync)} failed with parameter(s):" +
                      $"{Environment.NewLine}|| {nameof(fileName)}: {JsonConvert.SerializeObject(fileName)}");
                throw;
            }
        }

        /// <summary>Uploads to file to blob storage asynchronous.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public async Task<bool> UploadToBlobAsync(string fileName, Stream stream = null)
        {
            try
            {
                // Get a reference to the blob address, then upload the file to the blob.
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(fileName);
                cloudBlockBlob.Properties.ContentType = ContentTypeHelper.GetFileContentType(fileName);

                if (stream == null)
                    return false;

                await cloudBlockBlob.UploadFromStreamAsync(stream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UploadToBlobAsync)} failed with parameter(s):" +
                      $"{Environment.NewLine}|| {nameof(fileName)}: {JsonConvert.SerializeObject(fileName)}" +
                      $"{Environment.NewLine}|| {nameof(stream)}: {JsonConvert.SerializeObject(stream)}");
                return false;
            }
        }

        /// <summary>Deletes the file from blob storage asynchronous.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public async Task<bool> DeleteFileFromBlobAsync(string fileName)
        {
            try
            {
                // Get a reference to the blob address, then upload the file to the blob.
                CloudBlockBlob cloudBlockBlob = CloudBlobContainer.GetBlockBlobReference(fileName);

                //delete from blob
                await cloudBlockBlob.DeleteIfExistsAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteFileFromBlobAsync)} failed with parameter(s):" +
                     $"{Environment.NewLine}|| {nameof(fileName)}: {JsonConvert.SerializeObject(fileName)}");
                return false;
            }
        }
    }
}
