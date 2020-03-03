using System;

namespace Connect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connect Four!");

            string playerOneName = ReadPlayerName(1);
            string playerTwoName = ReadPlayerName(2);

            Player playerOne = GetPlayerForName(playerOneName, 1);
            Player playerTwo = GetPlayerForName(playerTwoName, 2);

            int width = 7;
            int height = 6;

            ConnectFour connectFour = new ConnectFour(width, height, playerOne, playerTwo);

            Player winningPlayer;

            int round = 1;
            do
            {
                Console.WriteLine($"Round #{round++}");

                Console.WriteLine(connectFour.ToString());

                Move nextMove = connectFour.CurrentPlayer.GetNextMove();

                if (nextMove == null)
                {
                    winningPlayer = null;
                    break;
                }

                Console.WriteLine($"{connectFour.CurrentPlayer} chose {nextMove.ColumnToPlaceToken}");

                connectFour.Place(new Move(connectFour.CurrentPlayer, nextMove.ColumnToPlaceToken));
            
            } while (!connectFour.IsGameOver(out winningPlayer));

            Console.WriteLine(winningPlayer == null ? "Draw or AI fails to find solution!" : $"{winningPlayer.Name} wins!");
            Console.WriteLine(connectFour.ToString());
        }

        private static Player GetPlayerForName(string name, int playerNumber)
        {
            if (name.Contains("ai", StringComparison.InvariantCultureIgnoreCase))
            {
                return new MinimaxPlayer($"AI {playerNumber}", (Token) playerNumber);
            }
            else
            {
                return new HumanPlayer(name, (Token)playerNumber);
            }
        }

        public static string ReadPlayerName(int playerNumber)
        {
            string name;

            do
            {
                Console.WriteLine($"Enter player #{playerNumber} name: ");
                name = Console.ReadLine();
            } 
            while (string.IsNullOrWhiteSpace(name));

            return name; 
        }
    }
}
