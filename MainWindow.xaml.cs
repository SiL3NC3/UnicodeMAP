using UnicodeMAP.Logic;
using UnicodeMAP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Threading;

namespace UnicodeMAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string BaseTitle = "UnicodeMAP ";
        private bool IsStarting = true;
        private bool IsDoubleClick = false;

        private object SelectedTile = null;
        private bool IsCtrlKeyPressed = false;

        private EmojiParser EmojiParser = new EmojiParser();
        private UnicodeParser UnicodeParser = new UnicodeParser();

        //public List<EmojiCharacter> Emojis { get; set; }
        //public List<UnicodeCharacter> Characters { get; set; }

        public string Status
        {
            get => tbStatus.Text;
            set => tbStatus.Text = value;
        }
        public DispatcherTimer ClickWaitTimer { get; set; }

        // CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();

            InitializeClickWaitTimer();

            // Startup in Rendered() funtion
        }

        // METHODS
        private void InitializeClickWaitTimer()
        {
            ClickWaitTimer = new DispatcherTimer(DispatcherPriority.Background);
            ClickWaitTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            ClickWaitTimer.Tick += ClickWaitTimer_Tick;
        }

        private async void Rendered()
        {
            IsStarting = false;

            // Startup
            UIHelper.SetBusyState();

            if (!CheckParserFilesExist())
            {
                ShowInfo("Required Unicode Data files are not present." + Environment.NewLine +
                    "Trying to download...");

                Clear();

                try
                {
                    ShowOverlay();
                    Status = $"Downloading fresh Unicode files... Please wait.";
                    Debug.WriteLine($"Downloading fresh Unicode files... Please wait.");

                    if (await DownloadUnicodeFiles())
                    {
                        Status = "Download successful. :)";
                        ShowInfo("Download successful. Click OK to reload...");

                        LoadParserFiles();

                        TxSearch.Focus();
                        RbSelectUnicode.IsChecked = true;

                        Run();
                    }
                    else
                    {
                        Status = "Download failed. Unicode Data files still missing... :(";

                        ShowInfo("- Download unsuccessful -" + Environment.NewLine + Environment.NewLine +
                            "An Exception occurred during downloading!" + Environment.NewLine + Environment.NewLine +
                            "Please check your firewall or internet connection " + Environment.NewLine +
                            "and try updating again.");

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception thrown: 'Download failed'");
                    Debug.WriteLine(ex);
                }
                finally
                {
                    ShowOverlay(false);
                }
            }
            else
            {
                LoadParserFiles();

                TxSearch.Focus();
                RbSelectUnicode.IsChecked = true;

                Run();
            }

        }
        private void ShowOverlay(bool show = true)
        {
            if (show)
                Overlay.Visibility = Visibility.Visible;
            else
                Overlay.Visibility = Visibility.Collapsed;
        }
        private bool CheckParserFilesExist()
        {
            bool parserFilesExisting = true;

            if (!UnicodeParser.CheckFilesExist())
            {
                parserFilesExisting = false;
            }

            if (!EmojiParser.CheckFilesExist())
            {
                parserFilesExisting = false;
            }

            return parserFilesExisting;
        }
        private void LoadParserFiles()
        {
            // Check and try to download if Unicode files are not existing
            if (CheckParserFilesExist())
            {
                UnicodeParser.LoadData();
                EmojiParser.LoadData();
                GetUnicodeVersion();
            }
            else
            {
                ShowInfo("Required Unicode Data files are not present!" + Environment.NewLine
                    + "Please click the Update button to try again downloading.");
            }
        }
        private void GetUnicodeVersion()
        {
            var versionBlocksArray = UnicodeParser.UnicodeBlocks[0]
                .Replace("# ", null)
                .Replace(Path.GetFileNameWithoutExtension(UnicodeParser.UnicodeBlockFile.FilePath), null)
                .Replace("-", null)
                .Split('.');
            var versionBlocks = new Version(int.Parse(versionBlocksArray[0]), int.Parse(versionBlocksArray[1]));

            var versionPropertiesArray = UnicodeParser.UnicodeProperties[0]
                .Replace("# ", null)
                .Replace(Path.GetFileNameWithoutExtension(UnicodeParser.UnicodePropertiesFile.FilePath), null)
                .Replace("-", null)
                .Split('.');
            var versionProperties = new Version(int.Parse(versionPropertiesArray[0]), int.Parse(versionPropertiesArray[1]));

            var versionEmojiArray = EmojiParser.EmojiUnicode[7]
              .Replace("# Version: ", null)
              .Split('.');
            var versionEmoji = new Version(int.Parse(versionPropertiesArray[0]), int.Parse(versionPropertiesArray[1]));

            if (versionBlocks != versionProperties ||
                versionBlocks != versionEmoji ||
                 versionProperties != versionEmoji)
            {
                throw new Exception($"GetUnicodeversion: Versions mismatch! " +
                    $"(Blocks:{versionBlocks.ToString()}, " +
                    $"Properties:{versionProperties.ToString()}, " +
                    $"Emoji:{versionEmoji.ToString()})");
            }

            Title = BaseTitle + $"[Unicode Version v{versionProperties.Major}.{versionProperties.Minor}]";
        }
        private async Task<bool> DownloadUnicodeFiles()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            try
            {
                await Task.Run(() =>
                {
                    // Rename existing (as fallback)
                    if (UnicodeParser.CheckFilesExist())
                        UnicodeParser.BackupFiles();
                    if (EmojiParser.CheckFilesExist())
                        EmojiParser.BackupFiles();

                    // Try to download
                    UnicodeParser.DownloadData();
                    EmojiParser.DownloadData();
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (UnicodeParser.CheckFilesExist())
                    UnicodeParser.RemoveBackupFiles();
                else
                    UnicodeParser.BackupFiles(true);

                if (EmojiParser.CheckFilesExist())
                    EmojiParser.RemoveBackupFiles();
                else
                    EmojiParser.BackupFiles(true);


            }
        }

        private void Run()
        {
            try
            {
                Debug.WriteLine("Run()");



                //if (CheckParserFilesExist())
                //{
                UIHelper.SetBusyState();
                Clear();

                if (RbSelectEmoji.IsChecked.Value)
                {
                    //EmojiParser.Clear();
                    //EmojiParser.LoadData();
                    if (EmojiParser.EmojiUnicode != null)
                    {
                        EmojiParser.ParseEmojis();

                        PopulateEmojiGroups();
                        PopulareEmojiSubgroups();
                        PopulateEmojis();
                    }
                }
                else if (RbSelectUnicode.IsChecked.Value)
                {
                    //EmojiParser.Clear();
                    //UnicodeParser.LoadData();
                    if (UnicodeParser.UnicodeData != null && UnicodeParser.UnicodeBlocks != null && UnicodeParser.UnicodeProperties != null)
                    {
                        UnicodeParser.ParseBlocks();
                        UnicodeParser.ParseProperties();
                        UnicodeParser.ParseCharacters();

                        PopulateUnicodeBlocks();
                        PopulateUnicodeGroups();
                        PopulareUnicodeSubgroups();
                        PopulateCharacters();
                    }
                }
                //}
                //else
                //{
                //    Status = "Required Unicode Data files are missing! :(";
                //}
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void ShowInfo(string msg, MessageBoxImage image = MessageBoxImage.Information)
        {
            MessageBox.Show(msg, "Information", MessageBoxButton.OK, image);
        }
        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Clear()
        {
            Debug.WriteLine("Clear()");
            CbBlocks.Items.Clear();
            CbGroups.Items.Clear();
            CbGroups.SelectedItem = null;
            CbSubgroups.Items.Clear();
            CbSubgroups.SelectedItem = null;
            PanelMapTiles.ItemsSource = null;
            //Emojis = null;
            //Characters = null;
        }
        private void Search(string text)
        {
            if (RbSelectEmoji.IsChecked.Value)
            {
                var emojis = EmojiParser.Emojis.Where(c => c.Name.ToLower().Contains(text.ToLower())).ToList();
                CbGroups.SelectedItem = null;
                CbSubgroups.SelectedItem = null;
                PopulateEmojis(emojis);
            }
            else if (RbSelectUnicode.IsChecked.Value)
            {
                var characters = UnicodeParser.Characters.Where(c => c.Name.ToLower().Contains(text.ToLower())).ToList();
                CbBlocks.SelectedItem = null;
                CbGroups.SelectedItem = null;
                CbSubgroups.SelectedItem = null;
                PopulateCharacters(characters);
            }
        }
        private void ResetSearchFilter()
        {
            TxSearch.Text = null;
            CbBlocks.SelectedItem = null;
            CbGroups.SelectedItem = null;
            CbSubgroups.SelectedItem = null;
            if (RbSelectEmoji.IsChecked.Value)
            {
                PopulateEmojis();
            }
            else if (RbSelectUnicode.IsChecked.Value)
            {
                PopulateCharacters();
            }
            TxSearch.Focus();
        }
        private void CopyToClipboard(string text = null)
        {
            IsCtrlKeyPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            //Unicode-Nummer: U + 0033
            //&#51;

            Clipboard.Clear();

            if (text != null)
            {
                Clipboard.SetDataObject(text);
                return;
            }

            if (RbSelectEmoji.IsChecked.Value && EmojiParser != null)
            {
                var selectedItem = (EmojiCharacter)SelectedTile;

                if (IsCtrlKeyPressed)
                {
                    Clipboard.SetDataObject(selectedItem.Code);
                }
                else
                {
                    Clipboard.SetDataObject(selectedItem.Icon);
                }
            }
            else if (RbSelectUnicode.IsChecked.Value && UnicodeParser != null)
            {
                var selectedItem = (UnicodeCharacter)SelectedTile;

                if (IsCtrlKeyPressed)
                {
                    Clipboard.SetDataObject(selectedItem.Code);
                }
                else
                {
                    Clipboard.SetDataObject(selectedItem.Icon);
                }
            }
            SelectedTile = null;
            PanelMapTiles.SelectedItem = null;
        }

        // EMOJIs
        private void PopulateEmojiGroups()
        {
            foreach (var group in EmojiParser.Groups)
            {
                CbGroups.Items.Add(group);
            }
        }
        private void PopulareEmojiSubgroups(EmojiGroup group = null)
        {
            CbSubgroups.Items.Clear();
            if (group != null)
            {
                var subgroups = EmojiParser.Subgroups.Where(s => s.Group.Name == group.Name).ToList();

                foreach (var subgroup in subgroups)
                {
                    CbSubgroups.Items.Add(subgroup);
                }
            }
            else
            {
                foreach (var subgroup in EmojiParser.Subgroups)
                {
                    CbSubgroups.Items.Add(subgroup);
                }
            }
        }
        private void PopulateEmojis(List<EmojiCharacter> emojis = null)
        {
            Debug.WriteLine($"PopulateEmojis({emojis})");
            UIHelper.SetBusyState();

            if (emojis == null)
            {
                if (!string.IsNullOrWhiteSpace(TxSearch.Text))
                {
                    Search(TxSearch.Text);
                    return;
                }
                else
                {
                    emojis = EmojiParser.Emojis.ToList();
                }
            }

            PanelMapTiles.ItemsSource = emojis;

            Status = $"{emojis.Count()} Emojies listed.";
        }
        private void PopulateEmojiStatus()
        {
        }

        // UNICODEs
        private void PopulateUnicodeBlocks()
        {
            foreach (var block in UnicodeParser.Blocks.OrderBy(p => p.Name).ToList())
            {
                CbBlocks.Items.Add(block);
            }
        }
        private void PopulateUnicodeGroups()
        {
            foreach (var property in UnicodeParser.Properties.Where(p => p.IsMainProperty).OrderBy(p => p.Name).ToList())
            {
                CbGroups.Items.Add(property);
            }
        }
        private void PopulareUnicodeSubgroups(UnicodeProperty mainProperty = null)
        {
            if (mainProperty != null && !mainProperty.IsMainProperty)
                throw new Exception("Main UnicodeProperty is not marked as IsMainPropery!");

            CbSubgroups.Items.Clear();
            if (mainProperty != null)
            {
                var subgroups = UnicodeParser.Properties.Where(
                        p => !p.IsMainProperty && mainProperty.LinkedProperties.Contains(p.Abbreviation)
                    ).ToList();

                foreach (var subgroup in subgroups)
                {
                    CbSubgroups.Items.Add(subgroup);
                }
            }
            else
            {
                var subgroups = UnicodeParser.Properties.Where(p => !p.IsMainProperty).OrderBy(p => p.Name).ToList();

                foreach (var subgroup in subgroups)
                {
                    CbSubgroups.Items.Add(subgroup);
                }
            }
        }
        private void PopulateCharacters(List<UnicodeCharacter> characters = null)
        {
            Debug.WriteLine($"PopulateCharacters({characters})");
            UIHelper.SetBusyState();

            if (characters == null)
            {
                if (!string.IsNullOrWhiteSpace(TxSearch.Text))
                {
                    Search(TxSearch.Text);
                    return;
                }
                else
                {
                    //characters = UnicodeParser.Characters.OrderBy(c => c.Name).ToList();
                    characters = UnicodeParser.Characters.ToList();
                }
            }

            PanelMapTiles.ItemsSource = characters;

            Status = $"{characters.Count()} Characters listed.";
        }

        // EVENTS
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Rendered();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetSearchFilter();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ResetSearchFilter();
            }
        }
        private void TxSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UIHelper.SetBusyState();
                Search(TxSearch.Text);
            }
        }

        private void FooterLink_Click(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink)sender;
            Process.Start(hyperlink.NavigateUri.AbsoluteUri);
        }

        private void RbSelectEmoji_Checked(object sender, RoutedEventArgs e)
        {
            RbSelectUnicode.IsChecked = false;
            RbSelectUnicode.IsEnabled = true;
            RbSelectEmoji.IsEnabled = false;
            GridBlocks.Visibility = Visibility.Collapsed;

            if (!IsStarting)
            {
                //Clear();
                Run();
            }
        }
        private void RbSelectUnicode_Checked(object sender, RoutedEventArgs e)
        {
            RbSelectEmoji.IsChecked = false;
            RbSelectEmoji.IsEnabled = true;
            RbSelectUnicode.IsEnabled = false;
            GridBlocks.Visibility = Visibility.Visible;

            if (!IsStarting)
            {
                //Clear();
                Run();
            }
        }

        private void CbBlocks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only visible at Unicode mode
            if (RbSelectUnicode.IsChecked.Value && UnicodeParser != null)
            {
                var block = (UnicodeBlock)((ComboBox)sender).SelectedItem;

                if (block != null)
                {
                    var characters = UnicodeParser.Characters.Where(c => block.CodePointRange.Contains(Helper.HexToInt(c.Code))).ToList();
                    PopulateCharacters(characters);
                    CbGroups.SelectedItem = null;
                    CbSubgroups.SelectedItem = null;
                    TxSearch.Text = null;
                }
            }
        }
        private void CbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RbSelectEmoji.IsChecked.Value && EmojiParser != null)
            {
                var group = (EmojiGroup)((ComboBox)sender).SelectedItem;
                if (group != null)
                {
                    PopulareEmojiSubgroups(group);
                    var emojis = EmojiParser.Emojis.Where(c => c.Group == group).ToList();
                    PopulateEmojis(emojis);
                    TxSearch.Text = null;
                }
            }
            else if (RbSelectUnicode.IsChecked.Value && UnicodeParser != null)
            {
                var property = (UnicodeProperty)((ComboBox)sender).SelectedItem;

                if (property != null)
                {
                    CbBlocks.SelectedItem = null;
                    PopulareUnicodeSubgroups(property);
                    var characters = UnicodeParser.Characters.Where(c => property.LinkedProperties.Contains(c.Property.Abbreviation)).ToList();
                    PopulateCharacters(characters);
                    TxSearch.Text = null;
                }
            }
        }
        private void CbSubgroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RbSelectEmoji.IsChecked.Value && EmojiParser != null)
            {
                var subgroup = (EmojiSubgroup)((ComboBox)sender).SelectedItem;
                if (subgroup != null)
                {
                    var emojis = EmojiParser.Emojis.Where(c => c.Subgroup == subgroup).ToList();
                    PopulateEmojis(emojis);
                    TxSearch.Text = null;
                }
            }
            else if (RbSelectUnicode.IsChecked.Value && UnicodeParser != null)
            {
                CbBlocks.SelectedItem = null;
                var subgroup = (UnicodeProperty)((ComboBox)sender).SelectedItem;
                if (subgroup != null)
                {
                    var characters = UnicodeParser.Characters.Where(c => c.Property == subgroup).ToList();
                    PopulateCharacters(characters);
                    TxSearch.Text = null;
                }
            }
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowOverlay();
                Status = "Downloading fresh Unicode files... Please wait.";
                Debug.WriteLine($"Downloading fresh Unicode files... Please wait.");

                Clear();

                //Characters = null;
                //Emojis = null;

                await DownloadUnicodeFiles();

                LoadParserFiles();

                Status = "Download successful.";
                ShowInfo("Download successful. Click OK to reload...");
                Run();

            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                ShowOverlay(false);
            }
        }

        private void PanelMapTiles_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDoubleClick)
            {
                SelectedTile = PanelMapTiles.SelectedItem;
                ClickWaitTimer.Start();
            }
        }
        private void PanelMapTiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("CopyToTextbox()");
            IsDoubleClick = true;
            TxText.Text += ((Character)SelectedTile).Icon;
        }
        private void ClickWaitTimer_Tick(object sender, EventArgs e)
        {
            ClickWaitTimer.Stop();

            if (!IsDoubleClick)
            {
                Debug.WriteLine("CopyToClipboard()");
                CopyToClipboard();
            }
            IsCtrlKeyPressed = false;
            IsDoubleClick = false;
        }

        private void GridTile_MouseEnter(object sender, MouseEventArgs e)
        {
            var gridTile = (Grid)sender;
            Preview.Visibility = Visibility.Visible;
            Preview.DataContext = gridTile.DataContext;
        }
        private void GridTile_MouseLeave(object sender, MouseEventArgs e)
        {
            var gridTile = (Grid)sender;
            Preview.Visibility = Visibility.Collapsed;
            Preview.DataContext = null;
        }

        private void BtnTextReset_Click(object sender, RoutedEventArgs e)
        {
            TxText.Text = null;
        }
        private void BtnTextCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(TxText.Text);
        }
    }
}
