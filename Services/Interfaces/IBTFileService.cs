namespace BugTrackerMVC.Services.Interfaces
{
    // actions for uploading a file
    public interface IBTFileService
    {
        // convert file to a byte array
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        // convert byte array to a file
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage);

        // get file icon
        public string GetFileIcon(string file);

        // format file size
        public string FormatFileSize(long bytes);
    }
}
