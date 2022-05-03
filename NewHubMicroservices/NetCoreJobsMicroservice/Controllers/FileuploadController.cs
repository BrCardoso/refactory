using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using NetCoreJobsMicroservice.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class FileuploadController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get(string file)
        {
            try
            {            
                string filePath = file;

                IFileProvider provider = new PhysicalFileProvider(Path.GetFullPath("."));
                IFileInfo fileInfo = provider.GetFileInfo(file);
                var readStream = fileInfo.CreateReadStream();

                return File(readStream, "image/jpeg", file);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        [Route("UploadAsync")]
        public async Task<object> UploadAsync(List<IFormFile> file)
        {
            FileUploadedList ful = new FileUploadedList();
            ful.FileUploadeds = new List<FileUploaded>();
            long size = file.Sum(f => f.Length);

            foreach (var formFile in file)
            {
                string extension = Path.GetExtension(formFile.FileName);
                var filePath = Path.GetRandomFileName() + extension;
                if (formFile.Length > 0)
                {
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
                ful.FileUploadeds.Add(new FileUploaded
                {
                    FileName = formFile.FileName,
                    Size = formFile.Length,
                    URL = filePath,
                    Message = "Arquivo salvo"
                });
            }
            ful.TotalSize = size;
            ful.TotalFiles = file.Count;
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(ful);
        }

        [HttpPost]
        [Route("UploadInvoice")]
        public async Task<object> UploadInvoice(List<IFormFile> file)
        {
            FileUploadedList ful = new FileUploadedList();

            ful.FileUploadeds = new List<FileUploaded>();
            long size = file.Sum(f => f.Length);

            foreach (var formFile in file)
            {
                string extension = Path.GetExtension(formFile.FileName);
                var fileName = Path.GetRandomFileName();
                var filePath = Path.GetTempPath() + fileName;
                if (formFile.Length > 0 && extension == ".csv")
                {
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }                    
                }

                ful.FileUploadeds.Add(new FileUploaded
                {
                    FileName = formFile.FileName,
                    Size = formFile.Length,
                    URL = fileName,
                    Message = "Arquivo salvo"
                });
            }
            ful.TotalSize = size;
            ful.TotalFiles = file.Count;
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.




            return Ok(ful);
        }
    }
}
