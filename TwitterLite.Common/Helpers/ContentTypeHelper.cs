using System;
using System.IO;

namespace TwitterLite.Common.Helpers
{
    public static class ContentTypeHelper
    {
        public static string GetFileContentType(string FilePath)
        {
            string ContentType = String.Empty;
            string Extension = Path.GetExtension(FilePath).ToLower().Replace(".", "");

            switch (Extension)
            {
                case "txt":
                    ContentType = "text/plain";
                    break;
                default:
                    ContentType = "application/octet-stream";
                    break;

            }

            return ContentType;
        }
    }
}
