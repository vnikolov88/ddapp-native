using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;

namespace DDAppNative.Common
{
    public class ApplicationCache
    {
        private const int _textStoreFieldMaxLength = 127;
        private const int _entityTagMaxLength = _textStoreFieldMaxLength;
        private const int _mimeTypeMaxLenght = _textStoreFieldMaxLength;

        private readonly string baseDir;
        private readonly HttpClient _httpClient;

        public ApplicationCache(string appBaseUrl, string localBaseDir)
        {
            baseDir = localBaseDir ?? throw new ArgumentNullException(nameof(localBaseDir));

            _httpClient = new HttpClient {
                BaseAddress = new Uri(appBaseUrl)
            };
        }

        public async Task SendAndUpdateAsync(string url, IHttpResponse response, CancellationToken cancellationToken)
        {
            var fileName = $"Caches{url.GetGUID()}";
            var filePath = $"{baseDir}/{fileName}";

            using (var cache = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                #region Loading Cache
                var cacheState = await LoadCachedResponseAsync(cache);
                if(cacheState != null)
                {
                    var outputStreams = new List<Stream> { response.OutputStream };

                    response.StatusCode = (int)cacheState.StatusCode;
                    response.ContentType = cacheState.ContentType;
                    response.ContentLength64 = cacheState.ContentLength64;

                    // Copy the cache to the response
                    await WriteToOutputStreamAsync(outputStreams, cache, cache.Position, cancellationToken).ConfigureAwait(false);
                }
                #endregion Loading Cache

                #region Updating Cache
                try
                {
                    var remoteCache = await CheckRemoteCacheAsync(url, cacheState?.AppPageVersion ?? string.Empty);

                    if (remoteCache != null)
                    {
                        var outputStreams = new List<Stream> { cache };
                        if (cacheState == null)
                        {
                            response.StatusCode = (int)remoteCache.StatusCode;
                            // Set the result mime type
                            response.ContentType = remoteCache.ContentType;
                            response.ContentLength64 = remoteCache.ContentLength64;
                            // Copy the cache to the response
                            outputStreams.Add(response.OutputStream);
                        }

                        await WriteRemoteCacheAsync(remoteCache, cache, outputStreams);
                    }
                }
                catch (WebException ex)
                {
                    var errMsg = ex.ToString();
                    Debug.WriteLine(errMsg);
                }
                catch (Exception ex)
                {
                    var errMsg = ex.ToString();
                    Debug.WriteLine(errMsg);
                }
                #endregion Updating Cache
            }
        }

        public async Task<CacheCandidateState> CheckRemoteCacheAsync(string url, string appPageVersionCurrent)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // Check for remote content based on the app page version
            if (EntityTagHeaderValue.TryParse(appPageVersionCurrent, out var eTagValue))
            {
                request.Headers.IfNoneMatch.Add(eTagValue);
            }

            var serverResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);

            var contentType = serverResponse?.Content?.Headers?.ContentType?.MediaType;
            // Check if request returned some content
            if (serverResponse.StatusCode == HttpStatusCode.NotModified || contentType == null)
            {
                return null;
            }

            // Check if we have a new entity tag
            var appPageVersion = serverResponse.Headers.ETag?.ToString() ?? string.Empty;

            var responseStream = await serverResponse.Content.ReadAsStreamAsync();

            return new CacheCandidateState
            {
                ResponseStream = responseStream,
                StatusCode = serverResponse.StatusCode,
                AppPageVersion = appPageVersion,
                ContentType = contentType,
                ContentLength64 = responseStream.Length
            };
        }

        public async Task<CacheState> LoadCachedResponseAsync(Stream cache)
        {
            try
            {
                if (cache.Length >= _mimeTypeMaxLenght + _entityTagMaxLength)
                {
                    // Read the existing cache
                    using (var reader = new BinaryReader(cache, Encoding.UTF8, true))
                    {
                        // TODO: Refactor
                        // Read App Page Version (ETag)
                        var entityTag = new byte[_entityTagMaxLength];
                        await cache.ReadAsync(entityTag, 0, entityTag.Length).ConfigureAwait(false);
                        var appPageVersion = Encoding.UTF8.GetString(entityTag).TrimEnd('\0');
                        // Read Mime Type
                        var mimeType = new byte[_mimeTypeMaxLenght];
                        await cache.ReadAsync(mimeType, 0, mimeType.Length).ConfigureAwait(false);
                        var contentType = Encoding.UTF8.GetString(mimeType).TrimEnd('\0');

                        var response = new CacheState
                        {
                            AppPageVersion = appPageVersion,
                            // Return cache
                            StatusCode = HttpStatusCode.OK,
                            // Set the result mime type
                            ContentType = contentType,
                            ContentLength64 = cache.Length - cache.Position,
                        };
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                var errMsg = ex.ToString();
                Debug.WriteLine(errMsg);
            }
            return null;
        }

        public async Task WriteRemoteCacheAsync(CacheCandidateState cacheState, Stream cache, IEnumerable<Stream> outputStreams)
        {
            if (cacheState == null)
                throw new ArgumentNullException(nameof(cacheState));
            if (cacheState.AppPageVersion == null)
                throw new ArgumentNullException(nameof(cacheState.AppPageVersion));
            if (cacheState.ContentType == null)
                throw new ArgumentNullException(nameof(cacheState.ContentType));

            // Clear the old cache
            cache.SetLength(0);
            cache.Flush();

            // Update the cache file
            using (var writer = new BinaryWriter(cache, Encoding.UTF8))
            {
                // Rewind the stream
                writer.Seek(0, SeekOrigin.Begin);

                using (var serverStream = cacheState.ResponseStream)
                {
                    // TODO: Refactor
                    // Write App Page Version
                    var entityTag = new byte[_entityTagMaxLength];
                    Encoding.UTF8.GetBytes(cacheState.AppPageVersion, 0, cacheState.AppPageVersion.Length, entityTag, 0);
                    writer.Write(entityTag);
                    // Write Mime Type
                    var mimeType = new byte[_mimeTypeMaxLenght];
                    Encoding.UTF8.GetBytes(cacheState.ContentType, 0, cacheState.ContentType.Length, mimeType, 0);
                    writer.Write(mimeType);
                    writer.Flush();
                    // Write the content
                    await WriteToOutputStreamAsync(outputStreams, serverStream, 0, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        #region Auxiliary methods
        private const int _chunkSize = 24 * 1024;
        public static async Task WriteToOutputStreamAsync(
                IEnumerable<Stream> outputs,
                Stream buffer,
                long lowerByteIndex,
                CancellationToken ct)
        {
            var streamBuffer = new byte[_chunkSize];
            long sendData = 0;
            var readBufferSize = _chunkSize;

            while (true)
            {
                if (sendData + _chunkSize > buffer.Length) readBufferSize = (int)(buffer.Length - sendData);

                buffer.Seek(lowerByteIndex + sendData, SeekOrigin.Begin);
                var read = await buffer.ReadAsync(streamBuffer, 0, readBufferSize, ct).ConfigureAwait(false);

                if (read == 0) break;

                sendData += read;
                foreach (var output in outputs)
                {
                    await output.WriteAsync(streamBuffer, 0, readBufferSize, ct).ConfigureAwait(false);
                }
            }
        }
        #endregion Auxiliary methods
    }
}
