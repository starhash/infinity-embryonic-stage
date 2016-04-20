using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace InfinityIDE
{
    /// <summary>
    /// Interaction logic for FileFolderPickerWindow.xaml
    /// </summary>
    public partial class FileFolderPickerWindow : Window
    {
        public enum FileFolderPickerType
        {
            Directories,
            Files
        }

        private FileFolderPickerType _type;
        public FileFolderPickerType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string SelectedFileOrFolder
        {
            get { return SelectedPath.Text; }
        }

        public delegate void SelectionEvent(FileFolderPickerWindow window);
        public event SelectionEvent Selected;

        public FileFolderPickerWindow(FileFolderPickerType type = FileFolderPickerType.Directories)
        {
            InitializeComponent();
            _type = type;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                DriveList.Items.Add(((drive.IsReady) ? " " + ((drive.VolumeLabel != null) ? drive.VolumeLabel + " (" : "")
                    + drive.Name.Trim('/', '\\') + ((drive.VolumeLabel != null) ? ")" : "") : " " + drive.DriveType));
            }
        }
        private void TitleBarRectangle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void CloseInfinityButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        string parentPath = "";
        string currentPath = "";
        private void DriveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DriveList.SelectedIndex != -1)
            {
                string drive = (string)DriveList.SelectedItem;
                if(drive.Contains("(") && !drive.StartsWith(""))
                {
                    drive = drive.Substring(drive.IndexOf('(') + 1, 2) + "\\";
                    parentPath = drive.Trim('\\');
                    SelectedPath.Text = drive;
                    FileFolderList.Items.Clear();
                    DirectoryInfo[] dirs = new DirectoryInfo[0];
                    FileInfo[] files = new FileInfo[0];
                    if (Type == FileFolderPickerType.Files)
                        files = new DriveInfo(drive[0] + "").RootDirectory.GetFiles();
                    if (Type == FileFolderPickerType.Directories || Type == FileFolderPickerType.Files)
                        dirs = new DriveInfo(drive[0] + "").RootDirectory.GetDirectories();
                    foreach (DirectoryInfo d in dirs)
                        FileFolderList.Items.Add(" " + d.Name);
                    foreach (FileInfo f in files)
                        FileFolderList.Items.Add(f.Name);
                }
                else
                {
                    FileFolderList.Items.Clear();
                }
            }
        }
        private void FileFolderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileFolderList.SelectedIndex != -1)
            {
                string sel = (string)FileFolderList.SelectedItem;
                sel = sel.Trim('', '', '').Trim();
                if (!currentPath.EndsWith(sel))
                {
                    currentPath = parentPath + "\\" + sel;
                    SelectedPath.Text = currentPath;
                    SelectedPath.CaretIndex = currentPath.Length - 1;
                }
            }
        }
        private void FileFolderList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileFolderList.SelectedIndex != -1)
            {
                ListFilesAndFolders();
            }
        }
        private void UpOneLevel_Click(object sender, RoutedEventArgs e)
        {
            if (currentPath.EndsWith(":"))
            {
                FileFolderList.Items.Clear();
                DriveList.Focus();
                DriveList.SelectedIndex = -1;
                SelectedPath.Text = "";
                parentPath = "";
            }
            else
            {
                if (currentPath.Contains("\\"))
                {
                    currentPath = currentPath.Substring(0, currentPath.LastIndexOf('\\'));
                }
                ListFilesAndFolders();
            }
            SelectedPath.Text = parentPath;
        }
        public void ListFilesAndFolders()
        {
            if (currentPath.Trim().Length != 0)
            {
                if (new DirectoryInfo(currentPath).Exists)
                {
                    FileFolderList.Items.Clear();
                    DirectoryInfo[] dirs = new DirectoryInfo[0];
                    FileInfo[] files = new FileInfo[0];
                    if (Type == FileFolderPickerType.Files)
                        files = new DirectoryInfo(currentPath).GetFiles();
                    if (Type == FileFolderPickerType.Directories || Type == FileFolderPickerType.Files)
                        dirs = new DirectoryInfo(currentPath).GetDirectories();
                    if (files.Length == 0 && dirs.Length == 0)
                        UpOneLevel_Click(null, null);
                    foreach (DirectoryInfo d in dirs)
                        FileFolderList.Items.Add(" " + d.Name);
                    foreach (FileInfo f in files)
                        FileFolderList.Items.Add(f.Name);
                    parentPath = currentPath;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_type == FileFolderPickerType.Directories)
                {
                    if (new DirectoryInfo(SelectedPath.Text).Exists)
                    {
                        Selected(this);
                        this.Hide();
                        this.Owner.Activate();
                    }
                }
                if (_type == FileFolderPickerType.Files)
                {
                    if (new FileInfo(SelectedPath.Text).Exists)
                    {
                        Selected(this);
                        this.Hide();
                        this.Owner.Activate();
                    }
                }
            }
            catch (NullReferenceException) { }
        }
    }
}
