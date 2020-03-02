using System;

namespace Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect Four!");

            Console.WriteLine("Enter player one name:");
            string playerOneName = Console.ReadLine();

            Console.WriteLine("Enter player two name:");
            string playerTwoName = Console.ReadLine();

            Player playerOne = new Player(playerOneName, Token.RED);
            Player playerTwo = new Player(playerTwoName, Token.YELLOW);


            ConnectFour connectFour = new ConnectFour(5, 5, playerOne, playerTwo);

            Player winningPlayer;

            while (!connectFour.TryGetWinningPlayer(out winningPlayer))
            {
                Console.WriteLine(connectFour.ToString());

                Player currentPlayer = connectFour.CurrentPlayer;

                Console.WriteLine($"Enter a column number, player {currentPlayer}:");

                int columnNumber = int.Parse(Console.ReadLine());

                connectFour.Place(currentPlayer, columnNumber);

                connectFour.AdvanceToNextPlayer();
            }

            Console.WriteLine($"{winningPlayer.Name} wins!");
        }
    }
}
