using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using yucee.Helpers;

namespace Fitness_Diary.Helpers
{
    class MapFunctionHelper
    {
        string mapkey;

        public MapFunctionHelper(string mMapkey)
        {
            mapkey = mMapkey;
        }

        public string GetGeoCodeUrl(double lat, double lng)
        {
            string url = "https://maps.googleapis.com/maps/api/geocode/json?latlng=" + lat + "," + lng + "&key=" + mapkey;
            return url;
        }

        //making web request through http client to the googleapi url
        public async Task<string> GetGeoJsonAsync(string url)
        {
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);
            return result;
        }

        public async Task<string> FindCoordinateAddress(LatLng position)
        {
            string url = GetGeoCodeUrl(position.Latitude, position.Longitude);
            string json = "";
            string placeAddress = "";

            json = await GetGeoJsonAsync(url);

            //deserializing json
            if(!string.IsNullOrEmpty(json))
            {
                var geoCodeData = JsonConvert.DeserializeObject<GeocodingParser>(json);

                //grabs first formatted address from json after deserialized
                if (!geoCodeData.status.Contains("ZERO"))
                {
                    if(geoCodeData.results[0] != null)
                    {
                        placeAddress = geoCodeData.results[0].formatted_address;
                    }
                }
            }
            return placeAddress;
        }
    }
}
