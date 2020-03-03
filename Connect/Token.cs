using System;
namespace Connect
{
    public enum Token
    {
        BLANK = 0,
        RED = 1,
        YELLOW = 2,
    }

    public static class TokenExtensions
    {
        public static string GetString(this Token token)
        {
            switch (token)
            {
                case Token.BLANK:
                    return " ";
                case Token.RED:
                    return "R";
                case Token.YELLOW:
                    return "Y";
                default:
                    throw new ArgumentOutOfRangeException($"Token value {token} is unknown");
            }
        }
    }
}
