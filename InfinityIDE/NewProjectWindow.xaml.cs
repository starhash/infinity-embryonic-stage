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

namespace InfinityIDE
{
    public partial class NewProjectWindow : Window
    {
        FileFolderPickerWindow _ffpw;
        bool _yesShown;

        public NewProjectWindow()
        {
            InitializeComponent();
        }

        private void TitleBarRectangle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void CloseInfinityButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            this.Owner.Activate();
        }
        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_ffpw == null)
            {
                _ffpw = new FileFolderPickerWindow(FileFolderPickerWindow.FileFolderPickerType.Directories);
                _ffpw.Selected += _ffpw_Selected;
                _ffpw.Owner = this;
                _ffpw.Title = "Select folder for project ";
            }
            _ffpw.ShowDialog();
        }
        void _ffpw_Selected(FileFolderPickerWindow window)
        {
            ProjectDirectory.Text = window.SelectedFileOrFolder;
        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if(!_yesShown)
            {
                string pn = ProjectName.Text;
                if (pn.Length >= 16)
                    pn = pn.Substring(0, 13) + "...";
                Create.Content = "Yes, Create " + pn;
                _yesShown = true;
                No.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Progress.IsIndeterminate = true;
                ProjectInfo pi = new ProjectInfo
                {
                    Name = ProjectName.Text,
                    Created = DateTime.Now,
                    Type = (string)((ComboBoxItem)ProjectType.SelectedItem).Content
                };
                XmlSerializer xs = new XmlSerializer(typeof(ProjectInfo));
                DirectoryInfo pd = new DirectoryInfo(ProjectDirectory.Text + "\\" + pi.Name);
                if (!pd.Exists)
                    pd.Create();
                StreamWriter pinfo = new StreamWriter(pd.FullName + "\\" + pi.Name + ".iproj");
                xs.Serialize(pinfo, pi);
                pinfo.Close();
                InfinityMain im = (InfinityMain)Owner;
                im.OpenProject(pd);
                Progress.IsIndeterminate = false;
                this.Hide();
            }
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            _yesShown = false;
            Create.Content = "Create";
            No.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
