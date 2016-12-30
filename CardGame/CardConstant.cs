namespace CardGame
{
    public class CardConstant
    {
        //Score options
        public static readonly int Pair = 1;
        public static readonly int TwoPair = 2;
        public static readonly int ThreeOfAKind = 3;
        public static readonly int Straight = 4;
        public static readonly int Flush = 5;

        public static readonly int FullHouse = 8;
        public static readonly int FourOfAKind = 25;
        public static readonly int StraightFlush = 50;
        public static readonly int RoyalFlush = 250;

        //Special cards
        public static readonly int A = 1;
        public static readonly int Ten = 10;
        public static readonly int Jack = 11;
        public static readonly int Queen = 12;
        public static readonly int King = 13;
        public static readonly int Ghosts = 14;

        public static readonly string Moon = "14s";
        public static readonly string Sun = "14b";
        public static readonly string MoonAsset = "14s.jpg";
        public static readonly string SunAsset = "14b.jpg";

        public static readonly int EndingPoint = 25;

        //Message content
        public const int Transform = 0x000001;
        public const int SubwinClosed = 0x000002;
        public const int CloseExhibition= 0x000003;

        //Game state
        public static readonly string NextOne = "Next One";
        public static readonly string Over = "Over";
        public static readonly string Trans = "Transformer";

        //Evaluation Level
        public static readonly string Default = "Error";
        public static readonly string LevelZero = "Give me a break!";
        public static readonly string LevelOne = "Not Bad!";
        public static readonly string LevelTwo = "Nice!";
        public static readonly string LevelThree = "Impressive!";
        public static readonly string LevelFour = "Awesome!";
        public static readonly string LevelFive = "Unbelievable！";
        public static readonly string LevelSix = "Most handsome in the world!";

    }

    public enum CouplesType
    {
        Null,
        Pair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind
    }
}
