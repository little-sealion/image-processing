using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessing.API.Services.Interfaces;

public interface IBlobsManagement
{
    Task<string> UploadFile(string containerName, string fileName, byte[] file, string connectionString);
}
