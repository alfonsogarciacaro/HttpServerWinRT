using System.Threading.Tasks;

namespace HttpServerWinRT
{
    /// <summary>
    /// Read forward-only binary asynchronous stream
    /// </summary>
    public interface IInputStreamAsync : HttpMultipartParser.IInputStreamAsync
    {
        /// <summary>
        /// Reads bytes and decodes them as UTF-8 characters until finding a newline char.
        /// </summary>
        /// <returns>The character string from the current position to the next newline char.</returns>
        Task<string> ReadLineAsync();

        /// <summary>
        /// Reads bytes and decodes them as UTF-8 characters until hitting the end of the stream.
        /// </summary>
        /// <returns>The character string from the current position to the end of the stream.</returns>
        Task<string> ReadToEndAsync();
    }

    /// <summary>
    /// Write forward-only binary asynchronous stream
    /// </summary>
    public interface IOutputStreamAsync
    {
        /// <summary>
        /// Asynchronously writes the bytes contained in the buffer, advancing the writing cursor.
        /// </summary>
        /// <param name="bytes">The buffer where to write the bytes to.</param>
        Task WriteBytesAsync(byte[] bytes);

        /// <summary>
        /// Encodes a character string as UTF-8 and writes the resulting bytes into the stream.
        /// </summary>
        /// <param name="str">The string to be encoded and written into the buffer.</param>
        Task WriteStringAsync(string str);
    }
}
