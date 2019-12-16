using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cutera
{
    public interface IGeocodeProvider
    {
        Task<Geocode> GetGeoCode(string zipCode, string country = null);
        Task<List<Geocode>> GetBatchGeocodes(List<string> zipCodes, string country = null);
    }
}
