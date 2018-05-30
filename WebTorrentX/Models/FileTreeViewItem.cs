using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTorrentX.Models
{
    internal sealed class FileTreeViewItem
    {
        public int Level { get; set; }

        public string Name { get; set; }

        public List<FileTreeViewItem> SubItems { get; set; }
    }
}
