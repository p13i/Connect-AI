using System;
using System.Text;
using System.Linq;

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

        private Grid<Token> Grid;
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
        }

        public void AdvanceToNextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Length;
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

        private bool IsColumnFull(int col)
        {
            return !IsBlank(col, Height);
        }

        private bool IsBlank(int col, int row)
        {
            return Grid[col, row] == Token.BLANK;
        }

        public void Place(Player player, int col)
        {
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
    }
}
