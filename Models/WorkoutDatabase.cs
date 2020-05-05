using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Fitness_Diary.Models
{
    class WorkoutDatabase
    {
        public string Date { get; set; }
        public string Workouts { get; set; }
        public string Rep { get; set; }
        public string WorkoutID { get; set; }
    }
}