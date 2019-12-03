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
using Android.Gms.Location.Places.UI;
using Android.Gms.Location.Places;
using Android.Gms.Location;
using Android.Content.PM;
using Fitness_Diary.Helpers;

namespace Fitness_Diary.Fragments
{
    public class MapFragment : Android.Support.V4.App.Fragment, IOnMapReadyCallback
    {
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

            SupportMapFragment mapFragment = (SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();

            return view;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Activity, Resource.Raw.retromapstyle));

            mainMap = googleMap;
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
    }
}