using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace SiteKit.Files
{
    /// <summary>
    /// Singleton service.
    /// </summary>
    public class FileManager
    {
        private readonly ConcurrentDictionary<string, PhysicalFileProvider> cache = new ConcurrentDictionary<string, PhysicalFileProvider>();
        private readonly ILogger<FileManager> logger;

        public FileManager(IOptions<FileManagerOptions> fileManagerOptions, ILogger<FileManager> logger)
        {
            if (fileManagerOptions.Value.BasePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"'{nameof(fileManagerOptions.Value.BasePath)}' property cannot be null or whitespace", nameof(fileManagerOptions));
            }

            Options = fileManagerOptions;
            FullBasePath = Path.GetFullPath(Options.Value.BasePath);
            this.logger = logger;
        }

        public IOptions<FileManagerOptions> Options { get; }
        public string FullBasePath { get; }

        public PhysicalFileProvider GetFileProvider(string subpath)
        {
            return cache.GetOrAdd(subpath, subpath =>
            {
                if (string.IsNullOrWhiteSpace(subpath))
                {
                    throw new ArgumentException($"'{nameof(subpath)}' cannot be null or whitespace", nameof(subpath));
                }
                else if (!Regex.IsMatch(subpath, "^[A-Za-z0-9]+$"))
                {
                    throw new ArgumentException($"The '{subpath}' {nameof(subpath)} can only contain letters and numbers.", nameof(subpath));
                }

                PhysicalFileProvider fileProvider = new PhysicalFileProvider(Path.GetFullPath(subpath, FullBasePath));

                try
                {
                    if (!Directory.Exists(fileProvider.Root))
                    {
                        Directory.CreateDirectory(fileProvider.Root);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Suppressed {ex.GetType()} trying to read or create the '{fileProvider.Root}' directory: {ex.Message}", ex);
                }

                return fileProvider;
            });
        }
    }
}
