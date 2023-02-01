namespace AspAuthDemoApp.Feature.Authentication.Config
{
    public class JwtConfig
    {
        public const string SectionName = "Jwt";

        public string ValidAudience { get; set; } = default!;

        public string ValidIssuer { get; set; } = default!;

        public string Secret { get; set; } = default!;
    }
}
