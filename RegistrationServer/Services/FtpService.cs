using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationServer.Services
{
    public class FtpService
    {
        public static void updateAddresses(List<string> addresses)
        {
            WriteToFtp("IpAddresses.txt","testip:1212, 192.168.0.12");
        }

        private static void WriteToFtp(string filename, string message)
        {
            var request = (FtpWebRequest) WebRequest.Create("ftp://alcatraz.bplaced.net/"+filename);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("alcatraz", "campus09");
            using (Stream requestStream = request.GetRequestStream())
            {
                var content = Encoding.Default.GetBytes(message);
                requestStream.Write(content, 0, content.Length);
            }

            request.Abort();
        }

        public static bool DeleteFileOnServer(Uri serverUri)
        {
            // The serverUri parameter should use the ftp:// scheme.
            // It contains the name of the server file that is to be deleted.
            // Example: ftp://contoso.com/someFile.txt.
            //

            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
