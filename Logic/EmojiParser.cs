using UnicodeMAP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UnicodeMAP.Logic
{
    public class EmojiParser
    {
        // Static setting
        const bool _parseOnlyFullyQulified = true;

        readonly UnicodeFile EmojiFile = new UnicodeFile(
            @"Data\emoji-test.txt",
            @"Data\emoji-test.backup",
            @"http://unicode.org/Public/emoji/15.0/emoji-test.txt");

        // UNICODE - PREFIXES
        const string _prefixGroup = "# group: ";
        const string _prefixSubgroup = "# subgroup: ";
        const string _prefixComment = "#";

        public List<string> EmojiUnicode { get; private set; }
        public List<EmojiCharacter> Emojis { get; private set; }
        public List<EmojiGroup> Groups { get; private set; }
        public List<EmojiSubgroup> Subgroups { get; private set; }
        public int CommentsCount { get; private set; }

        EmojiCharacter _currentEmoji = null;
        EmojiGroup _currentGroup = null;
        EmojiSubgroup _currentSubgroup = null;

        public EmojiParser()
        {
            Emojis = new List<EmojiCharacter>();
            Groups = new List<EmojiGroup>();
            Subgroups = new List<EmojiSubgroup>();
        }

        public void Clear()
        {
            EmojiUnicode = null;
            CommentsCount = 0;
            Emojis = new List<EmojiCharacter>();
            Groups = new List<EmojiGroup>();
            Subgroups = new List<EmojiSubgroup>();
        }
        public void LoadData()
        {
            EmojiUnicode = File.ReadAllLines(EmojiFile.FilePath).ToList();
        }
        public void ParseEmojis()
        {
            Emojis.Clear();
            Groups.Clear();
            Subgroups.Clear();
            CommentsCount = 0;

            foreach (string line in EmojiUnicode)
            {
                if (line.StartsWith(_prefixGroup))
                {
                    ParseGroup(line);
                }
                else if (line.StartsWith(_prefixSubgroup))
                {
                    ParseSubgroup(line);
                }
                else if (line.StartsWith(_prefixComment))
                {
                    CommentsCount++;
                    continue;
                }
                else if (string.IsNullOrWhiteSpace(line.Trim()))
                {
                    continue;
                }
                else
                {
                    ParseEmoji(line);
                }
            }

            Emojis = Emojis.OrderBy(e => e.Name).ToList();
            Groups = Groups.OrderBy(g => g.Name).ToList();
            Subgroups = Subgroups.OrderBy(g => g.Name).ToList();
        }

        private EmojiCharacter ParseEmoji(string line)
        {
            //1F4AC                                                  ; fully-qualified     # 💬 E0.6 speech balloon
            //1FAF1 1F3FB 200D 1FAF2 1F3FD                           ; fully-qualified     # 🫱🏻‍🫲🏽 E14.0 handshake: light skin tone, medium skin tone


            var code = line.Substring(0, 54).Trim();
            var qualifiedStatus = line.Substring(56, 20).Trim();
            var emojiData = line.Substring(79, line.Length - 79).Trim();

            var iconLength = emojiData.NthIndexOf(" ", 1);
            var icon = emojiData.Substring(0, iconLength).Trim();

            var versionIdx = emojiData.NthIndexOf(" ", 1);
            var versionLength = emojiData.NthIndexOf(" ", 2) - emojiData.NthIndexOf(" ", 1);
            var iconVersion = emojiData.Substring(versionIdx, versionLength).Trim();

            var textIdx = emojiData.NthIndexOf(" ", 2);
            var textLength = emojiData.Length - textIdx;
            var iconText = emojiData.Substring(textIdx, textLength).Trim();

            EmojiCharacter emoji = new EmojiCharacter(code, qualifiedStatus, icon, iconVersion, iconText, _currentGroup, _currentSubgroup);
            _currentEmoji = emoji;

            if (!_parseOnlyFullyQulified || emoji.QualifiedStatus == "fully-qualified")
            {
                Emojis.Add(emoji);
            }
            return emoji;
        }
        private EmojiGroup ParseGroup(string line)
        {
            string name = line.Replace(_prefixGroup, null).Trim();
            EmojiGroup group = new EmojiGroup(name);
            _currentGroup = group;
            Groups.Add(group);
            return group;
        }
        private EmojiSubgroup ParseSubgroup(string line)
        {
            string name = line.Replace(_prefixSubgroup, null).Trim();
            EmojiSubgroup subgroup = new EmojiSubgroup(name, _currentGroup);
            _currentSubgroup = subgroup;
            Subgroups.Add(subgroup);
            return subgroup;
        }

        public bool CheckFilesExist()
        {
            bool result = true;

            if (!File.Exists(EmojiFile.FilePath))
                result = false;

            return result;
        }
        public void BackupFiles(bool restore = false)
        {
            if (restore)
            {
                if (!File.Exists(EmojiFile.BackupFilePath))
                {
                    Debug.WriteLine("EmojiParser.Backup(restore): BackupFile is not existing!");
                    return;
                }
                File.Move(EmojiFile.BackupFilePath, EmojiFile.FilePath);

            }
            else
            {
                if (!File.Exists(EmojiFile.FilePath))
                {
                    Debug.WriteLine("EmojiParser.Backup(): DataFile is not existing!");
                    return;
                }
                File.Move(EmojiFile.FilePath, EmojiFile.BackupFilePath);
            }
        }
        public void RemoveBackupFiles()
        {
            File.Delete(EmojiFile.BackupFilePath);
        }
        public void DownloadData()
        {
            using (var webclient = new WebClient())
            {
                webclient.DownloadFile(EmojiFile.DownloadURL, EmojiFile.FilePath);
            }
        }
    }
}
