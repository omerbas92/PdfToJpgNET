using System;
using System.Collections.Generic;

namespace PdfToJpg.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class FileDownloadModel
    {
        public List<FileModel> Files { get; set; }
        public string DownloadAllId { get; set; }
    }

    public class FileModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}