namespace MyShop.Client.Services.Interfaces
{
    public interface IImageService : IAPI
    {
        /// <summary>
        /// Upload image to server
        /// </summary>
        /// <param name="fileData">Raw file bytes</param>
        /// <param name="fileName">Original file name</param>
        /// <returns>Server returned file name</returns>
        Task<string> UploadImageAsync(byte[] fileData, string fileName);

        /// <summary>
        /// Download image from server
        /// </summary>
        /// <param name="fileName">File name returned from upload</param>
        /// <returns>Image file bytes</returns>
        Task<byte[]> DownloadImageAsync(string fileName);

        /// <summary>
        /// Get image names from Images string (separated by ;)
        /// </summary>
        /// <param name="imagesString">Images string like "img1.png;img2.png"</param>
        /// <returns>List of image names</returns>
        List<string> ParseImageNames(string imagesString);

        /// <summary>
        /// Download multiple images by parsing Images string
        /// </summary>
        /// <param name="imagesString">Images string like "img1.png;img2.png"</param>
        /// <returns>Dictionary of fileName -> imageBytes</returns>
        Task<Dictionary<string, byte[]>> DownloadImagesAsync(string imagesString);
    }
}
