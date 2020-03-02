namespace Connect
{
    public class Move
    {
        public Move(Player player, int columnToPlaceToken)
        {
            Player = player;
            ColumnToPlaceToken = columnToPlaceToken;
        }

        public Player Player { get; }
        public int ColumnToPlaceToken { get; }
    }
}