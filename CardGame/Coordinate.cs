namespace CardGame
{
    public class Coordinate
    {
        public Coordinate(int row, int colomn)
        {
            this.Row = row;
            this.Column = colomn;
        }

        public int Row { get; set; }

        public int Column { get; set; }
    }
}