using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Services
{
    // actions for the image
    public class BTFileService : IBTFileService
    {
        private readonly string _defaultBTUserImgSrc = "/img/DefaultProjImg.png";
        private readonly string _defaultCompanyImgSrc = "/img/DefaultProjImg.png";
        private readonly string _defaultProjectImgSrc = "/img/DefaultProjImg.png";

        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB " };

        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage)
        {
            if (fileData == null || fileData.Length == 0)
            {
                switch (defaultImage)
                {
                    case 1:
                        return _defaultBTUserImgSrc;
                    case 2:
                        return _defaultCompanyImgSrc;
                    case 3:
                        return _defaultProjectImgSrc;
                }
            }

            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);

                string imageSrcString = string.Format($"data:{extension};base64, {imageBase64Data}");

                return imageSrcString;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }

        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");

            return $"/img/contenttype/{ext}.png";
        }
    }
}
