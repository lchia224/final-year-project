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
using Firebase.Auth;
using Firebase.Database;
using Fitness_Diary.Fragments;
using Fitness_Diary.Helpers;
using Fitness_Diary.Models;

namespace Fitness_Diary.EventListeners
{
    class WorkoutListener : Java.Lang.Object, IValueEventListener
    {
        FirebaseAuth mAuth;

        List<WorkoutDatabase> workoutList = new List<WorkoutDatabase>();

        public event EventHandler<WorkoutDataEventArgs> WorkoutRetrieved;

        public class WorkoutDataEventArgs : EventArgs
        {
            public List<WorkoutDatabase> Workout { get; set; }
        }

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if(snapshot.Value != null)
            {
                var child = snapshot.Children.ToEnumerable<DataSnapshot>();
                workoutList.Clear();

                foreach(DataSnapshot workoutData in child)
                {
                    WorkoutDatabase workout = new WorkoutDatabase();

                    workout.WorkoutID = workoutData.Key;
                    workout.Date = workoutData.Child("date").Value.ToString();
                    workout.Workouts = workoutData.Child("workout").Value.ToString();
                    workout.Rep = workoutData.Child("reps").Value.ToString();
                    workoutList.Add(workout);
                }
                WorkoutRetrieved.Invoke(this, new WorkoutDataEventArgs { Workout = workoutList });
            }
        }

        public void Create()
        {
            mAuth = FirebaseAuth.Instance;
            DatabaseReference workoutRef = AppDataHelper.GetDatabase().GetReference("workouts");
            workoutRef.AddValueEventListener(this);
        }
    }
}