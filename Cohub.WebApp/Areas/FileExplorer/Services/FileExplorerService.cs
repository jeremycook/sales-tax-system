using Cohub.WebApp.Areas.FileExplorer.Views.FileExplorer;
using Microsoft.AspNetCore.StaticFiles;
using SiteKit.Files;
using System.Text.RegularExpressions;

namespace Cohub.WebApp.Areas.FileExplorer.Services
{
    public class FileExplorerService
    {
        private const string relativeRoot = "FileExplorer";

        private readonly FileManager fileManager;
        private readonly IContentTypeProvider contentTypeProvider;

        public FileExplorerService(FileManager fileManager, IContentTypeProvider contentTypeProvider)
        {
            this.fileManager = fileManager;
            this.contentTypeProvider = contentTypeProvider;
        }

        public FileModel GetFileModel(string id)
        {
            id = (id ?? string.Empty).Replace('\\', '/');
            id = Regex.Replace(id, @"\.+", ".");
            id = id.Trim(new[] { '/', '.', ' ' });

            var model = new FileModel()
            {
                Id = id,
                Subpath = "/" + id,
            };

            var fileProvider = fileManager.GetFileProvider(relativeRoot);
            var fileInfo = fileProvider.GetFileInfo(model.Subpath);

            model.Exists = fileInfo.Exists;
            model.IsDirectory = fileInfo.IsDirectory;
            model.PhysicalPath = fileInfo.PhysicalPath;

            if (contentTypeProvider.TryGetContentType("/" + fileInfo.Name, out string contentType))
            {
                model.ContentType = contentType;
                model.IsText = Regex.IsMatch(contentType, "^(text/|application/json$)");
                model.IsTrusted = Regex.IsMatch(contentType, "^(text/|image/|video/|audio/|application/json$|application/pdf$)");
            }
            else
            {
                model.ContentType = "application/octet-stream";
                model.IsText = false;
                model.IsTrusted = false;
            }

            return model;
        }

        public Microsoft.Extensions.FileProviders.IFileProvider GetFileProvider()
        {
            return fileManager.GetFileProvider(relativeRoot);
        }
    }
}
