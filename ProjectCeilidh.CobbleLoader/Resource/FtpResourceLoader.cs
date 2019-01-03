using System;
using System.IO;
using System.Net;

namespace ProjectCeilidh.CobbleLoader.Resource
{
    public class FtpResourceLoader : IResourceLoader
    {
        public bool CanAccept(Uri uri) => uri != null && uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeFtp;

        public bool TryOpenStream(Uri uri, out Stream stream)
        {
            stream = default;

            if (!CanAccept(uri)) return false;

            var request = WebRequest.Create(uri);
            request.Method = WebRequestMethods.File.DownloadFile;
            try
            {
                using (var response = (FtpWebResponse)request.GetResponse())
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

        private static bool IsSuccessCode(FtpStatusCode code) =>
            code == FtpStatusCode.CommandOK || code == FtpStatusCode.FileActionOK;
    }
}
