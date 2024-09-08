using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace MatchmakingServer.Authentication
{
    /// <summary>
    /// Custom authentication handler for API key-based authentication.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        /// <summary>
        /// Constructor for the API key authentication handler.
        /// </summary>
        /// <param name="options">Options monitor for the API key authentication options.</param>
        /// <param name="logger">Logger factory for logging purposes.</param>
        /// <param name="encoder">URL encoder for encoding operations.</param>
        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        /// <summary>
        /// Handles the API key authentication by validating the provided API key against the expected API key.
        /// </summary>
        /// <returns>An <see cref="AuthenticateResult"/> representing the result of the authentication attempt.</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check if the API key is present in the request headers.
            if (!Request.Headers.TryGetValue(ApiKeyDefaults.RequestHeaderKey, out var apiKeyValues))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
            }

            // Retrieve the provided and expected API keys.
            string? providedApiKey = apiKeyValues.FirstOrDefault();
            string? expectedApiKey = Options.ApiKey;

            // Validate the provided API key against the expected API key.
            if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != expectedApiKey)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            // Create claims for the authenticated user.
            Claim[] claims = [new Claim(ClaimTypes.Name, "APIKeyUser")];

            // Create an authentication ticket for the successfully authenticated user.
            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
