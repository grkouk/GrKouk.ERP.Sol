﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GrKouk.Erp.Domain.MediaEntities;
using GrKouk.Web.ERP.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GrKouk.Web.ERP.Pages.MediaMng
{
    public class CreateModel : PageModel
    {
        private readonly ApiDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CreateModel(ApiDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public MediaEntry MediaEntry { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            long uploadedSize = 0;
            string pathForUploadedFiles = _hostingEnvironment.WebRootPath + "\\productimages\\";
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
                string newFileName = Guid.NewGuid() + extension;
                string newFilenameOnServer = pathForUploadedFiles + "\\" + newFileName;

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

                MediaEntry.MediaFile = newFileName;
                MediaEntry.OriginalFileName = uploadedFilename;
                _context.MediaEntries.Add(MediaEntry);
            }
            //return new JsonResult(listFiles);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}