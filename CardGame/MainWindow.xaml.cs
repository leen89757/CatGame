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
        private Random rand = new Random();

        private AnimationClock clock = null;

        private int overturnCounter;

        private IList<string> cards;

        private IList<int> foretells = new List<int>();

        private IList<int> oldRandoms = new List<int>();

        private MediaPlayer player = new MediaPlayer();

        private static string[,] CardsArrays = new string[5, 5] {{"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" },
                                                                {"","","","","" }};


        public MainWindow()
        {
            InitializeComponent();
            //PlayMusic();
            InitCards();
            PrepareCards();
            Overturn(nextImg, foretells[overturnCounter]);
            this.DataContext = this;
            player.MediaEnded += MediaEnded;

            //Register for communicating with Exhibtition 
            Exhibition.MessageNotified += ReveiveMessage;
        }

        private void PlayMusic()
        {
            var day = DateTime.Now.Day;
            var uri = day % 2 != 0 ?
                @"Media\ISeeFire.m4a" : @"Media\ChinesePoker.mp3";
            player.Open(new Uri(uri, UriKind.Relative));
            player.Play();
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
                foretells.Add(index);
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
            cards = new List<string>();

            string[] colors = { "s", "h", "c", "d" };
            for (int i = 1; i < 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var singleCard = i.ToString() + colors[j];
                    cards.Add(singleCard);
                }
            }
            cards.Add(CardConstant.Moon);
            cards.Add(CardConstant.Sun);

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
            Image img = (Image)currentButton.Content;

            if (overturnCounter > 24)
            {
                CurrentGhost = (Image)((sender as Button).Content);
                CurrentGhost.Tag = currentButton.Name.Substring(3);
                ExhibitAlternative();

                currentButton.IsEnabled = false;
            }
            else
            {
                var imageSrc = img.Source.ToString();
                if (imageSrc.Substring(imageSrc.LastIndexOf("/") + 1) == CardConstant.MoonAsset
                    || imageSrc.Substring(imageSrc.LastIndexOf("/") + 1) == CardConstant.SunAsset)
                {
                    return;
                }

                UpdateCardsCoordinate(currentButton);

                Overturn(img, foretells[overturnCounter++]);
                TryOverturn(nextImg, foretells[overturnCounter]);
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
                cardInHand = cards[foretells[overturnCounter]];
                CardsArrays[resRow - 1, resCol - 1] = cardInHand;
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

        private void TryOverturn(Image nextImg, int foreIndex)
        {
            if (overturnCounter == CardConstant.EndingPoint)
            {
                nextImg.Source = BlankSource;
                CheckOver();
            }
            else
            {
                Overturn(nextImg, foreIndex);
            }
        }

        private void CheckOver()
        {
            if (GhostsCount(CardsArrays) == 0)
            {
                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    nextText.Text = CardConstant.Over;
                });
                ComputeScore();
                StopOpacityAnimation();

                MessageBoxX.Show("Game Over", Evaluate());
            }
            else
            {
                this.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    nextText.Text = CardConstant.Trans;

                    StartOpacityAnimation();
                });
            }
        }

        private void StartOpacityAnimation()
        {
            if (clock == null)
            {
                var opacityAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = 1,
                    AutoReverse = true,
                    FillBehavior = FillBehavior.HoldEnd,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                clock = opacityAnimation.CreateClock();
                nextText.ApplyAnimationClock(TextBlock.OpacityProperty, clock);
            }
        }

        private void StopOpacityAnimation()
        {
            clock?.Controller.Stop();
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
            var imgUri = string.Format(@"Assets/{0}.jpg", cards[foreIndex]);
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
                case CardConstant.TRANSFORM:
                    Transform(e);
                    break;
                case CardConstant.SUBWIN_CLOSED:
                    AvoidAbnormalClose();
                    break;
                default:
                    break;
            }
        }

        private void Transform(MessageEventAgrs message)
        {
            InsertBack((string)message.Obj);
            TransformGhost((string)message.Obj);
            ComputeScore();
            EnsureDesButtonDisabled();
        }

        private void EnsureDesButtonDisabled()
        {
            var btnName = String.Format("btn{0}", currentGhost.Tag.ToString());
            VisualHelper.FindChild<Button>(this.wholeGrid, btnName).IsEnabled = false;
        }

        private void ComputeScore()
        {
            ScoreList = Scorer.ObtainScores(CardsArrays);

            int curSum = 0;
            ScoreList.ForEach(p => curSum += p);

            ScoreSum = curSum;
        }

        private int GenarateNext()
        {
            int index = rand.Next(0, 54);
            if (oldRandoms.Contains(index))
            {
                return GenarateNext();
            }
            else
            {
                oldRandoms.Add(index);
                return index;
            }
        }

        #region member Property
        private static Image currentGhost = new Image();
        private static Image CurrentGhost
        {
            get
            {
                return currentGhost;
            }
            set
            {
                currentGhost = value;
            }
        }

        private ImageSource blankSource;
        public ImageSource BlankSource
        {
            get
            {
                return blankSource;
            }
            set
            {
                blankSource = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("BlankSource"));
            }
        }

        private ImageSource nextImage;
        public ImageSource NextImage
        {
            get
            {
                return nextImage;
            }
            set
            {
                nextImage = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs("NextImage"));
            }
        }

        private List<int> scoreList = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> ScoreList
        {
            get
            {
                return scoreList;
            }
            set
            {
                scoreList = value;
                PropertyChanged?.Invoke(this,
                       new PropertyChangedEventArgs("ScoreList"));
            }
        }

        private int scoreSum;
        public int ScoreSum
        {
            get { return scoreSum; }
            set
            {
                scoreSum = value;
                PropertyChanged?.Invoke(this,
                  new PropertyChangedEventArgs("ScoreSum"));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void ExhibitAlternative()
        {
            var exhibition = new Exhibition(Filter.FilterCards(CardsArrays));
            exhibition.Owner = this;
            exhibition.Show();
        }

        public void TransformGhost(string imgUri)
        {
            var imageUri = string.Format(@"Assets/{0}.jpg", imgUri);
            CurrentGhost.Source = new BitmapImage(new Uri(imageUri, UriKind.Relative));

            CheckOver();
        }

        public void InsertBack(string candidate)
        {
            int row = 0; int col = 0;
            if (int.TryParse(CurrentGhost.Tag.ToString().Substring(0, 1), out row)
             && int.TryParse(CurrentGhost.Tag.ToString().Substring(1, 1), out col))
            {
                CardsArrays[row - 1, col - 1] = candidate;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            TryCloseOwnedWinWow();

            //Reset data
            overturnCounter = 0;
            nextText.Text = CardConstant.NextOne;

            oldRandoms.Clear();
            foretells.Clear();
            ResetNormalCards();
            ResetScoreRelative();
            AssignBlankSource();
            EnableAllButtons();

            //Restart Game
            PrepareCards();
            Overturn(nextImg, foretells[overturnCounter]);
        }

        private void TryCloseOwnedWinWow()
        {
            for (int i = 0; i < this.OwnedWindows.Count; i++)
                this.OwnedWindows[i].Close();
        }

        private void ResetScoreRelative()
        {
            ScoreSum = 0;
            ScoreList = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            CardsArrays = new string[5, 5] {{"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" },
                                            {"","","","","" }};
        }

        private void EnableAllButtons()
        {
            var desName = String.Empty;
            for (int i = 1; i < 6; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    desName = String.Format("btn{0}{1}", i.ToString(), j.ToString());
                    var curButton = VisualHelper.FindChild<Button>(this.wholeGrid, desName);

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
            var coor = SearchForGhost(CardsArrays);

            coor.ForEach(p =>
            {
                var des = VisualHelper.FindChild<Button>(this.wholeGrid,
                                   string.Format("btn{0}{1}", (p.Row + 1).ToString(),
                                   (p.Column + 1).ToString()));

                des.IsEnabled = true;
            });
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", @"ScoreInfo.txt");
        }
    }

}
