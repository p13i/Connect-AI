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

            foreach (Player player in Players)
            {
                player.ConnectFour = this;
            }
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

        public bool IsGameOver(out Player winningPlayer)
        {
            // 1. Check all the rows
            foreach (Token[] tokenArray in GetAllTokenArrays())
            {
                if (IsWinningTokenArray(tokenArray))
                {
                    winningPlayer = GetPlayerWithToken(tokenArray[0]);
                    return true;
                }
            }

            if (Enumerable.Range(1, Width).All(IsColumnFull))
            {
                winningPlayer = null;
                return false;
            }

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
            ConnectFour clone = new ConnectFour(Width, Height, Players.Select(p => p.Clone()).ToArray());

            clone.Grid = Grid.Clone();
            clone.CurrentPlayerIndex = CurrentPlayerIndex;

            foreach (Player player in clone.Players)
            {
                player.ConnectFour = clone;
            }

            return clone;
        }

        public IEnumerable<Token[]> GetAllTokenArrays()
        {
            // Count in the horizontal direcation first
            for (int row = 1; row <= this.Height; row++)
            {
                for (int i = 1, j = 2, k = 3, l = 4; l <= this.Width; i++, j++, k++, l++)
                {
                    Token[] tokens = { this.Grid[i, row], this.Grid[j, row], this.Grid[k, row], this.Grid[l, row] };

                    yield return tokens;
                }
            }

            // Count in the vertical direcation second
            for (int col = 1; col <= this.Width; col++)
            {
                for (int i = 1, j = 2, k = 3, l = 4; l <= this.Height; i++, j++, k++, l++)
                {
                    Token[] tokens = { this.Grid[col, i], this.Grid[col, j], this.Grid[col, k], this.Grid[col, l] };

                    yield return tokens;
                }
            }
        }

        public bool IsWinningTokenArray(Token[] tokens)
        {
            if (tokens == null || tokens.Length != 4)
            {
                throw new ArgumentException($"{nameof(tokens)} must be of length {4}");
            }

            return tokens.Distinct().Count() == 1 && tokens.All(t => t != Token.BLANK);
        }
    }

    /// <summary>
    /// All the AI functionality
    /// </summary>
    public static class ConnectFourMinimaxExtensions
    {

        public static IEnumerable<Tuple<Move, ConnectFour>> GetChildGameStates(this ConnectFour @this)
        {
            for (int i = 1; i <= @this.Width; i++)
            {
                if (@this.CanPlaceToken(i))
                {
                    Move move = new Move(@this.CurrentPlayer, i);

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
        public static int Evaluation(this ConnectFour @this)
        {
            return Evaluation(@this, @this.CurrentPlayer) - Evaluation(@this, @this.NextPlayer);
        }

        private static int Evaluation(ConnectFour @this, Player player)
        {
            int score = 0;

            foreach (Token[] tokenArray in @this.GetAllTokenArrays())
            {
                score += ScoreWindow(tokenArray, player.Token);
            }

            return score;
        }

        private static int ScoreWindow(Token[] tokens, Token playerToken)
        {
            if (tokens == null || tokens.Length != 4)
            {
                throw new ArgumentException($"{nameof(tokens)} must be of length {4}");
            }

            int score = 0;

            // This player has four in a row
            if (tokens.All(t => t == playerToken))
            {
                score += 100;
            }
            // This player has three placed with one blank in a window
            else if (tokens.Where(t => t == playerToken).Count() == 3 && tokens.Where(t => t == Token.BLANK).Count() == 1)
            {
                score += 50;
            }
            // Player has two tokens and two blank spaces in a window
            else if (tokens.Where(t => t == playerToken).Count() == 2 && tokens.Where(t => t == Token.BLANK).Count() == 2)
            {
                score += 20;
            }
            // Player has two tokens and two blank spaces in a window
            else if (tokens.Where(t => t == playerToken).Count() == 1 && tokens.Where(t => t == Token.BLANK).Count() == 3)
            {
                score += 10;
            }
            // Other player player has four in a row
            else if (tokens.All(t => t != playerToken && t != Token.BLANK))
            {
                score -= 100;
            }
            // Other player has three tokens and one blank space in a window
            else if (tokens.Where(t => t != playerToken && t != Token.BLANK).Count() == 3 && tokens.Where(t => t == Token.BLANK).Count() == 1)
            {
                score -= 50;
            }
            // Other player has two tokens and two blank spaces in a window
            else if (tokens.Where(t => t != playerToken && t != Token.BLANK).Count() == 2 && tokens.Where(t => t == Token.BLANK).Count() == 2)
            {
                score -= 20;
            }
            // Other player has two tokens and two blank spaces in a window
            else if (tokens.Where(t => t != playerToken && t != Token.BLANK).Count() == 1 && tokens.Where(t => t == Token.BLANK).Count() == 3)
            {
                score -= 10;
            }

            return score;
        }

        public static Move GetNextMove(this ConnectFour @this, int depth = 3)
        {
            Tuple<Move, int> minimax = Minimax(
                moveAndPosition: new Tuple<Move, ConnectFour>(Move.Default, @this.Clone()), 
                depth: depth, 
                alpha: int.MinValue, 
                beta: int.MaxValue, 
                maximizingCurrentPlayer: true);

            Move nextMove = minimax.Item1;

            if (nextMove == Move.Default)
            {
                return null;
                throw new InvalidOperationException();
            }

            return nextMove;
        }

        private static Tuple<Move, int> Minimax(
            Tuple<Move, ConnectFour> moveAndPosition, 
            int depth, 
            int alpha, 
            int beta, 
            bool maximizingCurrentPlayer)
        {
            ConnectFour position = moveAndPosition.Item2;
            if (position.IsGameOver(out Player winningPlayer) || depth == 0)
            {
                Move move = moveAndPosition.Item1;
                int evaluation;
                
                if (winningPlayer != default)
                {
                    evaluation = winningPlayer == move.Player ? int.MaxValue : int.MinValue;
                }
                else
                {
                    evaluation = position.Evaluation();
                }
                
                return new Tuple<Move, int>(moveAndPosition.Item1, evaluation);
            }

            if (maximizingCurrentPlayer)
            {
                int maxEval = int.MinValue;
                Move maxMove = Move.Default;

                foreach (Tuple<Move, ConnectFour> child in GetChildGameStates(position))
                {
                    Tuple<Move, int> evalResult = Minimax(child, depth - 1, alpha, beta, !maximizingCurrentPlayer);

                    Move move = evalResult.Item1;
                    int eval = evalResult.Item2;

                    if (eval > maxEval)
                    {
                        maxMove = move;
                        maxEval = eval;
                    }

                    alpha = Math.Max(alpha, evalResult.Item2);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return new Tuple<Move, int>(maxMove, maxEval);
            }
            else
            {
                int minEval = int.MaxValue;
                Move minMove = Move.Default;

                foreach (Tuple<Move, ConnectFour> child in GetChildGameStates(position))
                {
                    Tuple<Move, int> result = Minimax(child, depth - 1, alpha, beta, !maximizingCurrentPlayer);
                    
                    Move move = result.Item1;
                    int eval = result.Item2;

                    if (eval < minEval)
                    {
                        minMove = move;
                        minEval = eval;
                    }

                    beta = Math.Min(beta, result.Item2);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return new Tuple<Move, int>(minMove, minEval);
            }
        }
    }
}
