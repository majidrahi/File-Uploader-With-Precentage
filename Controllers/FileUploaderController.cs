using Microsoft.AspNetCore.Mvc;

namespace FileUploaderWithPrecentage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploaderController : ControllerBase
    {
        private readonly IHostEnvironment environment;
        // This varibale is not necessary, you can use client side response checking instead 
        public static int ProgressPercentage { get; set; }

        public FileUploaderController(IHostEnvironment environment)
        {
            this.environment = environment;
        }

        [HttpPost, RequestSizeLimit(524288000)]//500 MB
        public async Task<IActionResult> Index(IList<IFormFile> files)
        {

            ProgressPercentage = 0;
            long totalBytes = files.Sum(f => f.Length);
            long totalReadBytes = 0;
            string path = Path.Combine(environment.ContentRootPath, "FileUploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var file in files)
            {
                var buffer = new byte[16 * 1024];
                var fileName = Path.GetFileName(file.FileName);

                using (FileStream output = System.IO.File.Create(Path.Combine(path, fileName)))
                {
                    using (Stream input = file.OpenReadStream())
                    {
                        int readBytes = 0;

                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;
                            ProgressPercentage = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                            //await Task.Delay(100); // It is only to make the process slower, you could delete this line!
                        }
                    }
                }
            }

            return Ok(new { message = "File has been successfully uploaded" });
        }

        [HttpPost("CheckProgress")]
        public ActionResult CheckProgress()
        {
            return this.Content(ProgressPercentage.ToString());
        }
    }


}
