using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrKouk.Web.ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PictureController(ApiDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFilesAjax()
        {
            //< init >

            long uploadedSize = 0;

            string pathForUploadedFiles = _hostingEnvironment.WebRootPath + "\\productImages\\";

            var uploadedFiles = Request.Form.Files;


            int iCounter = 0;
            string sFilesUploaded = "";
            List<string> listFiles = new List<string>();
            foreach (var uploadedFile in uploadedFiles)
            {
                iCounter++;
                uploadedSize += uploadedFile.Length;
                sFilesUploaded += "\n" + uploadedFile.FileName;
                listFiles.Add(uploadedFile.FileName);
                //< Filename >
                var extension = "." + uploadedFile.FileName.Split('.')[uploadedFile.FileName.Split('.').Length - 1];
                string uploadedFilename = uploadedFile.FileName;
                string newFilenameOnServer = pathForUploadedFiles + "\\" + Guid.NewGuid() + extension;
                //</ Filename >
                //< Copy File to Target >

                await using (FileStream stream = new FileStream(newFilenameOnServer, FileMode.Create))
                {
                    try
                    {
                        await uploadedFile.CopyToAsync(stream);
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, new {message=e.Message });
                    }
                    catch (UnauthorizedAccessException  e)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new {message=e.Message });
                    }
                    catch (Exception e)
                    {

                        return StatusCode(StatusCodes.Status409Conflict, new {message=e.Message });
                    }
                    
                }
            }

            //------</ @Loop: Uploaded Files >------


            return Ok(listFiles);
            //return new JsonResult(listFiles);
        }
    }
}