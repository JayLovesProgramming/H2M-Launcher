namespace H2MLauncher.Core.Interfaces
{
    /// <summary>
    /// Interface for a service that handles errors and exceptions within the application.
    /// </summary>
    public interface IErrorHandlingService
    {
        /// <summary>
        /// Handles the provided exception with additional information.
        /// </summary>
        /// <param name="ex">The exception to be handled.</param>
        /// <param name="info">Optional additional information about the context of the exception.</param>
        void HandleException(Exception ex, string info = "");

        /// <summary>
        /// Handles a general error by providing context or description.
        /// </summary>
        /// <param name="info">The information or description of the error to be handled.</param>
        void HandleError(string info);
    }
}
