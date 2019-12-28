using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cutera
{
    public interface IGeocodeProvider
    {
        Task<Geocode> GetGeoCode(string zipCode, CancellationTokenSource cancellationTokenSource, string country = null);
        Task<List<Geocode>> GetBatchGeocodes(List<string> zipCodes, CancellationTokenSource cancellationTokenSource, string country = null);
    }
}
