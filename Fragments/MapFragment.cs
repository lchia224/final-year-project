﻿using System;
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
using System.Timers;
using Android.Support.Design.Widget;

namespace Fitness_Diary.Fragments
{
    public class MapFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
    {
        //Fragment Transaction
        SupportMapFragment mapFragment;
        Android.Support.V4.App.FragmentTransaction fragmentTransaction;

        //Layouts
        RelativeLayout layoutStartDestination;
        RelativeLayout layoutEndDestination;

        //Bottomsheets
        BottomSheetBehavior timerBottomSheetBehaviour;
        BottomSheetBehavior datasheetBottomSheetBehaviour;

        //TextViews
        TextView startDestinationText;
        TextView endDestinationText;
        TextView TimerText;
        TextView TimerTotalText;
        TextView DistanceTotalText;
        TextView SpeedText;

        //ImageView
        ImageView centreMarker;
        ImageView runCentreMarker;

        //Buttons
        RadioButton startRadio;
        RadioButton endRadio;
        Button mapStartButton;
        Button mapDoneButton;
        Button datasheetDoneButton;

        //Helpers
        MapFunctionHelper mapHelper;

        //TripDetails
        LatLng startLocationLatlng;
        LatLng endLocationLatlng;

        //Flags
        int addressRequest = 1;
        bool takeAddressFromSearch;

        //Timer
        Timer timer;

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

        double sec = 0, min = 0, hr = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.map, container, false);

