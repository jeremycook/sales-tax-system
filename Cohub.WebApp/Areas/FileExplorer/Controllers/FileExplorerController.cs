using Cohub.Data.Usr;
using Cohub.WebApp.Areas.FileExplorer.Services;
using Cohub.WebApp.Areas.FileExplorer.Views.FileExplorer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cohub.WebApp.Areas.FileExplorer.Controllers
{
    [Authorize(Policy.Super)]
    [Area("FileExplorer")]
    [Route("file-explorer")]
    public class FileExplorerController : Controller
    {
        private readonly FileExplorerService fileExplorerService;
        private readonly ILogger<FileExplorerController> logger;

        public FileExplorerController(FileExplorerService fileExplorerService, ILogger<FileExplorerController> logger)
        {
            this.fileExplorerService = fileExplorerService;
            this.logger = logger;
        }

        [HttpGet]
        public ViewResult Index()
        {
            IFileProvider fileProvider = fileExplorerService.GetFileProvider();

            return View(fileProvider);
        }

        [Route("create")]
        public async Task<ActionResult> Create(Edit input, [FromServices] IContentTypeProvider contentTypeProvider)
        {
            var fileProvider = fileExplorerService.GetFileProvider();
            var fileInfo = fileProvider.GetFileInfo("/" + input.Id);

            if (contentTypeProvider.TryGetContentType("/" + input.Id, out string contentType))
            {
                input.ContentType = contentType;
                input.IsText = Regex.IsMatch(contentType, "^(text/|application/json$)");
            }

            if (Request.IsGet())
            {
                ModelState.Clear();
            }
            else if (Request.IsPost())
            {
                if (fileInfo.Exists)
                {
                    ModelState.AddModelError("", $"The {input.Id} file was not created because a file with that name already exists.");
                }
                else if (!input.IsText)
                {
                    ModelState.AddModelError("", $"The {input.ContentType} is not a supported text type.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath)!);

                        using var file = System.IO.File.Open(fileInfo.PhysicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                        using var writer = new StreamWriter(file, Encoding.UTF8) { AutoFlush = true };

                        await writer.WriteAsync(input.Content);

                        TempData.Success($"Created {input.Id}.");
                        return RedirectToAction("Details", new { input.Id });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while trying to save the file: " + ex.AllMessages());
                    }
                }
            }

            return View(input);
        }

        [Route("upload")]
        [AutoValidateAntiforgeryToken]
        public async Task<ActionResult> Upload(string? id)
        {
            if (Request.IsGet())
            {
                ModelState.Clear();
            }
            else if (Request.IsPost())
            {
                var fileProvider = fileExplorerService.GetFileProvider();

                var folderInfo = fileProvider.GetFileInfo("/" + id);

                if (!Request.HasFormContentType || !Request.Form.Files.Any())
                {
                    ModelState.AddModelError("", "Please select one or more files for upload.");
                }
                else if (folderInfo.Exists && !folderInfo.IsDirectory)
                {
                    ModelState.AddModelError("", "The file must be stored in a directory.");
                }

                if (ModelState.IsValid)
                {
                    Directory.CreateDirectory(folderInfo.PhysicalPath);

                    foreach (var upload in Request.Form.Files)
                    {
                        string filename = Path.GetFileName(upload.FileName);
                        var fileInfo = fileProvider.GetFileInfo("/" + id + "/" + filename);
                        if (fileInfo.Exists)
                        {
                            ModelState.AddModelError("", $"The {filename} file was not uploaded because a file with that name already exists.");
                            continue;
                        }

                        try
                        {
                            using var file = System.IO.File.Open(fileInfo.PhysicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                            await upload.CopyToAsync(file);
                            TempData.Success($"Uploaded {filename}.");
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                            ModelState.AddModelError("", $"An error occurred while trying to save the {filename} file: " + ex.AllMessages());
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }

            return View();
        }

        [HttpGet("details/{**Id}")]
        public ViewResult Details(string id)
        {
            var model = fileExplorerService.GetFileModel(id);

            return View(model);
        }

        [HttpGet("preview/{**Id}")]
        public ActionResult Preview(string id, bool forceTrust = false)
        {
            var model = fileExplorerService.GetFileModel("/" + id);

            if (!model.Exists || model.IsDirectory)
            {
                return NotFound();
            }

            if (model.IsTrusted || forceTrust)
            {
                return PhysicalFile(model.PhysicalPath, model.ContentType);
            }
            else
            {
                return View(model);
            }
        }

        [Route("edit/{**Id}")]
        public async Task<ActionResult> Edit(Edit input, [FromServices] IContentTypeProvider contentTypeProvider)
        {
            var fileProvider = fileExplorerService.GetFileProvider();
            var fileInfo = fileProvider.GetFileInfo("/" + input.Id);

            if (!fileInfo.Exists || fileInfo.IsDirectory)
            {
                return NotFound();
            }

            contentTypeProvider.TryGetContentType("/" + input.Id, out string contentType);
            input.ContentType = contentType;
            input.IsText = Regex.IsMatch(contentType, "^(text/|application/json$)");

            if (Request.IsGet())
            {
                ModelState.Clear();

                if (input.IsText)
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var reader = new StreamReader(stream);
                    input.Content = reader.ReadToEnd();
                }
            }
            else if (Request.IsPost())
            {
                if (!input.IsText)
                {
                    ModelState.AddModelError("", $"The {input.ContentType} is not a supported text type.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        using var file = System.IO.File.Open(fileInfo.PhysicalPath, FileMode.Truncate, FileAccess.Write, FileShare.None);
                        using var writer = new StreamWriter(file, Encoding.UTF8) { AutoFlush = true };

                        await writer.WriteAsync(input.Content);

                        TempData.Success($"Updated {input.Id}.");
                        return RedirectToAction("Details", new { input.Id });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while trying to save the file: " + ex.AllMessages());
                    }
                }
            }

            return View(input);
        }

        [Route("delete/{**Id}")]
        public ActionResult Delete(string id)
        {
            var model = fileExplorerService.GetFileModel(id);

            if (!model.Exists)
            {
                return NotFound();
            }

            if (model.Id == string.Empty)
            {
                ModelState.AddModelError("", "Cannot delete the root directory.");
            }

            if (Request.IsGet())
            {
            }
            else if (Request.IsPost())
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (model.IsDirectory)
                        {
                            Directory.Delete(model.PhysicalPath);
                        }
                        else
                        {
                            System.IO.File.Delete(model.PhysicalPath);
                        }

                        TempData.Success($"Deleted {model.Id}.");
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while trying to delete: " + ex.AllMessages());
                    }
                }
            }

            return View(model);
        }
    }
}
