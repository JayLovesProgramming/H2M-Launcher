using Microsoft.AspNetCore.Authentication;

namespace MatchmakingServer.Authentication
{
    /// <summary>
    /// Options for API key-based authentication.
    /// </summary>
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Gets or sets the expected API key for authentication.
        /// </summary>
        public string? ApiKey { get; set; }
    }
}