            //Map
            mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);

            //Place
            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(Activity, "AIzaSyBYxu6LmRE5baH64VLCfgZFJo1xaImdZw4");
            }

            //TextViews
            startDestinationText = view.FindViewById<TextView>(Resource.Id.txtStartDestination);
            endDestinationText = view.FindViewById<TextView>(Resource.Id.txtEndDestination);
            TimerText = view.FindViewById<TextView>(Resource.Id.txtTimer);
            TimerTotalText = view.FindViewById<TextView>(Resource.Id.txtTotalTime);
            DistanceTotalText = view.FindViewById<TextView>(Resource.Id.txtTotalDistance);
            SpeedText = view.FindViewById<TextView>(Resource.Id.txtSpeed);
            

            //Buttons
            startRadio = view.FindViewById<RadioButton>(Resource.Id.rbtnStartDestination);
            endRadio = view.FindViewById<RadioButton>(Resource.Id.rbtnEndDestination);
            mapStartButton = view.FindViewById<Button>(Resource.Id.btnMapStart);
            mapDoneButton = view.FindViewById<Button>(Resource.Id.btnMapDone);
            datasheetDoneButton = view.FindViewById<Button>(Resource.Id.btnDatasheetDone);

            //ImageView
            centreMarker = view.FindViewById<ImageView>(Resource.Id.centreMarker);
            runCentreMarker = view.FindViewById<ImageView>(Resource.Id.runCentreMarker);

            //Layouts
            layoutStartDestination = view.FindViewById<RelativeLayout>(Resource.Id.layoutStartDestination);
            layoutEndDestination = view.FindViewById<RelativeLayout>(Resource.Id.layoutEndDestination);

            //Bottomsheets
            FrameLayout timerView = view.FindViewById<FrameLayout>(Resource.Id.timer_bottomsheet);
            timerBottomSheetBehaviour = BottomSheetBehavior.From(timerView);
            FrameLayout datasheetView = view.FindViewById<FrameLayout>(Resource.Id.datasheet_bottomsheet);
            datasheetBottomSheetBehaviour = BottomSheetBehavior.From(datasheetView);

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
            mapStartButton.Click += MapStartButton_Click;
            datasheetDoneButton.Click += DatasheetDoneButton_Click;

            mapDoneButton.Click += MapDoneButton_Click;

            return view;
        }

        private void DatasheetDoneButton_Click(object sender, EventArgs e)
        {
            datasheetBottomSheetBehaviour.State = BottomSheetBehavior.StateCollapsed;
            mapStartButton.RefreshDrawableState();
            mapStartButton.Enabled = true;
            mapStartButton.Text = "Start";
            EnableMapFunctions();
            mapHelper.ResetRoute();
            sec = 0;
            min = 0;
            hr = 0;
        }

        private void MapDoneButton_Click(object sender, EventArgs e)
        {
            timerBottomSheetBehaviour.State = BottomSheetBehavior.StateCollapsed;
            datasheetBottomSheetBehaviour.State = BottomSheetBehavior.StateExpanded;

            if (endDestinationText.Text != "Ending Destination")
            {
                TimerTotalText.Text = "Time Taken: " + hr.ToString() + ":" + min.ToString() + ":" + sec.ToString();
                DistanceTotalText.Text = "Distance Travelled: " + mapHelper.CalculateDistance().ToString() + "km";
                SpeedText.Text = "Speed: " + calculateSpeed().ToString() + "km/m";
            } else
            {
                TimerTotalText.Text = "Time Taken: " + hr.ToString() + ":" + min.ToString() + ":" + sec.ToString();
                DistanceTotalText.Text = null;
                SpeedText.Text = null;
            }

            timer.Dispose();
        }

        private double calculateSpeed()
        {
            double hoursToMin;
            double secToMin;
            double totalTime;
            double distance = mapHelper.CalculateDistance();

            hoursToMin = hr * 60;
            secToMin = sec / 60;
            totalTime = hoursToMin + secToMin + min;

            double speed = distance / totalTime;

            return speed;
        }

        async void MapStartButton_Click(object sender, EventArgs e)
        {

            mapStartButton.Text = "Please wait...";
            mapStartButton.Enabled = false;

            if (endDestinationText.Text != "Ending Destination")
            {
                string json;
                json = await mapHelper.GetDirectionJsonAsync(startLocationLatlng, endLocationLatlng);

                if ((!string.IsNullOrEmpty(json)))
                {
                    mapHelper.DrawRouteOnMap(json);
                    DisableMapFunctions();

                    await mapHelper.FindCoordinateAddress(startLocationLatlng);

                    DisplayTimerBottomSheet();
                }
            }
            else
            {
                DisableMapFunctions();

                DisplayTimerBottomSheet();
            }        
        }

        void mapButtonStartSet()
        {
            mapStartButton.Visibility = ViewStates.Visible;
        }

        void DisplayTimerBottomSheet()
        {
            timerBottomSheetBehaviour.State = BottomSheetBehavior.StateExpanded;

            timer = new Timer();
            timer.Interval = 1000; // 1 second
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        void DisableMapFunctions()
        {
            layoutEndDestination.Clickable = false;
            layoutStartDestination.Clickable = false;
            startRadio.Enabled = false;
            endRadio.Enabled = false;
            takeAddressFromSearch = true;
            centreMarker.Visibility = ViewStates.Invisible;
            runCentreMarker.Visibility = ViewStates.Visible;
        }
        void EnableMapFunctions()
        {
            layoutEndDestination.Clickable = true;
            layoutStartDestination.Clickable = true;
            startRadio.Enabled = true;
            endRadio.Enabled = true;
            takeAddressFromSearch = false;
            centreMarker.Visibility = ViewStates.Visible;
            runCentreMarker.Visibility = ViewStates.Invisible;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            sec++;
            if (sec == 60)
            {
                min++;
                sec = 0;
            }
            if (min == 60)
            {
                hr++;
                min = 0;
            }
            Activity.RunOnUiThread(() => { TimerText.Text = $"{hr} : {min} : {sec}"; });
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
            mapHelper = new MapFunctionHelper(mapkey, mainMap);
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
            if(grantResults.Length > 1)
            {
                Toast.MakeText(Activity, "Permission was granted", ToastLength.Long).Show();

                if (grantResults[0] == Permission.Granted)
                {
                    StartLocationUpdates();
                }
            }
        }

        //this event will be fired when location changes 
        void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myPosition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myPosition, 17));
        }

        //method to get current position when camera is idle
        async void MainMap_CameraIdle(object sender, EventArgs e)
        {
            if (!takeAddressFromSearch)
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
                    //mapButtonStartSet();
                }
            }
        }

        //Place search auto complete
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
                startLocationLatlng = place.LatLng;
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
                endLocationLatlng = place.LatLng;
                mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 18));
                centreMarker.SetColorFilter(Color.Red);
                //mapButtonStartSet();
            }
        }
    }
}