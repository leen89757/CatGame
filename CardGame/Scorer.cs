using System;
using System.Linq;
using System.Collections.Generic;

namespace CardGame
{
    internal class Scorer
    {
        private const int COUNT = 5;

        public static List<int> ObtainScores(string[,] cardsArrays)
        {
            List<int> resultList = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            FigureRowsOut(cardsArrays, resultList);

            FigureColomnsOut(cardsArrays, resultList);

            FigureRightOppositeAngleOut(cardsArrays, resultList);

            FigureLeftOppositeAngleOut(cardsArrays, resultList);

            return resultList;
        }

        private static void FigureLeftOppositeAngleOut(string[,] cardsArrays, List<int> resultList)
        {
            //Left opposite angle score 
            bool leftOppAngleFull = true;
            List<string> listOppoLeftAngle = new List<string>();
            for (int center = 0; center < COUNT; center++)
            {
                listOppoLeftAngle.Add(cardsArrays[center, 4 - center]);
                if (cardsArrays[center, 4 - center] == string.Empty)
                {
                    leftOppAngleFull = false;
                    break;
                }
            }
            if (leftOppAngleFull)
            {
                resultList[11] = Algorithm(listOppoLeftAngle);
            }
        }

        private static void FigureRightOppositeAngleOut(string[,] cardsArrays, List<int> resultList)
        {
            //Right opposite angle score 
            bool rightOppAngleFull = true;
            List<string> listOppoRightAngle = new List<string>();
            for (int center = 0; center < COUNT; center++)
            {
                listOppoRightAngle.Add(cardsArrays[center, center]);
                if (cardsArrays[center, center] == string.Empty)
                {
                    rightOppAngleFull = false;
                    break;
                }
            }
            if (rightOppAngleFull)
            {
                resultList[5] = Algorithm(listOppoRightAngle);
            }
        }

        private static void FigureColomnsOut(string[,] cardsArrays, List<int> resultList)
        {
            //Colomn scores records  range from [6~10]
            for (int col = 0; col < COUNT; col++)
            {
                bool isColFull = true;
                List<string> listRow = new List<string>();
                for (int row = 0; row < COUNT; row++)
                {
                    listRow.Add(cardsArrays[row, col]);
                    if (cardsArrays[row, col] == string.Empty)
                    {
                        isColFull = false;
                        break;
                    }
                }
                if (isColFull)
                {
                    resultList[col + 6] = Algorithm(listRow);
                }
            }
        }

        private static void FigureRowsOut(string[,] cardsArrays, List<int> resultList)
        {
            //Row scores records: range from [0~4]
            for (int row = 0; row < COUNT; row++)
            {
                bool isRowFull = true;
                List<string> lists = new List<string>();
                for (int col = 0; col < COUNT; col++)
                {
                    lists.Add(cardsArrays[row, col]);
                    if (cardsArrays[row, col] == string.Empty)
                    {
                        isRowFull = false;
                        break;
                    }

                }
                if (isRowFull)
                {
                    resultList[row] = Algorithm(lists);
                }
            }
        }

        private static int Algorithm(List<string> lists)
        {
            int score = 0;

            var couplesType = ObtainCouplesType(lists);
            switch (couplesType)
            {
                case CouplesType.Null:
                    break;
                case CouplesType.Pair:
                    score = CardConstant.Pair;
                    break;
                case CouplesType.TwoPair:
                    score = CardConstant.TwoPair;
                    break;
                case CouplesType.ThreeOfAKind:
                    score = CardConstant.ThreeOfAKind;
                    break;
                case CouplesType.FullHouse:
                    score = CardConstant.FullHouse;
                    break;
                case CouplesType.FourOfAKind:
                    score = CardConstant.FourOfAKind;
                    break;
                default:
                    throw new NullReferenceException();
            }

            bool isFlush = Flush(lists);

            var valList = AcquireSortedVal(lists);
            bool isStraight = Straight(valList);

            if (isStraight)
                score = CardConstant.Straight;
            if (isFlush)
                score = couplesType == CouplesType.FourOfAKind ?
                    score : CardConstant.Flush;
            if (isFlush && isStraight)
            {
                if (RoyalStraight(valList))
                    score = CardConstant.RoyalFlush;
                else
                    score = CardConstant.StraightFlush;
            }

            return score;
        }

        private static List<int> AcquireSortedVal(List<string> lists)
        {
            var valList = new List<int>();
            lists.ForEach(p =>
            {
                var val = 0;
                if ((int.TryParse(p.Substring(0, p.Length - 1), out val)))
                {
                    valList.Add(val);
                }
            });
            valList.Sort();
            return valList;
        }

        private static bool Straight(List<int> lists)
        {
            if (RoyalStraight(lists)) return true;

            bool isStraight = true;
            for (int i = 0; i + 1 < COUNT; i++)
            {
                if (lists[i] != lists[i + 1] - 1)
                {
                    isStraight = false;
                    break;
                }
            }

            var lastCard = lists[4];
            if (lastCard == CardConstant.Ghosts)
            {
                isStraight = false;
            }

            return isStraight;
        }

        private static bool RoyalStraight(List<int> lists)
        {
            if (lists[0] == CardConstant.A
                && lists[1] == CardConstant.Ten
                && lists[2] == CardConstant.Jack
                && lists[3] == CardConstant.Queen
                && lists[4] == CardConstant.King)
                return true;
            return false;
        }

        private static bool Flush(List<string> lists)
        {
            for (int i = 0; i < COUNT; i++)
            {
                var firstFlower = lists[0].Substring(lists[0].Length - 1, 1);
                if (firstFlower != lists[i].Substring(lists[i].Length - 1, 1))
                    return false;
            }

            return true;
        }

        private static CouplesType ObtainCouplesType(List<string> lists)
        {
            //0 represents the amount of specific card in current position 
            int[] positions = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            List<string> lineNums = new List<string>();
            for (int i = 0; i < COUNT; i++)
            {
                var current = lists[i].Substring(0, lists[i].Length - 1);
                lineNums.Add(current);
            }

            //Place card in relative position
            lineNums.ForEach(card =>
            {
                positions[int.Parse(card) - 1]++;
            });

            //Record count of two pair and three of a kind
            int countOfBrother = 0;
            int countOfSwornBrothers = 0;
            for (int i = 0; i < positions.Count(); i++)
            {
                if (positions[i] == 2)
                    ++countOfBrother;
                if (positions[i] == 3)
                    ++countOfSwornBrothers;
            }

            return JudgeCouples(countOfBrother, positions.Max());
        }

        private static CouplesType JudgeCouples(int countOfBrother, int maxRepeatTimes)
        {
            switch (maxRepeatTimes)
            {
                case 4:
                    return CouplesType.FourOfAKind;
                case 3:
                    return countOfBrother == 1 ? CouplesType.FullHouse : CouplesType.ThreeOfAKind;
                case 2:
                    return countOfBrother == 2 ? CouplesType.TwoPair : CouplesType.Pair;
                default:
                    return CouplesType.Null;
            }
        }
    }
}

