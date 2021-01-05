using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RegistrationServer.Services
{
    public class FtpService
    {
        private const string USERNAME = "alcatraz";
        private const string PASSWORD = "campus09";
        private const string FTP_URL = "ftp://alcatraz.bplaced.net/";
        private const string IP_ADDRESSES_FILENAME = "IpAddresses.txt";
        private const string IP_ADDRESSES_DELIMITER = ",";

        public static void UpdateAddresses(List<string> addresses)
        {
            WriteToFtp(IP_ADDRESSES_FILENAME, string.Join(IP_ADDRESSES_DELIMITER, addresses));
        }

        private static void WriteToFtp(string filename, string message)
        {
            var request = (FtpWebRequest) WebRequest.Create(FTP_URL + filename);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(USERNAME, PASSWORD);
            using (Stream requestStream = request.GetRequestStream())
            {
                var content = Encoding.Default.GetBytes(message);
                requestStream.Write(content, 0, content.Length);
            }

            request.Abort();
        }

        public static List<string> ReadAddresses()
        {
            List<string> input = ReadFromFtp(IP_ADDRESSES_FILENAME);
            return input.First().Split(IP_ADDRESSES_DELIMITER).ToList();
        }

        private static List<string> ReadFromFtp(string filename)
        {
            var input = new List<string>();
            var client = new WebClient();
            client.Credentials = new NetworkCredential(USERNAME, PASSWORD);

            Stream stream = client.OpenRead(FTP_URL + filename);
            StreamReader sr = new StreamReader(stream);
            while (sr.Peek() >= 0)
            {
                input.Add(sr.ReadLine());
            }
            stream.Close();

            return input;
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
