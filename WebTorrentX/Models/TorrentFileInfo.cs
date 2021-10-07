using MonoTorrent.Client;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private TorrentManager manager;

        public string Name => Path.GetFileName(FilePath);

        public string FilePath => Path.Combine(manager.SavePath, manager.Torrent.Name, manager.Torrent.Files.ElementAt(fileindex).Path);

        public double DownloadedPercent
        {
            get
            {
                var info = new FileInfo(FilePath);
                if (info.Exists)
                {
                    return Math.Round(DownloadedSize * 100 / manager.Torrent.Files.ElementAt(fileindex).Length, 2);
                }
                else return 0;
            }
        }

        public double Size => manager.Torrent.Files.ElementAt(fileindex).Length / (1024 * 1024);

        public double DownloadedSize
        {
            get
            {
                var info = new FileInfo(FilePath);
                if (info.Exists)
                {
                    long length = GetFileSizeOnDisk(info.FullName);
                    length = length >= info.Length ? info.Length : length;
                    return length;
                }
                else return 0;
            }
        }

        public int Index => fileindex;

        public TorrentFileInfo(TorrentManager manager, int fileindex)
        {
            this.fileindex = fileindex;
            this.manager = manager;
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
            return (size + clusterSize - 1) / clusterSize * clusterSize;
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
