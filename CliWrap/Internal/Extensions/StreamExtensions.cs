﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CliWrap.Internal.Extensions
{
    internal static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, bool autoFlush,
            CancellationToken cancellationToken = default)
        {
            var buffer = new byte[BufferSizes.Stream];
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                if (autoFlush)
                    await destination.FlushAsync(cancellationToken);
            }
        }

        public static async IAsyncEnumerable<string> ReadAllLinesAsync(this StreamReader reader,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var stringBuilder = new StringBuilder();

            var buffer = new char[BufferSizes.StreamReader];
            int charsRead;

            while ((charsRead = await reader.ReadAsync(buffer, cancellationToken)) > 0)
            {
                for (var i = 0; i < charsRead; i++)
                {
                    if (buffer[i] == '\n')
                    {
                        // Trigger on buffered input (even if it's empty)
                        yield return stringBuilder.ToString();
                        stringBuilder.Clear();
                    }
                    else if (buffer[i] != '\r')
                    {
                        stringBuilder.Append(buffer[i]);
                    }
                }
            }

            // Yield what's remaining
            if (stringBuilder.Length > 0)
                yield return stringBuilder.ToString();
        }
    }
}