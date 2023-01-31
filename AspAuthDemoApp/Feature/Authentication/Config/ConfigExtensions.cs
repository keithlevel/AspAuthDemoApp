namespace AspAuthDemoApp.Feature.Authentication.Config
{
    public static class ConfigExtensions
    {
        public static JwtConfig GetJwtConfig(this IConfiguration config)
        {
            return config.GetSection(JwtConfig.SectionName).Get<JwtConfig>();
        }
    }
}
