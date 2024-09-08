namespace H2MLauncher.Core.Interfaces
{
    /// <summary>
    /// Interface for a clipboard service that provides functionality 
    /// to save text data to the system clipboard.
    /// </summary>
    public interface IClipBoardService
    {
        /// <summary>
        /// Saves the specified text to the system clipboard.
        /// </summary>
        /// <param name="text">The text to be copied to the clipboard.</param>
        void SaveToClipBoard(string text);
    }
}
