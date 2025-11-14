using System.IO;
using System.Threading.Tasks;

namespace CompatibilityApp.Domain.Common.Images
{
    public interface IImageStorageService
    {
        /// <summary>
        /// Saves an image for a category/id.
        /// Any existing images for that id in that category are deleted first.
        /// Returns the web-relative path (e.g. "/images/person/12.png").
        /// </summary>
        Task<string> SaveImageAsync(
            string category,
            int id,
            Stream content,
            string extension);

        /// <summary>
        /// Deletes all images for a given category/id (any extension).
        /// Safe to call even if no file exists.
        /// </summary>
        Task DeleteImageAsync(string category, int id);
    }
}
