using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class FileUploadedList {
        public List<FileUploaded> FileUploadeds { get; set; }

        public long TotalSize { get; set; }
        public int TotalFiles { get; set; }
    }

    public class FileUploaded
    {
        public string FileName { get; set; }

        public string URL { get; set; }

        public long Size { get; set; }

        public string Message { get; set; }
    }
}