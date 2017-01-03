using CardGame.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CardGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Random _rand = new Random();

        private AnimationClock _clock = null;

        private int _overturnCounter;

        private IList<string> _cards;

        private readonly IList<int> _foretells = new List<int>();

        private readonly IList<int> _oldRandoms = new List<int>();

        private readonly MediaPlayer _player = new MediaPlayer();

        private static string[,] _cardsArrays = new string[5, 5] {{"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" }};


        public MainWindow()
        {
            InitializeComponent();
            PlayMusic();
            InitCards();
            PrepareCards();
            Overturn(NextImg, _foretells[_overturnCounter]);
            this.DataContext = this;
            _player.MediaEnded += MediaEnded;

            //Register for interacting with Exhibtition 
            Exhibition.MessageNotified += ReveiveMessage;
        }

        private void PlayMusic()
        {
            var day = DateTime.Now.Day;
            var uri = day % 2 != 0 ?
                @"Media\ISeeFire.m4a" : @"Media\ChinesePoker.mp3";
            _player.Open(new Uri(uri, UriKind.Relative));
            _player.Play();
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            PlayMusic();
        }

        private void PrepareCards()
        {
            #region Original
            for (int i = 0; i < 26; i++)
            {
                int index = GenarateNext();
                _foretells.Add(index);
            }
            #endregion

            #region test data 
            //foretells = new List<int>()
            //{
            //     52,53,50,49,28,
            //     33,38,42,46,1,
            //     3,5,7,9,11,13,
            //     15,17,18,19,20,
            //     21,22,19,50,36
            //};
            #endregion
        }

        private void InitCards()
        {
            _cards = new List<string>();

            string[] colors = { "s", "h", "c", "d" };
            for (int i = 1; i < 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var singleCard = i.ToString() + colors[j];
                    _cards.Add(singleCard);
                }
            }
            _cards.Add(CardConstant.Moon);
            _cards.Add(CardConstant.Sun);

            AssignBlankSource();
        }

        private void AssignBlankSource()
        {
            BlankSource = DateTime.Now.Second % 2 == 0 ?
                            new BitmapImage(new Uri(@"Assets/scare.jpg", UriKind.Relative)) :
                              new BitmapImage(new Uri(@"Assets/colorful.jpg", UriKind.Relative));
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            Button currentButton = sender as Button;
            if (currentButton != null)
            {
                Image img = (Image)currentButton.Content;

                if (_overturnCounter > 24)
                {
                    CurrentGhost = (Image)((sender as Button).Content);
                    CurrentGhost.Tag = currentButton.Name.Substring(3);
                    ExhibitAlternative();

                    currentButton.IsEnabled = false;
                }
                else
                {
                    var imageSrc = img.Source.ToString();
                    if (imageSrc.Substring(imageSrc.LastIndexOf("/", StringComparison.Ordinal) + 1) == CardConstant.MoonAsset
                        || imageSrc.Substring(imageSrc.LastIndexOf("/", StringComparison.Ordinal) + 1) == CardConstant.SunAsset)
                    {
                        return;
                    }

                    UpdateCardsCoordinate(currentButton);

                    Overturn(img, _foretells[_overturnCounter++]);
                    TryOverturn(NextImg, _foretells[_overturnCounter]);
                }
            }

            ComputeScore();
        }

        private void UpdateCardsCoordinate(Button currentButton)
        {
            int resRow = 0; int resCol = 0;
            var cardInHand = string.Empty;
            if (int.TryParse((currentButton.Name.Substring(3, 1)), out resRow)
                && int.TryParse((currentButton.Name.Substring(4, 1)), out resCol))
            {
                cardInHand = _cards[_foretells[_overturnCounter]];
                _cardsArrays[resRow - 1, resCol - 1] = cardInHand;
            }
            if (cardInHand != CardConstant.Sun
                && cardInHand != CardConstant.Moon)
            {
                currentButton.IsEnabled = false;
            }
        }

        private List<Coordinate> SearchForGhost(string[,] cardsArrays)
        {
            var coorGhosts = new List<Coordinate>();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (cardsArrays[i, j].Substring(0, cardsArrays[i, j].Length) == CardConstant.Sun
                        || cardsArrays[i, j].Substring(0, cardsArrays[i, j].Length) == CardConstant.Moon)
                    {
                        coorGhosts.Add(new Coordinate(i, j));
                    }
                }
            }

            return coorGhosts;
        }

        private void TryOverturn(Image nextImaage, int foreIndex)
        {
            if (_overturnCounter == CardConstant.EndingPoint)
            {
                nextImaage.Source = BlankSource;
                CheckOver();
            }
            else
            {
                Overturn(nextImaage, foreIndex);
            }
        }

        private void CheckOver()
        {
            if (GhostsCount(_cardsArrays) == 0)
            {
                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    NextText.Text = CardConstant.Over;
                });
                ComputeScore();
                StopOpacityAnimation();

                MessageBoxX.Show("Game Over", Evaluate());
            }
            else
            {
                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    NextText.Text = CardConstant.Trans;

                    StartOpacityAnimation();
                });
            }
        }

        private void StartOpacityAnimation()
        {
            if (_clock == null)
            {
                var opacityAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    AutoReverse = true,
                    FillBehavior = FillBehavior.HoldEnd,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                _clock = opacityAnimation.CreateClock();
                NextText.ApplyAnimationClock(TextBlock.OpacityProperty, _clock);
            }
        }

        private void StopOpacityAnimation()
        {
            _clock?.Controller?.Stop();
        }

        private string Evaluate()
        {
            if (ScoreSum < 10)
                return CardConstant.LevelZero;
            if (ScoreSum >= 10 && ScoreSum < 20)
                return CardConstant.LevelOne;
            if (ScoreSum >= 20 && ScoreSum < 30)
                return CardConstant.LevelTwo;
            if (ScoreSum >= 30 && ScoreSum < 50)
                return CardConstant.LevelThree;
            if (ScoreSum >= 50 && ScoreSum < 70)
                return CardConstant.LevelFour;
            if (ScoreSum >= 70 && ScoreSum < 100)
                return CardConstant.LevelFive;
            if (ScoreSum >= 100)
                return CardConstant.LevelSix;
            return CardConstant.Default;
        }

        private void Overturn(Image img, int foreIndex)
        {
            var imgUri = $@"Assets/{_cards[foreIndex]}.jpg";
            img.Source = new BitmapImage(new Uri(imgUri, UriKind.Relative));
        }

        private int GhostsCount(string[,] cardsArrays)
        {
            int ghostsAmount = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (cardsArrays[i, j] == string.Empty) break;

                    if (cardsArrays[i, j] == CardConstant.Moon ||
                        cardsArrays[i, j] == CardConstant.Sun)
                    {
                        ghostsAmount++;
                    }
                }
            }

            return ghostsAmount;
        }

        private void ReveiveMessage(object sender, MessageEventAgrs e)
        {
            switch (e.What)
            {
                case CardConstant.Transform:
                    Transform(e);
                    break;
                case CardConstant.SubwinClosed:
                    AvoidAbnormalClose();
                    break;
            }
        }

        private void Transform(MessageEventAgrs message)
        {
            InsertBack((string)message.Obj);
            TransformGhost((string)message.Obj);
            ComputeScore();
            EnsureGhostButtonDisabled();
        }

        private void EnsureGhostButtonDisabled()
        {
            var btnName = $"Btn{_currentGhost.Tag.ToString()}";
            VisualHelper.FindChild<Button>(this.WholeGrid, btnName).IsEnabled = false;
        }

        private void ComputeScore()
        {
            ScoreList = Scorer.ObtainScores(_cardsArrays);

            int curSum = 0;
            ScoreList.ForEach(p => curSum += p);

            ScoreSum = curSum;
        }

        private int GenarateNext()
        {
            while (true)
            {
                var index = _rand.Next(0, 54);
                if (_oldRandoms.Contains(index))
                    continue;

                _oldRandoms.Add(index);
                return index;
            }
        }

        #region member Property
        private static Image _currentGhost = new Image();
        private static Image CurrentGhost
        {
            get
            {
                return _currentGhost;
            }
            set
            {
                _currentGhost = value;
            }
        }

        private ImageSource _blankSource;
        public ImageSource BlankSource
        {
            get
            {
                return _blankSource;
            }
            set
            {
                _blankSource = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(BlankSource)));
            }
        }

        private ImageSource _nextImage;
        public ImageSource NextImage
        {
            get
            {
                return _nextImage;
            }
            set
            {
                _nextImage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(NextImage)));
            }
        }

        private List<int> _scoreList = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> ScoreList
        {
            get
            {
                return _scoreList;
            }
            set
            {
                _scoreList = value;
                PropertyChanged?.Invoke(this,
                       new PropertyChangedEventArgs(nameof(ScoreList)));
            }
        }

        private int _scoreSum;
        public int ScoreSum
        {
            get { return _scoreSum; }
            set
            {
                _scoreSum = value;
                PropertyChanged?.Invoke(this,
                  new PropertyChangedEventArgs(nameof(ScoreSum)));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExhibitAlternative()
        {
            var exhibition = new Exhibition(Filter.FilterCards(_cardsArrays)) {Owner = this};
            exhibition.Show();
        }

        public void TransformGhost(string imgUri)
        {
            var imageUri = $@"Assets/{imgUri}.jpg";
            CurrentGhost.Source = new BitmapImage(new Uri(imageUri, UriKind.Relative));

            CheckOver();
        }

        public void InsertBack(string candidate)
        {
            int row = 0; int col = 0;
            if (int.TryParse(CurrentGhost.Tag.ToString().Substring(0, 1), out row)
             && int.TryParse(CurrentGhost.Tag.ToString().Substring(1, 1), out col))
            {
                _cardsArrays[row - 1, col - 1] = candidate;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            TryCloseOwnedWinWow();

            //Reset data
            _overturnCounter = 0;
            NextText.Text = CardConstant.NextOne;

            _oldRandoms.Clear();
            _foretells.Clear();
            ResetNormalCards();
            ResetScoreRelative();
            AssignBlankSource();
            EnableAllButtons();

            //Restart Game
            PrepareCards();
            Overturn(NextImg, _foretells[_overturnCounter]);
        }

        private void TryCloseOwnedWinWow()
        {
            for (int i = 0; i < this.OwnedWindows.Count; i++)
            {
                var ownedWindow = this.OwnedWindows[i];
                ownedWindow?.Close();
            }
        }

        private void ResetScoreRelative()
        {
            ScoreSum = 0;
            ScoreList = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            _cardsArrays = new string[5, 5] {{"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" }};
        }

        private void EnableAllButtons()
        {
            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    var desName = $"Btn{i}{j}";
                    var curButton = VisualHelper.FindChild<Button>(this.WholeGrid, desName);

                    curButton.IsEnabled = true;
                    ((Image)curButton.Content).Source = BlankSource;
                }
            }
        }

        private void ResetNormalCards()
        {
            Filter.NormalCards = new List<string>()
            {
                "1c","1d","1h","1s",
                "2c","2d","2h","2s",
                "3c","3d","3h","3s",
                "4c","4d","4h","4s",
                "5c","5d","5h","5s",
                "6c","6d","6h","6s",
                "7c","7d","7h","7s",
                "8c","8d","8h","8s",
                "9c","9d","9h","9s",
                "10c","10d","10h","10s",
                "11c","11d","11h","11s",
                "12c","12d","12h","12s",
                "13c","13d","13h","13s"
            };
        }

        private void AvoidAbnormalClose()
        {
            var coor = SearchForGhost(_cardsArrays);

            coor.ForEach(p =>
            {
                var des = VisualHelper.FindChild<Button>(this.WholeGrid,
                    $"Btn{(p.Row + 1).ToString()}{(p.Column + 1).ToString()}");

                des.IsEnabled = true;
            });
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"ScoreInfo.txt");
        }
    }

}
