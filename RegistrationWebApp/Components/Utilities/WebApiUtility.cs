namespace RegistrationWebApp.Components.Utilities
{
    public static class WebApiUtility
    {
        private const string WebApiUrlEnvironmentVariable = "WEB_API_URL";
        private static string? configuredBaseUrl;

        /// <summary> Configures the web API utility with a base URL from configuration. 
        /// This can be overridden by setting the WEB_API_URL environment variable.
        /// </summary>
        /// <param name="baseUrl">The base URL from configuration.</param>
        public static void Configure(string? baseUrl)
        {
            configuredBaseUrl = baseUrl;
        }

        /// <summary>
        /// Gets the web API base URL from the environment variable.
        /// </summary>
        /// <returns>Environment variable string value or null if not set.</returns>
        public static string? GetBaseUrlFromEnvironment()
        {
            return Environment.GetEnvironmentVariable(WebApiUrlEnvironmentVariable);
        }

        /// <summary> Gets the web API base URL from configuration (appsettings.json). 
        /// This is used if the environment variable is not set.
        /// </summary>
        /// <returns>Configuration string value or null if not set.</returns>

        public static string? GetBaseUrlFromConfiguration()
        {
            return configuredBaseUrl;
        }

        /// <summary> Gets the web API base URL, preferring the environment variable over configuration.
        /// Throws an exception if neither is set.
        /// </summary>
        /// <returns>The web API base URL.</returns>
        public static string GetBaseUrl()
        {
            return GetBaseUrlFromEnvironment()
                ?? GetBaseUrlFromConfiguration()
                ?? throw new InvalidOperationException("A web API base URL must be configured via the WEB_API_URL environment variable or the WebApi:BaseUrl configuration setting.");
        }
    }
}