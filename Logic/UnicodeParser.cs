using UnicodeMAP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnicodeMAP.Logic
{
    public class UnicodeParser
    {
        // SOURCES:
        // https://unicode.org/Public/UNIDATA/PropertyValueAliases.txt
        // https://unicode.org/Public/UNIDATA/UnicodeData.txt

        public readonly UnicodeFile UnicodeDataFile = new UnicodeFile(
            @"Data\UnicodeData.txt",
            @"Data\UnicodeData.backup",
            @"http://unicode.org/Public/UNIDATA/UnicodeData.txt"
            );
        public readonly UnicodeFile UnicodeBlockFile = new UnicodeFile(
            @"Data\Blocks.txt",
            @"Data\UnicodeBlocks.backup",
            @"http://unicode.org/Public/UNIDATA/Blocks.txt"
            );
        public readonly UnicodeFile UnicodePropertiesFile = new UnicodeFile(
            @"Data\PropertyValueAliases.txt",
            @"Data\PropertyValueAliases.backup",
            @"http://unicode.org/Public/UNIDATA/PropertyValueAliases.txt"
            );

        public List<string> UnicodeData { get; private set; }
        public List<string> UnicodeBlocks { get; private set; }
        public List<string> UnicodeProperties { get; private set; }

        public List<UnicodeProperty> Properties { get; private set; }
        public List<UnicodeBlock> Blocks { get; private set; }
        public List<UnicodeCharacter> Characters { get; private set; }

        public UnicodeParser()
        {
            Blocks = new List<UnicodeBlock>();
            Properties = new List<UnicodeProperty>();
            Characters = new List<UnicodeCharacter>();
        }

        public void Clear()
        {
            UnicodeData = null;
            UnicodeBlocks = null;
            UnicodeProperties = null;
            Blocks = new List<UnicodeBlock>();
            Properties = new List<UnicodeProperty>();
            Characters = new List<UnicodeCharacter>();
        }
        public void LoadData()
        {
            UnicodeData = File.ReadAllLines(UnicodeDataFile.FilePath).ToList();
            UnicodeBlocks = File.ReadAllLines(UnicodeBlockFile.FilePath).ToList();
            UnicodeProperties = File.ReadAllLines(UnicodePropertiesFile.FilePath).ToList();
        }
        public void ParseBlocks()
        {
            Blocks.Clear();

            foreach (var line in UnicodeBlocks)
            {
                if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else
                {
                    var segments = line.Split(";");
                    var codeRange = segments[0].Trim();
                    var name = segments[1].Trim();

                    var block = new UnicodeBlock(name, codeRange);
                    Blocks.Add(block);
                }
            }
        }

        public void ParseProperties()
        {
            Properties.Clear();

            foreach (var line in UnicodeProperties.Where(l => l.StartsWith("gc ; ")).ToList())
            {
                if (line.StartsWith("#") || String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else
                {
                    // Clear line prefix
                    var l = line.Replace("gc ; ", null);

                    // Get Abbreviation
                    var abbreviationEndIdx = l.IndexOf(";") - 1;
                    var abbreviation = l.Substring(0, abbreviationEndIdx).Trim();

                    // Get Name
                    var nameStartIdx = l.IndexOf(";") + 1;
                    var nameLength = 0;
                    if (l.Length > 68)
                    {
                        nameLength = 68 - nameStartIdx;
                    }
                    else
                    {
                        nameLength = l.Length - nameStartIdx;
                    }
                    var name = l.Substring(nameStartIdx, nameLength).Trim().Replace("_", " ");

                    // Check Is Main Category
                    var isMainProperty = l.Contains(" # ");
                    List<string> linkedProperties = null;
                    UnicodeProperty mainProperty = null;

                    if (isMainProperty)
                    {
                        var linkedStartIdx = l.IndexOf("#") + 1;
                        var linkedLength = l.Length - linkedStartIdx;
                        var linkedStr = l.Substring(linkedStartIdx, linkedLength).Trim();
                        linkedProperties = linkedStr.Split(" | ").ToList();
                    }

                    var property = new UnicodeProperty(abbreviation, name, isMainProperty, mainProperty, linkedProperties);
                    Properties.Add(property);
                }
            }
        }
        public void ParseCharacters()
        {
            Characters.Clear();

            foreach (var line in UnicodeData)
            {
                var cArray = line.Split(';');
                var property = Properties.Where(p => p.Abbreviation == cArray[2]).First();

                var character = new UnicodeCharacter(cArray[0], cArray[1], property);
                Characters.Add(character);
            }
        }

        public bool CheckFilesExist()
        {
            bool result = true;

            if (!File.Exists(UnicodeDataFile.FilePath))
                result = false;

            if (!File.Exists(UnicodeBlockFile.FilePath))
                result = false;

            if (!File.Exists(UnicodePropertiesFile.FilePath))
                result = false;

            return result;
        }
        public void BackupFiles(bool restore = false)
        {
            if (restore)
            {
                if (!File.Exists(UnicodeBlockFile.BackupFilePath) ||
                    !File.Exists(UnicodeDataFile.BackupFilePath) ||
                    !File.Exists(UnicodePropertiesFile.BackupFilePath))
                {
                    Debug.WriteLine("UnicodeParser.Backup(restore): Some or all BackupFiles are not existing!");
                    return;
                }
                File.Move(UnicodeBlockFile.BackupFilePath, UnicodeBlockFile.FilePath);
                File.Move(UnicodeDataFile.BackupFilePath, UnicodeDataFile.FilePath);
                File.Move(UnicodePropertiesFile.BackupFilePath, UnicodePropertiesFile.FilePath);
            }
            else
            {
                if (!File.Exists(UnicodeBlockFile.FilePath) ||
                    !File.Exists(UnicodeDataFile.FilePath) ||
                    !File.Exists(UnicodePropertiesFile.FilePath))
                {
                    Debug.WriteLine("UnicodeParser.Backup(): Some or all DataFiles are not existing!");
                    return;
                }
                File.Move(UnicodeBlockFile.FilePath, UnicodeBlockFile.BackupFilePath);
                File.Move(UnicodeDataFile.FilePath, UnicodeDataFile.BackupFilePath);
                File.Move(UnicodePropertiesFile.FilePath, UnicodePropertiesFile.BackupFilePath);
            }
        }
        public void RemoveBackupFiles()
        {
            File.Delete(UnicodeBlockFile.BackupFilePath);
            File.Delete(UnicodeDataFile.BackupFilePath);
            File.Delete(UnicodePropertiesFile.BackupFilePath);
        }
        public void DownloadData()
        {
            using (var webclient = new WebClient())
            {
                webclient.DownloadFile(UnicodeBlockFile.DownloadURL, UnicodeBlockFile.FilePath);
                webclient.DownloadFile(UnicodePropertiesFile.DownloadURL, UnicodePropertiesFile.FilePath);
                webclient.DownloadFile(UnicodeDataFile.DownloadURL, UnicodeDataFile.FilePath);
            }
        }
    }
}
