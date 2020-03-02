using System;
namespace Connect
{
    public class Player
    {
        public Player(string name, Token token)
        {
            Name = name;
            Token = token;
        }

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
    }
}
