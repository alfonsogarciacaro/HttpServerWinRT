using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HttpServerWinRT
{
    public sealed class StreamReaderAsync : IInputStreamAsync, IDisposable
    {
        public const uint DefaultBufferSize = 8192;
        public const InputStreamOptions DefaultStreamOptions = InputStreamOptions.Partial;
        
        uint currentIndex = 0;
        bool headersRead = false;
        readonly IInputStream inputStream;
        readonly StringBuilder stringBuilder;
        readonly Windows.Storage.Streams.Buffer buffer;

        StreamReaderAsync(IInputStream inputStream)
        {
            this.inputStream = inputStream;
            this.stringBuilder = new StringBuilder();
            this.buffer = new Windows.Storage.Streams.Buffer(DefaultBufferSize);
        }

        public static async Task<StreamReaderAsync> Create(IInputStream inputStream)
        {
            var stream = new StreamReaderAsync(inputStream);
            await inputStream.ReadAsync(stream.buffer, stream.buffer.Capacity, DefaultStreamOptions);
            return stream;
        }

        public async Task<int> ReadBytesAsync(byte[] bytes)
        {
            int otherIndex = 0;
            if (currentIndex < buffer.Length)
                for ( ; currentIndex < buffer.Length && otherIndex < bytes.Length; currentIndex++, otherIndex++)
                    bytes[otherIndex] = buffer.GetByte(currentIndex);

            while (!headersRead || (buffer.Length == buffer.Capacity && otherIndex < bytes.Length))
            {
                await inputStream.ReadAsync(buffer, buffer.Capacity, DefaultStreamOptions);
                headersRead = true;
                
                for (currentIndex = 0; currentIndex < buffer.Length && otherIndex < bytes.Length; currentIndex++, otherIndex++)
                    bytes[otherIndex] = buffer.GetByte(currentIndex);
            }

            return otherIndex;
        }

        void appendStringFromBufferBytes(uint start, uint count)
        {
            if (count > 0)
            {
                var bytes = buffer.ToArray(start, (int)count);
                var str = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                stringBuilder.Append(str);
            }
        }

        string scanForNewLine()
        {
            for (uint i = currentIndex; i < buffer.Length - 1; i++)
            {
                if (buffer.GetByte(i) == '\r' && buffer.GetByte(i + 1) == '\n')
                {
                    appendStringFromBufferBytes(currentIndex, i - currentIndex);
                    currentIndex = i + 2;
                    return stringBuilder.ToString();
                }
            }
            appendStringFromBufferBytes(0, buffer.Length);
            currentIndex = buffer.Length;
            return null;
        }

        public async Task<string> ReadLineAsync()
        {
            stringBuilder.Clear();
            if (currentIndex < buffer.Length)
            {
                var str = scanForNewLine();
                if (str != null) { return str; }
            }

            while (!headersRead || buffer.Length == buffer.Capacity)
            {
                await inputStream.ReadAsync(buffer, buffer.Capacity, DefaultStreamOptions);
                headersRead = true;
                currentIndex = 0;
                
                var str = scanForNewLine();
                if (str != null) { return str; }
            }

            return stringBuilder.ToString();
        }

        public async Task<string> ReadToEndAsync()
        {   
            stringBuilder.Clear();
            if (currentIndex < buffer.Length)
                appendStringFromBufferBytes(currentIndex, buffer.Length - currentIndex);

            while (!headersRead || buffer.Length == buffer.Capacity)
            {
                await inputStream.ReadAsync(buffer, buffer.Capacity, DefaultStreamOptions);
                appendStringFromBufferBytes(0, buffer.Length);
                currentIndex = buffer.Length;
                headersRead = true;
            }
            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            this.inputStream.Dispose();
        }
    }

    public sealed class StreamWriterAsync : IOutputStreamAsync, IDisposable
    {
        public readonly IOutputStream Stream;
        public readonly DataWriter DataWriter;

        public StreamWriterAsync(IOutputStream stream)
        {
            this.Stream = stream;
            this.DataWriter = new DataWriter(stream);
        }

        public async Task WriteBytesAsync(byte[] bytes)
        {
            this.DataWriter.WriteBytes(bytes);
            await this.DataWriter.StoreAsync();
        }

        public async Task WriteStringAsync(string str)
        {
            this.DataWriter.WriteString(str);
            await this.DataWriter.StoreAsync();
        }

        public void Dispose()
        {
            this.DataWriter.Dispose();
            this.Stream.Dispose(); // The stream should be disposed with the DataWriter actually
        }
    }
}
