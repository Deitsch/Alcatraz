using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RegistrationServer.Services
{
    public class FtpService
    {
        public static void updateAddresses(List<string> addresses)
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential("alcatraz", "campus09");

            using (var myStream = client.OpenWrite("ftp://alcatraz.bplaced.net/dsi.txt"))
            {
                using (var stream = GenerateStreamFromString("Frolo69"))
                {
                    stream.CopyTo(myStream);
                }
            }
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
