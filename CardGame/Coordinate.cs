namespace CardGame
{
    public class Coordinate
    {
        public Coordinate(int row, int colomn)
        {
            this.Row = row;
            this.Column = colomn;
        }

        private int row;
        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        private int column;
        public int Column
        {
            get { return column; }
            set { column = value; }
        }

    }
}