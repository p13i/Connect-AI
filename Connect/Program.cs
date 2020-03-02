using System;

namespace Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect Four!");

            Player playerOne = new Player("AI 1", Token.RED);
            Player playerTwo = new Player("AI 2", Token.YELLOW);


            ConnectFour connectFour = new ConnectFour(5, 5, playerOne, playerTwo);

            Player winningPlayer;

            while (!connectFour.TryGetWinningPlayer(out winningPlayer))
            {
                Console.WriteLine(connectFour.ToString());

                Player currentPlayer = connectFour.CurrentPlayer;

                Console.WriteLine($"Enter a column number, player {currentPlayer}:");

                Move nextAIMove = ConnectFourMinimaxExtensions.GetNextMove(connectFour);

                Console.WriteLine($"AI chose {nextAIMove.ColumnToPlaceToken}");

                connectFour.Place(new Move(currentPlayer, nextAIMove.ColumnToPlaceToken));
            }

            Console.WriteLine($"{winningPlayer.Name} wins!");
            Console.WriteLine(connectFour.ToString());
        }
    }
}
