using Pulsar.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pulsar.Common.IO
{
    public class PluginSplit : IEnumerable<FileChunk>, IDisposable
    {
        /// <summary>
        /// The maximum size per plugin chunk (2MB).
        /// </summary>
        public readonly int MaxChunkSize = 2 * 1024 * 1024; // 2MB

        /// <summary>
        /// The plugin data bytes.
        /// </summary>
        private readonly byte[] _pluginData;

        /// <summary>
        /// The size of the plugin data.
        /// </summary>
        public long PluginSize => _pluginData.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSplit"/> class using the given plugin data.
        /// </summary>
        /// <param name="pluginData">The plugin data bytes to split into chunks.</param>
        public PluginSplit(byte[] pluginData)
        {
            _pluginData = pluginData ?? throw new ArgumentNullException(nameof(pluginData));
        }

        /// <summary>
        /// Reads a chunk of the plugin data.
        /// </summary>
        /// <param name="offset">Offset of the data, must be a multiple of <see cref="MaxChunkSize"/> for proper reconstruction.</param>
        /// <returns>The read plugin chunk at the given offset.</returns>
        /// <remarks>
        /// The returned chunk can be smaller than <see cref="MaxChunkSize"/> if the
        /// remaining data size from the offset is smaller than <see cref="MaxChunkSize"/>,
        /// then the remaining data size is used.
        /// </remarks>
        public FileChunk ReadChunk(long offset)
        {
            if (offset >= _pluginData.Length)
                return null;

            long chunkSize = _pluginData.Length - offset < MaxChunkSize
                ? _pluginData.Length - offset
                : MaxChunkSize;

            var chunkData = new byte[chunkSize];
            Array.Copy(_pluginData, offset, chunkData, 0, chunkSize);

            return new FileChunk
            {
                Data = chunkData,
                Offset = offset
            };
        }

        /// <summary>
        /// Gets the total number of chunks required for this plugin.
        /// </summary>
        public int GetTotalChunks()
        {
            return (int)Math.Ceiling((double)_pluginData.Length / MaxChunkSize);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the plugin chunks.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the plugin chunks.</returns>
        public IEnumerator<FileChunk> GetEnumerator()
        {
            for (long offset = 0; offset < _pluginData.Length; offset += MaxChunkSize)
            {
                yield return ReadChunk(offset);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        /// <summary>
        /// Disposes all managed and unmanaged resources associated with this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
