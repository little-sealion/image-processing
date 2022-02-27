using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessing.Functions
{
    public static class ProcessImageResize
    {
        private static IImageEncoder GetEncoder(string extension)
        {
            IImageEncoder encoder = null;

            extension = extension.Replace(".", "");

            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);

            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        encoder = new PngEncoder();
                        break; 
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    default:
                        break;
                }
            }

            return encoder;
        }
        
        
        [FunctionName("ProcessImageResize")]
        public static async Task Run([ServiceBusTrigger("imagequeue", Connection = "demoimagebus_SERVICEBUS")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            var resizeInfo = JsonConvert.DeserializeObject<ImageResizeDto>(myQueueItem);

            var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Connect to the storage account
            var storageAccount = CloudStorageAccount.Parse(storageConn);

            var myClient = storageAccount.CreateCloudBlobClient();
            
            // connect to a container
            var container = myClient.GetContainerReference("images");
            
            await container.CreateIfNotExistsAsync();

            var blobName = resizeInfo.FileName;
            
            // Get reference of the blob storage
            var cloudBlockBlob = container.GetBlobReference(blobName);

            var ms = new MemoryStream();

            await cloudBlockBlob.DownloadToStreamAsync(ms);

            byte[] bytes = ms.ToArray();

            var extension = Path.GetExtension(resizeInfo.FileName);
            var encoder = GetEncoder(extension);

            using (var output = new MemoryStream())
            using (Image<Rgba32> image = Image.Load(bytes))
            {
                log.LogInformation("Image Resize has started");
                
                image.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Size = new Size(resizeInfo.Width,resizeInfo.Height),
                    Compand = true,
                    Mode = ResizeMode.Max
                }));
                
                 image.Save(output, encoder);
                 output.Position = 0;
                
                // Create a new file and upload it to a blob storage
                var newFileName = $"resize_{resizeInfo.FileName}";

                var blobServiceClient = new BlobServiceClient(storageConn);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

                var blobCopy = container.GetBlobReference(newFileName);

                if (!await blobCopy.ExistsAsync())
                {
                    log.LogInformation("Upload to blob has started");
                    var uploadResult = await blobContainerClient.UploadBlobAsync(newFileName, output);
                    
                    log.LogInformation($"Result: {uploadResult.Value.VersionId}");
                }
            }
        }
    }
}
