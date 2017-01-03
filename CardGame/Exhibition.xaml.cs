using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardGame
{
    /// <summary>
    /// Interaction logic for Exhibition.xaml
    /// </summary>
    public partial class Exhibition : Window
    {
        public static event EventHandler<MessageEventAgrs> MessageNotified;

        private string _checkedOne = string.Empty;

        public Exhibition(List<string> restsCards)
        {
            InitializeComponent();
            this.DataContext = this;
            this.RestsCards = restsCards;

            CheckIfVisible();
        }

        private void CheckIfVisible()
        {
            this.possibleRadioButton1.Visibility = RestsCards.Count > 27 ?
               Visibility.Visible : Visibility.Collapsed;
            this.possibleRadioButton2.Visibility = RestsCards.Count > 28 ?
                 Visibility.Visible : Visibility.Collapsed;
        }

        private void SendMessage(MessageEventAgrs e)
        {
            e.Raise(this, ref MessageNotified);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var cur = sender as RadioButton;
            if (cur != null)
                _checkedOne = RestsCards[int.Parse(cur.Tag.ToString())];
        }

        private void TransForm_Click(object sender, RoutedEventArgs e)
        {
            if (_checkedOne == string.Empty)
            {
                MessageBoxX.Show("Warm Prompt", "Please choose one card");
                return;
            }

            this.Close();

            //Back to home page
            SendMessage(new MessageEventAgrs(CardConstant.Transform, _checkedOne));
        }

        private List<string> _restsCards;
        public List<string> RestsCards
        {
            get
            {
                return _restsCards;
            }
            set
            {
                _restsCards = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RestsCards"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Window_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SendMessage(new MessageEventAgrs(CardConstant.SubwinClosed, null));
        }
    }
}
