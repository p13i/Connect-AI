using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Connect
{
    public class ConnectFour
    {
        public static readonly int FOUR = 4;

        public int Width { get; }
        public int Height { get; }
        public Player[] Players { get; }

        public Player CurrentPlayer
        {
            get
            {
                return Players[CurrentPlayerIndex];
            }
        }

        public Player NextPlayer
        {
            get
            {
                return Players[(CurrentPlayerIndex + 1) % Players.Length];
            }
        }

        public Grid<Token> Grid { get; private set; }
        private int CurrentPlayerIndex;

        public ConnectFour(int width, int height, params Player[] players)
        {
            if (width < FOUR || height < FOUR)
            {
                throw new ArgumentOutOfRangeException($"{nameof(width)} and {nameof(height)} must be greater than {FOUR}");
            }

            if (players == null || players.Length < 2)
            {
                throw new ArgumentOutOfRangeException($"{nameof(players)} must be at least {2}");
            }

            if (players.Select(player => player.Token).Distinct().Count() != players.Length)
            {
                throw new ArgumentException($"{nameof(players)} must all have unique tokens");
            }

            Width = width;
            Height = height;
            Players = players;
            Grid = new Grid<Token>(Width, Height);
            CurrentPlayerIndex = 0;
        }

        private void CheckColumnNumber(int columnNumber)
        {
            if (columnNumber < 1 || columnNumber > Width)
            {
                throw new ArgumentOutOfRangeException($"{nameof(columnNumber)}={columnNumber} must be between {1} and {Width}");
            }
        }

        private void CheckPlayerActionAllowed(Player player)
        {
            if (!object.Equals(CurrentPlayer, player))
            {
                throw new InvalidOperationException($"{player} may not act now");
            }
        }

        public bool CanPlaceToken(int col) => !IsColumnFull(col);

        private bool IsColumnFull(int col)
        {
            return !IsBlank(col, Height);
        }

        private bool IsBlank(int col, int row)
        {
            return Grid[col, row] == Token.BLANK;
        }

        public void Place(Move move)
        {
            Player player = move.Player;
            int col = move.ColumnToPlaceToken;

            CheckColumnNumber(col);
            CheckPlayerActionAllowed(player);

            if (IsColumnFull(col))
            {
                throw new InvalidOperationException($"Column number {col} is full");
            }

            int lowestBlankRow = 1;

            while (!IsBlank(col, lowestBlankRow))
            {
                lowestBlankRow++;
            }

            if (lowestBlankRow > Height)
            {
                throw new InvalidOperationException("invalid and unexpected state");
            }

            Grid[col, lowestBlankRow] = player.Token;

            // Advance to the next player
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Length;
        }

        public bool TryGetWinningPlayer(out Player winningPlayer)
        {
            // 1. Check all the rows
            for (int row = 1; row <= Height; row++)
            {
                // Count how many of the same token exist in a sequence
                int consequtiveCount = 1;
                Token previousToken = Token.BLANK;

                for (int col = 1; col <= Width; col++)
                {
                    Token currentToken = Grid[col, row];

                    if (currentToken.Equals(previousToken))
                    {
                        consequtiveCount++;
                    }
                    else
                    {
                        consequtiveCount = 1;
                    }

                    if (consequtiveCount == FOUR && !currentToken.Equals(Token.BLANK)) {
                        winningPlayer = GetPlayerWithToken(currentToken);
                        return true;
                    }

                    previousToken = currentToken;
                }
            }

            // 2. Check all the columns

            // 1. Check all the rows
            for (int col = 1; col <= Width; col++)
            {
                // Count how many of the same token exist in a sequence
                int consequtiveCount = 1;
                Token previousToken = Token.BLANK;

                for (int row = 1; row <= Height; row++)
                {
                    Token currentToken = Grid[col, row];

                    if (currentToken.Equals(previousToken))
                    {
                        consequtiveCount++;
                    }
                    else
                    {
                        consequtiveCount = 1;
                    }

                    if (consequtiveCount == FOUR && !currentToken.Equals(Token.BLANK))
                    {
                        winningPlayer = GetPlayerWithToken(currentToken);
                        return true;
                    }

                    previousToken = currentToken;
                }
            }

            // 3. Check along the positively-sloped diagonals
            // TODO

            // 4. Check along the negatively-sloped diagonals
            // TODO

            winningPlayer = default;
            return false;
        }

        private Player GetPlayerWithToken(Token token)
        {
            return Players
                .Where(player => player.Token.Equals(token))
                .First();
        }

        public override string ToString()
        {
            StringBuilder rowBuilder = new StringBuilder();

            for (int row = Height; row >= 1; row--)
            {
                StringBuilder colBuilder = new StringBuilder();

                colBuilder.Append('|');
                for (int col = 1; col <= Width; col++)
                {
                    Token token = Grid[col, row];

                    colBuilder.Append(token.GetString());
                    colBuilder.Append('|');
                }

                colBuilder.Append(Environment.NewLine);

                // Add this new row to the rowBuilder
                rowBuilder.Append(colBuilder);
            }

            return rowBuilder.ToString();
        }

        public ConnectFour Clone()
        {
            ConnectFour clone = new ConnectFour(Width, Height, Players);

            clone.Grid = Grid.Clone();
            clone.CurrentPlayerIndex = CurrentPlayerIndex;

            return clone;
        }
    }

    public static class ConnectFourMinimaxExtensions
    {
        public static bool IsGameOver(ConnectFour @this)
        {
            return @this.TryGetWinningPlayer(out _);
        }

        public static IEnumerable<Tuple<Move, ConnectFour>> GetChildGameStates(ConnectFour @this)
        {
            Player currentPlayer = @this.CurrentPlayer;

            for (int i = 1; i <= @this.Width; i++)
            {
                if (@this.CanPlaceToken(i))
                {
                    Move move = new Move(currentPlayer, i);

                    ConnectFour clone = @this.Clone();
                    clone.Place(move);

                    yield return new Tuple<Move, ConnectFour>(move, clone);
                }
            }
        }

        /// <summary>
        /// The evaluation of the game state is measured by the number of instances that pair of the current player's tokens are found together
        /// minus the same for the opposing player
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static int Evaluation(ConnectFour @this)
        {
            return Evaluation(@this, @this.CurrentPlayer) - Evaluation(@this, @this.NextPlayer);
        }

        private static int Evaluation(ConnectFour @this, Player player)
        {
            int countOfAdjacentPairs = 0;

            // Count in the horizontal direcation first
            for (int row = 1; row <= @this.Height; row++) {
                for (int i = 1, j = 2; j <= @this.Width; i++, j++)
                {
                    if (@this.Grid[i, row] == player.Token) {
                        if (@this.Grid[i, row] == @this.Grid[j, row])
                        {
                            countOfAdjacentPairs++;
                        }
                    }
                }
            }

            // Count in the vertical direcation second
            for (int col = 1; col <= @this.Width; col++)
            {
                for (int i = 1, j = 2; j <= @this.Height; i++, j++)
                {
                    if (@this.Grid[col, i] == player.Token)
                    {
                        if (@this.Grid[col, i] == @this.Grid[col, j])
                        {
                            countOfAdjacentPairs++;
                        }
                    }
                }
            }

            return countOfAdjacentPairs;
        }

        public static Move GetNextMove(ConnectFour @this)
        {
            Tuple<Move, int> minimax = Minimax(new Tuple<Move, ConnectFour>(null, @this.Clone()), 5, true);
            return minimax.Item1;
        }

        /*
         *   function minimax(position, depth, maximizingPlayer)
	            if depth == 0 or game over in position
		            return static evaluation of position

	            if maximizingPlayer
		            maxEval = -infinity
		            for each child of position
			            eval = minimax(child, depth - 1, false)
			            maxEval = max(maxEval, eval)
		            return maxEval

	            else
		            minEval = +infinity
		            for each child of position
			            eval = minimax(child, depth - 1, true)
			            minEval = min(minEval, eval)
		            return minEval


            // initial call
            minimax(currentPosition, 3, true)
         */

        private static Tuple<Move, int> Minimax(Tuple<Move, ConnectFour> moveAndPosition, int depth, bool maximizingCurrentPlayer)
        {
            ConnectFour position = moveAndPosition.Item2;
            if (depth == 0 || IsGameOver(position))
            {
                return new Tuple<Move, int>(moveAndPosition.Item1, Evaluation(position));
            }

            if (maximizingCurrentPlayer)
            {
                int maxEval = -1 * int.MaxValue;
                Move maxMove = default;

                foreach (Tuple<Move, ConnectFour> child in GetChildGameStates(position))
                {
                    Tuple<Move, int> evalResult = Minimax(child, depth - 1, !maximizingCurrentPlayer);

                    if (evalResult.Item2 > maxEval)
                    {
                        maxEval = evalResult.Item2;
                        maxMove = evalResult.Item1;
                    }
                }

                return new Tuple<Move, int>(maxMove, maxEval);
            }
            else
            {
                int minEval = int.MaxValue;
                Move minMove = default;

                foreach (Tuple<Move, ConnectFour> child in GetChildGameStates(position))
                {
                    Tuple<Move, int> evalResult = Minimax(child, depth - 1, !maximizingCurrentPlayer);

                    if (evalResult.Item2 < minEval)
                    {
                        minEval = evalResult.Item2;
                        minMove = evalResult.Item1;
                    }
                }

                return new Tuple<Move, int>(minMove, minEval);
            }
        }
    }
}
