namespace BugTrackerMVC.Services.Interfaces
{
    // actions for uploading an image 
    public interface IImageService
    {
        // convert file to a byte array
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        // convert byte array to a file
        public string ConvertByteArrayToFile(byte[] fileData, string extension);
    }
}
