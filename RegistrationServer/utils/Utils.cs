using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationServer.utils
{
    public static class Utils
    { 
        public static string decodeToString(this byte[] a)
            => Encoding.ASCII.GetString(a);
    }
}
