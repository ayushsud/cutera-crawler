using System;
using System.Collections.Generic;
using System.Text;

namespace Cutera
{
    public class GeocodeException : Exception
    {
        public GeocodeException(string message) : base(message) { }
    }
}
