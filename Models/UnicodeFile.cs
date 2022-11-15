using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicodeMAP.Models
{
    public class UnicodeFile
    {
        public string FilePath { get; set; }
        public string BackupFilePath { get; set; }
        public string DownloadURL { get; set; }

        public UnicodeFile(string filePath, string backupFilePath, string downloadURL)
        {
            FilePath = filePath;
            BackupFilePath = backupFilePath;
            DownloadURL = downloadURL;
        }
    }
}
