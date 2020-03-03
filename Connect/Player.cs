using System;
namespace Connect
{
    public abstract class Player
    {
        public Player(string name, Token token)
        {
            Name = name;
            Token = token;
        }

        public ConnectFour ConnectFour { get; set; }
        public string Name { get; }
        public Token Token { get; }

        public override int GetHashCode()
        {
            return Name.GetHashCode() * Token.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Player p = (Player) obj;
                return Name.Equals(p.Name) && Token.Equals(p.Token);
            }
        }

        public override string ToString()
        {
            return $"{Name} ({Token.GetString()})";
        }

        public abstract Move GetNextMove();

        public abstract Player Clone();
    }

    public sealed class HumanPlayer : Player
    {
        public HumanPlayer(string name, Token token) : base(name, token) { }

        public override Player Clone()
        {
            return new HumanPlayer(Name, Token);
        }

        public override Move GetNextMove()
        {
            string input;
            int result;

            do
            {
                Console.Write($"Player {this}, enter a column number [{1}, {ConnectFour.Width}]: ");
                input = Console.ReadLine();
            }
            while (!int.TryParse(input, out result));

            return new Move(this, result);
        }
    }

    public sealed class MinimaxPlayer : Player
    {
        public MinimaxPlayer(string name, Token token) : base(name, token) { }

        public override Player Clone()
        {
            return new MinimaxPlayer(Name, Token);
        }

        public override Move GetNextMove()
        {
            return ConnectFourMinimaxExtensions.GetNextMove(ConnectFour);
        }
    }
}
