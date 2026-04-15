using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Testing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly string _basePath = @"\\100.100.100.38\igso\Upload\";

        [HttpGet("viewpdf")]
        [HttpHead("viewpdf")]
        public IActionResult ViewPdf([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest(new { error = "Path is required" });
                }

                // Normalize path: replace forward slash with backslash
                string normalizedPath = path.Replace("/", "\\");

                // Extract directory and filename
                string fileName = Path.GetFileName(normalizedPath);          // e.g., "20220104006572.pdf"
                string directory = Path.GetDirectoryName(normalizedPath);   // e.g., "2\5\7\2022"

                // Build possible paths
                string[] possiblePaths = new string[]
                {
                    // 1. Original folder (preferred as per VB.NET code)
                    Path.Combine(directory, "Original", fileName),
                    
                    // 2. Directly in the year folder
                    normalizedPath
                };

                string? foundPath = null;

                foreach (var tryPath in possiblePaths)
                {
                    string fullPath = Path.Combine(_basePath, tryPath);
                    string fullPathReal = Path.GetFullPath(fullPath);
                    string basePathReal = Path.GetFullPath(_basePath);

                    // Security: ensure path stays within base folder
                    if (!fullPathReal.StartsWith(basePathReal))
                        continue;

                    if (System.IO.File.Exists(fullPathReal))
                    {
                        foundPath = fullPathReal;
                        break;
                    }
                }

                if (foundPath == null)
                {
                    return NotFound(new
                    {
                        error = "PDF file not found in Original or year folder",
                        searchedPaths = possiblePaths.Select(p => Path.Combine(_basePath, p))
                    });
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(foundPath);
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}