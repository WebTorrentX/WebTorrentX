using Ragnar;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace WebTorrentX.Models
{
    internal sealed class TorrentFileInfo : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int fileindex;
        private TorrentHandle handle; 

        public string Name
        {
            get
            {
                string path = Path.Combine(handle.QueryStatus().SavePath, handle.TorrentFile.FileAt(fileindex).Path);
                return Path.GetFileName(path);
            }
        }

        public string FilePath
        {
            get
            {
                return Path.Combine(handle.QueryStatus().SavePath, handle.TorrentFile.FileAt(fileindex).Path);
            }
        }

        public double DownloadedPercent
        {
            get
            {                
                FileInfo info = new FileInfo(Path.Combine(handle.QueryStatus().SavePath, handle.TorrentFile.FileAt(fileindex).Path));
                if (info.Exists)
                {
                    long length = GetFileSizeOnDisk(info.FullName);
                    length = length >= info.Length ? info.Length : length;
                    return length * 100 / handle.TorrentFile.FileAt(fileindex).Size;
                }                    
                else return 0;
            }

        }

        public double Size
        {
            get
            {
                return handle.TorrentFile.FileAt(fileindex).Size / (1024 * 1024);
            }
        }

        public int Priority
        {
            get
            {
                return handle.GetFilePriority(fileindex);
            }
        }

        public int Index
        {
            get
            {
                return fileindex;
            }
        }

        public TorrentFileInfo(TorrentHandle handle, int fileindex)
        {
            this.fileindex = fileindex;
            this.handle = handle;
        }

        public void StopDownload()
        {
            handle.SetFilePriority(fileindex, 0);
        }

        public void ContinueDownload()
        {
            handle.SetFilePriority(fileindex, 1);
        }

        private long GetFileSizeOnDisk(string file)
        {
            FileInfo info = new FileInfo(file);
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = GetCompressedFileSizeW(file, out hosize);
            long size;
            size = (long)hosize << 32 | losize;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
           out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);

    }
}
