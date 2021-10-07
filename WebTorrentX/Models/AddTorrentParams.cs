namespace WebTorrentX.Models
{
    internal class AddTorrentParams
    {
        public string SaveFolder { get; internal set; }
        public string Url { get; internal set; }
        public string Filename { get; internal set; }
        public byte[] ResumeData { get; internal set; }
    }
}
