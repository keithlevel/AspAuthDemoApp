namespace AspAuthDemoApp.Feature.Authentication.ViewModels
{
    public class TokenResponse
    {
        public string Token { get; }

        public DateTime Expiration { get; }

        public DateTime ValidFrom { get; }

        public TokenResponse(string token, DateTime expiration, DateTime validFrom)
        {
            Token = token;
            Expiration = expiration;
            ValidFrom = validFrom;
        }

        public override bool Equals(object? obj)
        {
            return obj is TokenResponse other &&
                   Token == other.Token &&
                   Expiration == other.Expiration &&
                   ValidFrom == other.ValidFrom;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Token, ValidFrom);
        }
    }
}
