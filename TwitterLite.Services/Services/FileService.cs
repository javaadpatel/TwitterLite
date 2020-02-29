using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TwitterLite.Contracts.Services;

namespace TwitterLite.Services.Services
{
    public class FileService : IFileService
    {
        public bool CheckIfFileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}
