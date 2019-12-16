using System;
using System.Collections.Generic;
using System.Text;

namespace Cutera
{
    public class GeocoderResponse
    {
        public string Longt { get; set; }
        public string Latt { get; set; }
        public int MyProperty { get; set; }
        public Error Error { get; set; }
    }

    public class Error
    {
        public string Message { get; set; }
    }
}
