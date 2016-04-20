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
    /// Interaction logic for ProjectNewDirectoryWindow.xaml
    /// </summary>
    public partial class ProjectNewDirectoryWindow : Window
    {
        public ProjectNewDirectoryWindow()
        {
            InitializeComponent();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            InfinityMain im = (InfinityMain)Owner;
            string name = FileName.Text;
            im.CreateDirectoryInProject(name);
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
