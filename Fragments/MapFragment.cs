using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V4.App;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps.Model;
using Android;
using Android.Gms.Location;
using Android.Content.PM;
using Fitness_Diary.Helpers;
using Google.Places;
using Android.Graphics;

namespace Fitness_Diary.Fragments
{
    public class MapFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
    {
        //Layouts
        RelativeLayout layoutStartDestination;
        RelativeLayout layoutEndDestination;

        //TextViews
        TextView startDestinationText;
        TextView endDestinationText;

        //ImageView
        ImageView centreMarker;

        //Buttons
        RadioButton startRadio;
        RadioButton endRadio;

        //Helpers
        MapFunctionHelper mapHelper;

        //TripDetails
        LatLng startLocationLatlng;
        LatLng endLocationLatlng;

        //Flags
        int addressRequest = 1;
        bool takeAddressFromSearch;

        GoogleMap mainMap;

        readonly string[] permissionGroupLocation = { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation };
        const int requestLocationId = 0;
        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationClient;
        Android.Locations.Location mLastLocation;
        LocationCallbackHelper mLocationCallBack;

        static int UPDATE_INTERVAL = 5; //5 seconds current location updates
        static int FASTEST_INTERVAL = 5; // 5 seconds current location update
        static int DISPLACEMENT = 3; //3 meters distance before location gets updated

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.map, container, false);

            //Map
            SupportMapFragment mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);

            //Place
            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(Activity, "AIzaSyBYxu6LmRE5baH64VLCfgZFJo1xaImdZw4");
            }

            //TextViews
            startDestinationText = view.FindViewById<TextView>(Resource.Id.txtStartDestination);
            endDestinationText = view.FindViewById<TextView>(Resource.Id.txtEndDestination);

            //Buttons
            startRadio = view.FindViewById<RadioButton>(Resource.Id.rbtnStartDestination);
            endRadio = view.FindViewById<RadioButton>(Resource.Id.rbtnEndDestination);

            //ImageView
            centreMarker = view.FindViewById<ImageView>(Resource.Id.centreMarker);

            //Layouts
            layoutStartDestination = view.FindViewById<RelativeLayout>(Resource.Id.layoutStartDestination);
            layoutEndDestination = view.FindViewById<RelativeLayout>(Resource.Id.layoutEndDestination);

            //Methods
            mapFragment.GetMapAsync(this);
            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();

            //Events
            layoutStartDestination.Click += LayoutStartDestination_Click;
            layoutEndDestination.Click += LayoutEndDestination_Click;
            startRadio.Click += StartRadio_Click;
            endRadio.Click += EndRadio_Click;

            return view;
        }

        private void StartRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 1;
            startRadio.Checked = true;
            endRadio.Checked = false;
            takeAddressFromSearch = false;
            centreMarker.SetColorFilter(Color.DarkGreen);
        }

        private void EndRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 2;
            endRadio.Checked = true;
            startRadio.Checked = false;
            takeAddressFromSearch = false;
            centreMarker.SetColorFilter(Color.Red);
        }

        private void LayoutStartDestination_Click(object sender, EventArgs e)
        {
            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("UK")
                .Build(Activity);

            StartActivityForResult(intent, 1);
        }

        private void LayoutEndDestination_Click(object sender, EventArgs e)
        {
            List<Place.Field> fields = new List<Place.Field>();
            fields.Add(Place.Field.Id);
            fields.Add(Place.Field.Name);
            fields.Add(Place.Field.LatLng);
            fields.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
                .SetCountry("UK")
                .Build(Activity);

            StartActivityForResult(intent, 2);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Activity, Resource.Raw.retromapstyle));
            mainMap = googleMap;
            mainMap.CameraIdle += MainMap_CameraIdle;
            string mapkey = Resources.GetString(Resource.String.map_key);
            mapHelper = new MapFunctionHelper(mapkey);
        }

        //method to get current position when camera is idle
        async void MainMap_CameraIdle(object sender, EventArgs e)
        {
            if(!takeAddressFromSearch)
            {
                if (addressRequest == 1)
                {
                    startLocationLatlng = mainMap.CameraPosition.Target;
                    startDestinationText.Text = await mapHelper.FindCoordinateAddress(startLocationLatlng);
                }
                else if (addressRequest == 2)
                {
                    endLocationLatlng = mainMap.CameraPosition.Target;
                    endDestinationText.Text = await mapHelper.FindCoordinateAddress(endLocationLatlng);
                }
            }
        }

        //requesting for location permissions
        bool CheckLocationPermission()
        {
            bool permissionGranted = false;

            if(ActivityCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ActivityCompat.CheckSelfPermission(Activity, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = LocationServices.GetFusedLocationProviderClient(Activity);
            mLocationCallBack = new LocationCallbackHelper();
            mLocationCallBack.MyLocation += MLocationCallback_MyLocation;
        }     

        //starts updating user's location
        void StartLocationUpdates()
        {
            if(CheckLocationPermission())
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallBack, null);
            }
        }

        //stops updating user's location
        void StopLocationUpdates()
        {
            if(locationClient != null && mLocationCallBack != null)
            {
                locationClient.RemoveLocationUpdates(mLocationCallBack);
            }
        }

        //getting location of current position
        async void GetMyLocation()
        {
            if(!CheckLocationPermission())
            {
                return;
            }

            mLastLocation = await locationClient.GetLastLocationAsync();
            //updates and zooms camera to where the device is current located
            if(mLastLocation != null)
            {
                LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17));
            }
        }

        //creating message if permissions were accepted or declined
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if(grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                Toast.MakeText(Activity, "Permission was granted", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(Activity, "Permission was denied", ToastLength.Short).Show();
            }
        }

        //this event will be fired when location changes 
        void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myPosition, 17));
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode == 1)
            {
                takeAddressFromSearch = true;
                startRadio.Checked = false;
                endRadio.Checked = false;

                var place = Autocomplete.GetPlaceFromIntent(data);
                startDestinationText.Text = place.Address.ToString();
                mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 18));
                centreMarker.SetColorFilter(Color.DarkGreen);
            }
            if (requestCode == 2)
            {
                takeAddressFromSearch = true;
                startRadio.Checked = false;
                endRadio.Checked = false;

                var place = Autocomplete.GetPlaceFromIntent(data);
                endDestinationText.Text = place.Address.ToString();
                mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 18));
                centreMarker.SetColorFilter(Color.Red);
            }
        }
    }
}