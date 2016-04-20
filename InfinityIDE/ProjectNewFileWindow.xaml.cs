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

namespace InfinityIDE
{
    /// <summary>
    /// Interaction logic for ProjectNewFileWindow.xaml
    /// </summary>
    public partial class ProjectNewFileWindow : Window
    {
        public ProjectNewFileWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            InfinityMain im = (InfinityMain)Owner;
            string type = (string)((ComboBoxItem)FileType.SelectedItem).Tag;
            string name = FileName.Text;
            im.CreateFileInProject(name + "." + type);
            this.Hide();
        }

        private void CloseInfinityButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
