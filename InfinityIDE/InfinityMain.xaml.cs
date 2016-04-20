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
using System.Xml.Serialization;
using System.IO;
using InfinityIDE.IDE;
using Infinity.Engine;
using Infinity.Scripting;

namespace InfinityIDE
{
    /// <summary>
    /// Interaction logic for InfinityMain.xaml
    /// </summary>
    public partial class InfinityMain : Window
    {
        private NewProjectWindow _new_project_window;
        private InfinityProject _openproject;

        int _consoleIndex = 1;
        bool snapped = false;
        bool minimiseErrorCheck = false;
        public bool IsSnapped { get { return snapped; } }
        public InfinityMain()
        {
            InitializeComponent();
            _new_project_window = new NewProjectWindow();
            ProjectMenu.Visibility = System.Windows.Visibility.Collapsed;
            textBox.SelectionStart = textBox.Text.Length;
        }
        public void AddSourceFileNode(string path, string name)
        {
            TreeViewItem newfiletreeitem = new TreeViewItem();
            newfiletreeitem.Foreground = __SampleFileTreeItem.Foreground;
            newfiletreeitem.Style = __SampleFileTreeItem.Style;
            newfiletreeitem.Padding = __SampleFileTreeItem.Padding;
            newfiletreeitem.Margin = __SampleFileTreeItem.Margin;
            StackPanel headerPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock headerIcon = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Text = "",
                Padding = new Thickness(4, 0, 4, 0)
            };
            TextBlock headerLabel = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Text = name
            };
            headerPanel.Children.Add(headerIcon);
            headerPanel.Children.Add(headerLabel);
            newfiletreeitem.Header = headerPanel;
            newfiletreeitem.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            if(path == null)
                ProjectSources.Items.Add(newfiletreeitem);
        }
        public void AddSourceDirectoryNode(string path, string name)
        {
            TreeViewItem newfiletreeitem = new TreeViewItem();
            newfiletreeitem.Foreground = __SampleDirectoryTreeItem.Foreground;
            newfiletreeitem.Style = __SampleDirectoryTreeItem.Style;
            newfiletreeitem.Padding = __SampleDirectoryTreeItem.Padding;
            newfiletreeitem.Margin = __SampleDirectoryTreeItem.Margin;
            StackPanel headerPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            TextBlock headerIcon = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 218, 177, 115)),
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Text = "",
                Padding = new Thickness(4, 0, 4, 0)
            };
            TextBlock headerLabel = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Text = name
            };
            headerPanel.Children.Add(headerIcon);
            headerPanel.Children.Add(headerLabel);
            newfiletreeitem.Header = headerPanel;
            newfiletreeitem.ContextMenu = __SampleDirectoryTreeItem.ContextMenu;
            newfiletreeitem.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            if (path == null)
                ProjectSources.Items.Add(newfiletreeitem);
        }
        public void OpenProject(DirectoryInfo _proj_directory)
        {
            XmlSerializer xs = InfinityProject.GetSerializer();
            if (new FileInfo(_proj_directory.FullName + "\\" + _proj_directory.Name + ".iproj").Exists)
            {
                StreamReader sr = new StreamReader(_proj_directory.FullName + "\\" + _proj_directory.Name + ".iproj");
                if (sr.EndOfStream)
                {
                    Status.Content = "Error opening project file. Project might not have successfully created or saved.";
                }
                ProjectInfo pi = (ProjectInfo)xs.Deserialize(sr);
                ProjectExplorerRoot.Text = pi.Name;
                ProjectDirectoryTreeItem.Text = _proj_directory.FullName;
                ProjectTypeTreeItem.Text = pi.Type;
                ProjectSources.Items.Clear();
                for (int i = 0; i < pi.Children.Count; i++ )
                {
                    if(pi.Children[i].GetType() == typeof(InfinityIDE.IDE.Directory))
                        AddSourceDirectoryNode(null, pi.Children[i].Name);
                }
                for (int i = 0; i < pi.Children.Count; i++)
                {
                    if (pi.Children[i].GetType() == typeof(InfinityIDE.IDE.File))
                        AddSourceFileNode(null, pi.Children[i].Name);
                }
                _openproject = new InfinityProject() { Info = pi, ProjectFolder = _proj_directory };
            }
            ProjectMenu.Visibility = System.Windows.Visibility.Visible;
        }
        public void CreateFileInProject(string name)
        {
            FileInfo newfile = new FileInfo(_openproject.ProjectFolder.FullName + "\\" + name);
            newfile.Create();
            string ext = name.Substring(name.LastIndexOf('.') + 1);
            _openproject.Info.Children.Add(new InfinityIDE.IDE.File() { Name = newfile.Name, Type = ext });
            
            _openproject.Save();
        }
        public void CreateDirectoryInProject(string name)
        {
            DirectoryInfo _newdirectory = new DirectoryInfo(_openproject.ProjectFolder.FullName + "\\" + name);
            _newdirectory.Create();
            _openproject.Info.Children.Add(new InfinityIDE.IDE.Directory() { Name = _newdirectory.Name });

            _openproject.Save();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            UpdateWindowLayout();
            Console.WriteLine(WindowState);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateWindowLayout();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine(Top + ", " + Left);
            Console.WriteLine(SystemParameters.WorkArea.Width / 2);
            Console.WriteLine(ActualWidth);
            Console.WriteLine(ActualHeight);
            if ((Left == 0 && ActualWidth == SystemParameters.WorkArea.Width / 2 && ActualHeight == SystemParameters.WorkArea.Height)
                || (Left == SystemParameters.WorkArea.Width / 2 && ActualWidth == SystemParameters.WorkArea.Width / 2 && ActualHeight == SystemParameters.WorkArea.Height))
            {
                Thickness windowGridMargin = WindowGrid.Margin;
                windowGridMargin.Top = 0;
                windowGridMargin.Left = 0;
                windowGridMargin.Right = 0;
                windowGridMargin.Bottom = 0;
                WindowGrid.Margin = windowGridMargin;
                WindowDropShadow.BlurRadius = 0;
                snapped = true;
            }
            else
            {
                snapped = false;
                UpdateWindowLayout();
            }
        }
        public void UpdateWindowLayout()
        {
            Thickness _new_Error = new Thickness(7, 7, 5, 45);
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                Thickness windowGridMargin = WindowGrid.Margin;
                windowGridMargin.Top = 7;
                windowGridMargin.Left = 7;
                windowGridMargin.Right = 7;
                windowGridMargin.Bottom = 47;
                WindowGrid.Margin = (minimiseErrorCheck) ? _new_Error : windowGridMargin;
                WindowDropShadow.BlurRadius = 0;
            }
            else
            {
                WindowDropShadow.BlurRadius = 16;
                Thickness windowGridMargin = WindowGrid.Margin;
                windowGridMargin.Top = 32;
                windowGridMargin.Left = 32;
                windowGridMargin.Right = 32;
                windowGridMargin.Bottom = 32;
                WindowGrid.Margin = windowGridMargin;
                if (WindowState == System.Windows.WindowState.Minimized)
                    minimiseErrorCheck = true;
            }
        }
        private void TitleBarRectangle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void CloseInfinityButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MaximizeRestoreInfinityButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Normal)
                WindowState = System.Windows.WindowState.Maximized;
            else
                WindowState = System.Windows.WindowState.Normal;
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }
        private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _new_project_window.Owner = this;
            _new_project_window.Title = "New Project";
            _new_project_window.ShowDialog();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
        private void OpenProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileFolderPickerWindow _ffpw = new FileFolderPickerWindow(FileFolderPickerWindow.FileFolderPickerType.Files);
            _ffpw.Selected += FileToOpenFileSelected;
            _ffpw.Owner = this;
            _ffpw.Title = "Open Project";
            _ffpw.ShowDialog();
        }
        private void FileToOpenFileSelected(FileFolderPickerWindow window)
        {
            FileInfo fi = new FileInfo(window.SelectedFileOrFolder);
            DirectoryInfo projfolder = fi.Directory;
            OpenProject(projfolder);
        }
        private void ProjectNewSourceFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProjectNewFileWindow _pnfw = new ProjectNewFileWindow();
            _pnfw.Owner = this;
            _pnfw.Title = "New File";
            _pnfw.ShowDialog();
            OpenProject(_openproject.ProjectFolder);
        }
        private void ProjectNewSourceDirectoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProjectNewDirectoryWindow _pndw = new ProjectNewDirectoryWindow();
            _pndw.Owner = this;
            _pndw.Title = "New Directory";
            _pndw.ShowDialog();
            OpenProject(_openproject.ProjectFolder);
        }
        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string changed = textBox.Text.Substring(_consoleIndex).Trim();

            if (e.Key == Key.Enter)
            {
                Infinity.Engine.Script.ICASMExecutionResult result = Infinity.Engine.Script.ICASMInterpreter.Execute("/", changed.Split('\n','\r'));
                if (result.Data.ContainsKey("$IDE_OUTPUT$"))
                {
                    textBox.Text += (string)result.Data["$IDE_OUTPUT$"];
                }
                textBox.Text += "\r\n> ";
                _consoleIndex = textBox.Text.Length;
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        private void textBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
