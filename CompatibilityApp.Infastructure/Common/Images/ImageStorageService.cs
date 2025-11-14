using System.IO;
using System.Threading.Tasks;
using CompatibilityApp.Domain.Common.Images;

namespace CompatibilityApp.Infrastructure.Common.Images
{
    public sealed class FileSystemImageStorageService : IImageStorageService
    {
        private readonly string _root;

        public FileSystemImageStorageService(string webRootPath)
        {
            _root = Path.Combine(webRootPath, "images");
        }

        public async Task<string> SaveImageAsync(
            string category,
            int id,
            Stream content,
            string extension)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required.", nameof(category));

            if (string.IsNullOrWhiteSpace(extension))
                extension = ".png";

            if (!extension.StartsWith('.'))
                extension = "." + extension;

            var dir = Path.Combine(_root, category);
            Directory.CreateDirectory(dir);

            // 🔥 NEW: delete any existing files for this id, any extension
            var pattern = id.ToString() + ".*";
            foreach (var file in Directory.GetFiles(dir, pattern))
            {
                try { File.Delete(file); }
                catch { /* swallow – not fatal */ }
            }

            var fileName = $"{id}{extension}";
            var physicalPath = Path.Combine(dir, fileName);

            await using (var fs = File.Create(physicalPath))
            {
                await content.CopyToAsync(fs);
            }

            // what you store in ImagePath
            return $"/images/{category}/{fileName}";
        }

        public Task DeleteImageAsync(string category, int id)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required.", nameof(category));

            var dir = Path.Combine(_root, category);
            if (!Directory.Exists(dir))
                return Task.CompletedTask;

            var pattern = id.ToString() + ".*";
            foreach (var file in Directory.GetFiles(dir, pattern))
            {
                try { File.Delete(file); }
                catch { /* ignore */ }
            }

            return Task.CompletedTask;
        }
    }
}
