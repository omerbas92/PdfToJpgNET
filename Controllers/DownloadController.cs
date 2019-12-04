using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace PdfToJpg.Controllers
{
    public class DownloadController : Controller
    {
        private IHostingEnvironment HostingEnvironment;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(HomeController));

        public DownloadController(IHostingEnvironment hostingEnvironment)
        {
            this.HostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Download")]
        public ActionResult Download(string id, string name = null)
        {
            try
            {
                string path;
                string fileName;

                if (string.IsNullOrEmpty(name))
                {
                    path = $"{this.HostingEnvironment.WebRootPath}/download/{id}/pdftojpg.zip";
                    fileName = "pdftojpg.zip";
                }
                else
                {
                    path = $"{this.HostingEnvironment.WebRootPath}/download/{id}/{name}.zip";
                    fileName = "pdftojpg_" + name + ".zip";
                }

                if (System.IO.File.Exists(path))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    return Redirect("~/");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Redirect("~/Home/Error");
            }

        }
    }
}