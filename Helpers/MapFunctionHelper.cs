using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;
using yucee.Helpers;

namespace Fitness_Diary.Helpers
{
    class MapFunctionHelper
    {
        string mapkey;
        GoogleMap map;

        public double distance;
        public string distanceString;

        public MapFunctionHelper(string mMapkey, GoogleMap mMap)
        {
            mapkey = mMapkey;
            map = mMap;
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

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
            //Origin of route
            string str_origin = "origin=" + location.Latitude + "," + location.Longitude;

            //Destination of route
            string str_destination = "destination=" + destination.Latitude + "," + destination.Longitude;

            //Mode
            string mode = "mode=walking";

            //Building the parameters to the webservice
            string parameters = str_origin + "&" + str_destination + "&" + "&" + mode + "&key=";

            //Output format
            string output = "json";

            string key = mapkey;

            //Building the final url string
            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?"  + parameters + key;

            string json = "";
            json = await GetGeoJsonAsync(url);

            return json;
        }

        public void DrawRouteOnMap(string json)
        {
            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            //Decode Encoded Route
            var points = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(points);

            ArrayList routeList = new ArrayList();
            foreach(LatLng item in line)
            {
                routeList.Add(item);
            }

            //Draw Polylines on Map
            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(10)
                .InvokeColor(Color.Cyan)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            Android.Gms.Maps.Model.Polyline mPolyline = map.AddPolyline(polylineOptions);

            //Getting start and end destination points
            LatLng startPoint = line[0];
            LatLng endPoint = line[line.Count - 1];

            //Start marker options
            MarkerOptions startMarkerOptions = new MarkerOptions();
            startMarkerOptions.SetPosition(startPoint);
            startMarkerOptions.SetTitle("Start Location");
            startMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

            //End marker options
            MarkerOptions endMarkerOptions = new MarkerOptions();
            endMarkerOptions.SetPosition(endPoint);
            endMarkerOptions.SetTitle("End Location");
            endMarkerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));

            Marker startMarker = map.AddMarker(startMarkerOptions);
            Marker endMarker = map.AddMarker(endMarkerOptions);

            //Get route bounds
            double southlng = directionData.routes[0].bounds.southwest.lng;
            double southlat = directionData.routes[0].bounds.southwest.lat;
            double northlng = directionData.routes[0].bounds.northeast.lng;
            double northlat = directionData.routes[0].bounds.northeast.lat;

            LatLng southwest = new LatLng(southlat, southlng);
            LatLng northeast = new LatLng(northlat, northlng);
            LatLngBounds routeBound = new LatLngBounds(southwest, northeast);

            map.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(routeBound, 470));
            map.SetPadding(40, 70, 40, 70);
            startMarker.ShowInfoWindow();

            distance = directionData.routes[0].legs[0].distance.value;
            distanceString = directionData.routes[0].legs[0].distance.text;

        }

        public void ResetRoute()
        {
            map.Clear();
        }

        public double CalculateDistance()
        {
            double km = (distance / 1000); //converting from metres to km

            return km;

        }
    }
}
