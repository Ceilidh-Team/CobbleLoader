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
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    if (!IsSuccessCode(response.StatusCode)) return false;

                    stream = response.GetResponseStream();
                    return true;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }

        private static bool IsSuccessCode(HttpStatusCode code) => (int) code >= 200 && (int) code < 300;
    }
}
