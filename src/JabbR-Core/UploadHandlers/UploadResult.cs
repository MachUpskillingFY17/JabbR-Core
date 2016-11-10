namespace JabbR_Core.UploadHandlers
{
    public class UploadResult
    {
        public string Identifier { get; set; }
        public string Url { get; set; }
        public bool UploadTooLarge { get; set; }
        public int MaxUploadSize { get; set; }
    }
}