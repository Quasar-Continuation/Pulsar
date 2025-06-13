using Pulsar.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulsar.Common.IO
{
    public class PluginReconstructor : IDisposable
    {
        private readonly Dictionary<long, byte[]> _chunks;
        private readonly string _pluginName;
        private readonly long _totalSize;
        private readonly int _totalChunks;
        private long _receivedSize;
        private bool _disposed = false;

        /// <summary>
        /// Gets the plugin name being reconstructed.
        /// </summary>
        public string PluginName => _pluginName;

        /// <summary>
        /// Gets the total expected size of the plugin.
        /// </summary>
        public long TotalSize => _totalSize;

        /// <summary>
        /// Gets the number of bytes received so far.
        /// </summary>
        public long ReceivedSize => _receivedSize;

        /// <summary>
        /// Gets the number of chunks received so far.
        /// </summary>
        public int ReceivedChunks => _chunks.Count;

        /// <summary>
        /// Gets the total number of expected chunks.
        /// </summary>
        public int TotalChunks => _totalChunks;

        /// <summary>
        /// Gets whether all chunks have been received.
        /// </summary>
        public bool IsComplete => _chunks.Count == _totalChunks && _receivedSize == _totalSize;

        /// <summary>
        /// Gets the completion percentage (0-100).
        /// </summary>
        public double ProgressPercentage => _totalSize == 0 ? 100 : Math.Round((double)_receivedSize / _totalSize * 100, 2);

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginReconstructor"/> class.
        /// </summary>
        /// <param name="pluginName">The name of the plugin being reconstructed.</param>
        /// <param name="totalSize">The total expected size of the plugin.</param>
        /// <param name="totalChunks">The total number of expected chunks.</param>
        public PluginReconstructor(string pluginName, long totalSize, int totalChunks)
        {
            _pluginName = pluginName ?? throw new ArgumentNullException(nameof(pluginName));
            _totalSize = totalSize;
            _totalChunks = totalChunks;
            _chunks = new Dictionary<long, byte[]>();
            _receivedSize = 0;
        }

        /// <summary>
        /// Adds a chunk to the reconstructor.
        /// </summary>
        /// <param name="chunk">The chunk to add.</param>
        /// <returns>True if the chunk was added successfully, false if it was a duplicate.</returns>
        public bool AddChunk(FileChunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            if (_chunks.ContainsKey(chunk.Offset))
            {
                //ignore duplicate chunk
                return false;
            }

            _chunks[chunk.Offset] = chunk.Data;
            _receivedSize += chunk.Data.Length;
            return true;
        }

        /// <summary>
        /// Reconstructs the complete plugin data from all received chunks.
        /// </summary>
        /// <returns>The complete plugin data as a byte array.</returns>
        /// <exception cref="InvalidOperationException">Thrown if not all chunks have been received.</exception>
        public byte[] GetCompleteData()
        {
            if (!IsComplete)
                throw new InvalidOperationException("Cannot reconstruct plugin: not all chunks have been received.");

            var result = new byte[_totalSize];
            var sortedChunks = _chunks.OrderBy(kvp => kvp.Key);

            foreach (var kvp in sortedChunks)
            {
                var offset = kvp.Key;
                var data = kvp.Value;
                Array.Copy(data, 0, result, offset, data.Length);
            }

            return result;
        }

        /// <summary>
        /// Gets missing chunk offsets.
        /// </summary>
        /// <returns>A list of missing chunk offsets.</returns>
        public List<long> GetMissingChunkOffsets()
        {
            var missing = new List<long>();
            const int chunkSize = 2 * 1024 * 1024; // 2MB chunks

            for (long offset = 0; offset < _totalSize; offset += chunkSize)
            {
                if (!_chunks.ContainsKey(offset))
                {
                    missing.Add(offset);
                }
            }

            return missing;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _chunks.Clear();
                }
                _disposed = true;
            }
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
