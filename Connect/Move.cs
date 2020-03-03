namespace Connect
{
    public class Move
    {
        public static readonly Move Default = new Move(null, 0);

        public Move(Player player, int columnToPlaceToken)
        {
            Player = player;
            ColumnToPlaceToken = columnToPlaceToken;
        }

        public Player Player { get; }
        public int ColumnToPlaceToken { get; }
    }
}