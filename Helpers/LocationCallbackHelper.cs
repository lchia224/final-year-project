using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Fitness_Diary.Helpers
{
    class LocationCallbackHelper : LocationCallback
    {
        public EventHandler<OnLocationCapturedEventArgs> MyLocation;
        public class OnLocationCapturedEventArgs : EventArgs
        {
            public Android.Locations.Location Location { get; set; }
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            //logging if user has allowed location permission
            Log.Debug("Fitness Diary", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }
        
        //location updates on current location
        public override void OnLocationResult(LocationResult result)
        {
            if(result.Locations.Count != 0)
            {
                MyLocation?.Invoke(this, new OnLocationCapturedEventArgs
                {
                    Location = result.Locations[0]
                });
            }
        }
    }
}