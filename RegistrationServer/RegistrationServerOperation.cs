using System;
using System.Collections.Generic;
using System.Text;

namespace RegistrationServer
{
    public enum RegistrationServerOperation
    {
        Write = 0,
        Read,
        Join,
        Leave,
    }

    public static class RegistratioServerOperationExtension
    {
        public static int Value(this RegistrationServerOperation operation)
            => Convert.ToInt32(operation);

        public static RegistrationServerOperation toRegistrationServerOperation(this int operation)
            => (RegistrationServerOperation)operation;
    }
}
