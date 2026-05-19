using System;
using System.Collections.Generic;
using System.Text;

namespace Flash.SensitiveWords.RestClient.Core
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
