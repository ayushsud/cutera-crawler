using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cutera
{
    public class GeocoderGeocodeProvider : IGeocodeProvider
    {
        private readonly string baseURL = "https://geocoder.ca/?json=1&showcountry=1&locate=";
        public Task<List<Geocode>> GetBatchGeocodes(List<string> zipCodes, CancellationTokenSource cancellationTokenSource, string country)
        {
            throw new NotImplementedException();
        }

        public async Task<Geocode> GetGeoCode(string zipCode, CancellationTokenSource cancellationTokenSource, string country = null)
        {
            try
            {
                var response = await HttpHelper.GETHttpResponse<GeocoderResponse>(baseURL + zipCode, false);
                if (response == null ||
                    string.IsNullOrWhiteSpace(response.Latt) ||
                    string.IsNullOrWhiteSpace(response.Longt))
                {
                    throw new Exception();
                }
                return new Geocode()
                {
                    Latitude = response.Latt,
                    Longitude = response.Longt
                };
            }
            catch
            {
                cancellationTokenSource.Cancel();
                throw new GeocodeException(zipCode);
            }

        }
    }
}
