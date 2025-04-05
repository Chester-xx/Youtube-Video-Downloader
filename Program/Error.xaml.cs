using System.Windows;
using System.Windows.Input;

namespace Program
{
    public partial class Error : Window
    {
        public Error()
        {
            InitializeComponent();
        }

        public static void Display(string title, string message)
        {
            Error instance = new Error();
            instance.lblTitle.Content = title;
            instance.lblMessage.Content = instance.FormatErrorString(message, 40);
            instance.lblMessage.ToolTip = instance.FormatHint(message);
            instance.ShowDialog();
        }

        private string FormatErrorString(string message, int l)
        {
            int ct = 0, cp = 0;
            for (int i = l; i < message.Length; i += l)
            {
                ct++;
                if (ct == 4)
                {
                    for (int a = 15; a > 0; a--)
                    {
                        if (message[a] == ' ')
                        {
                            message = message.Substring(0, cp) + "\n" + message.Substring(cp + 1, a) + "... [Hover for more]";
                            return message;
                        }
                    }
                }
                for (int c = i; c > i - l; c--)
                {
                    if (c < 0) break;
                    if (message[c] == ' ')
                    {
                        message = message.Substring(0, c) + "\n" + message.Substring(c + 1);
                        cp = c;
                        break;
                    }
                }
            }
            return message;
        }

        private string FormatHint(string hint)
        {
            for (int i = 25; i < hint.Length; i += 25)
            {
                for (int c = i; c > i - 25; c--)
                {
                    if (c < 0) break;
                    if (hint[c] == ' ')
                    {
                        hint = hint.Substring(0, c) + "\n" + hint.Substring(c + 1);
                        break;
                    }
                }
            }
            return hint;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
