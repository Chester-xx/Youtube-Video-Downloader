using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using YoutubeExplode;
using Microsoft.Win32;

namespace Program
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Did the user enter a URL?
            if (GetURL() is bool noURL && noURL == true)
            {
                MessageBox.Show("Error", "Please enter a valid URL.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // If so, proceed
            var URL = Convert.ToString(GetURL());
            
            // Did the user select a directory?
            if (GetDir() is bool noDir && noDir == true)
            {
                MessageBox.Show("Error", "Please select a folder for your download.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // If so, proceed
            var DIR = Convert.ToString(GetDir());



        }
        public object GetURL()
        {
            // Test URL
            if (string.IsNullOrEmpty(txtURL.Text))
            {
                return true;
            }
            return txtURL.Text;
        }
        public object GetDir()
        {
            // pass code
        }

        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            // pass code
        }
    }
}