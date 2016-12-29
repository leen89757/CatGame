using System.Collections.Generic;

namespace CardGame
{
    internal class Filter
    {
        public static List<string> NormalCards = new List<string>()
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

        public static List<string> FilterCards(string[,] cardsArrays)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    NormalCards.Remove(cardsArrays[i, j]);
                }
            }

            return NormalCards;
        }

    }
}
