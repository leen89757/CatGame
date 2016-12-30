using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CardGame
{
    /// <summary>
    /// Interaction logic for MessageBoxX.xaml
    /// </summary>
    public partial class MessageBoxX : Window
    {
        private MessageBoxX()
        {
            InitializeComponent();
        }

        public new string Title
        {
            get { return this.lblTitle.Text; }
            set { this.lblTitle.Text = value; }
        }

        public string Message
        {
            get { return this.lblMsg.Text; }
            set { this.lblMsg.Text = value; }
        }

        public static bool? Show(string title, string msg )
        {
            var msgBox = new MessageBoxX
            {
                Title = title,
                Message = msg
            };
            return msgBox.ShowDialog();
        }

        private void Ok_Tapped(object sender, MouseButtonEventArgs e)
        {
            this.IsEnabled = false;
            dialog.OpacityMask = FindResource("ClosedBrush") as LinearGradientBrush;
            var storyboard = this.FindResource("ClosedStoryboard") as Storyboard;
            if (storyboard == null) return;
            storyboard.Completed += delegate { Close(); };

            storyboard.Begin();
        }

        private void Trag_Tapped(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
