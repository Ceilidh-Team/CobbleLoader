using System;
using System.IO;
using System.Net;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public class HttpResourceLoader : IResourceLoader
    {
        public bool CanAccept(Uri uri) => uri != null && uri.IsAbsoluteUri &&
                                          (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        public bool TryOpenStream(Uri uri, out Stream stream)
        {
            stream = default;

            if (!CanAccept(uri)) return false;

            var request = WebRequest.CreateHttp(uri);
            request.Method = WebRequestMethods.Http.Get;
            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                if (!IsSuccessCode(response.StatusCode))
                {
                    response.Dispose();
                    return false;
                }

                stream = new WebRequestStream(response);
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        private static bool IsSuccessCode(HttpStatusCode code) => (int) code >= 200 && (int) code < 300;
        
        private sealed class WebRequestStream : Stream
        {
            public override bool CanRead => _requestStream.CanRead;

            public override bool CanSeek => _requestStream.CanSeek;

            public override bool CanWrite => _requestStream.CanWrite;

            public override long Length => _requestStream.Length;

            public override long Position
            {
                get => _requestStream.Position;
                set => _requestStream.Position = value;
            }
            
            private readonly WebResponse _response;
            private readonly Stream _requestStream;

            public WebRequestStream(WebResponse response)
            {
                _response = response;
                _requestStream = response.GetResponseStream();
            }

            public override void Flush()
            {
                _requestStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _requestStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _requestStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _requestStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _requestStream.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                if (!disposing) return;

                _requestStream.Dispose();
                _response.Dispose();
            }
        }
    }
}
