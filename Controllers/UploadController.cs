using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfToJpg.Models;
using System.IO.Compression;
using Ghostscript.NET.Rasterizer;

namespace PdfToJpg.Controllers
{
    public class UploadController : Controller
    {
        private IHostingEnvironment HostingEnvironment;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(HomeController));

        public UploadController(IHostingEnvironment hostingEnvironment)
        {
            this.HostingEnvironment = hostingEnvironment;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IList<IFormFile> files)
        {
            try
            {
                if (files.Any(f => !Path.GetExtension(f.FileName).Equals(".pdf")))
                {
                    return Redirect("~/Home");
                }

                if (files.Any(f => f.Length > 31457280))
                {
                    return Redirect("~/Home");
                }

                FileDownloadModel fdm = new FileDownloadModel();
                fdm.Files = new List<FileModel>();
                long size = files.Sum(f => f.Length);
                long totalReadBytes = 0;

                string firstFolderId = Guid.NewGuid().ToString();
                string directoryPath = $"{this.HostingEnvironment.WebRootPath}/download/{firstFolderId}";
                var filePath = Path.GetTempFileName();
                filePath = filePath.Replace(".tmp", ".pdf");

                foreach (var formFile in files)
                {
                    var type = Path.GetExtension(formFile.FileName);

                    if (formFile.Length > 0)
                    {
                        byte[] buffer = new byte[16 * 1024];

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            using (Stream input = formFile.OpenReadStream())
                            {
                                int readBytes;

                                while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    await stream.WriteAsync(buffer, 0, readBytes);
                                }
                            }

                            await formFile.CopyToAsync(stream);
                        }

                        directoryPath = string.Concat(directoryPath, "/" + formFile.FileName);
                        Directory.CreateDirectory(directoryPath);

                        using (var rasterizer = new GhostscriptRasterizer())
                        {
                            rasterizer.Open(filePath);

                            for (int i = 1; i <= rasterizer.PageCount; i++)
                            {
                                var page = rasterizer.GetPage(120, 120, i);
                                var savePath = $"{directoryPath}/{formFile.FileName}_{i}.jpg";
                                page.Save(savePath);
                            }

                            rasterizer.Close();
                        }

                        string zipPath = $"{this.HostingEnvironment.WebRootPath}/download/{firstFolderId}/{formFile.FileName}.zip";
                        ZipFile.CreateFromDirectory(directoryPath, zipPath);

                        FileModel fileModel = new FileModel
                        {
                            FileName = formFile.FileName,
                            FilePath = zipPath
                        };

                        fdm.Files.Add(fileModel);
                    }

                    directoryPath = $"{this.HostingEnvironment.WebRootPath}/download/{firstFolderId}";
                }

                fdm.DownloadAllId = firstFolderId;

                if (files.Count > 1)
                {
                    string allZipPath = $"{this.HostingEnvironment.WebRootPath}/download/{firstFolderId}/pdftojpg.zip";
                    var allToZip = Directory.GetFiles(directoryPath);

                    using (ZipArchive newFile = ZipFile.Open(allZipPath, ZipArchiveMode.Create))
                    {
                        foreach (string file in allToZip)
                        {
                            newFile.CreateEntryFromFile(file, System.IO.Path.GetFileName(file));
                        }
                    }
                }

                return View(fdm);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Redirect("~/Home");
            }
        }
    }
}