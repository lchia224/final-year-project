using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using System.Collections.ObjectModel;
using Android.Support.Design.Widget;
using Firebase.Auth;
using Firebase.Database;
using Fitness_Diary.EventListeners;
using Firebase;
using Java.Util;
using Fitness_Diary.Helpers;
using Fitness_Diary.Models;
using Android.Support.V7.Widget;
using Fitness_Diary.Adapter;

namespace Fitness_Diary.Fragments
{
    public class CalendarFragment : Android.Support.V4.App.Fragment
    {
        //TextViews
        TextView workoutText;
        TextView repText;
        TextView testText;

        //CalendarViews
        CalendarView calendarCalendarView;

        //TextBoxes
        TextView calendarWorkoutTextBox;
        TextView calendarRepTextBox;

        //Buttons
        Button selfStartButton;

        //Layouts
        BottomSheetBehavior workoutLayoutBottomSheetBehaviour;
        BottomSheetBehavior workoutRecyclerViewBottomSheetBehaviour;
        CoordinatorLayout calendarCoordinatorLayout;

        //Firebase
        FirebaseDatabase database;
        DatabaseReference dataref;
        FirebaseAuth mAuth;

        //Listeners
        WorkoutListener workoutListener;

        //RecyclerView
        RecyclerView myRecyclerView;

        //List
        List<Workout> WorkoutList;

        //TaskCompletionListeners
        TaskCompletionListener taskCompletionListener = new TaskCompletionListener();

        //Interfaces
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("workoutinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        //Variables
        public string userWorkout { get; set; }
        public string rep { get; set; }
        public string workoutID { get; set; }
        public static string selectedDay { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.calendar, container, false);

            //TextViews
            testText = view.FindViewById<TextView>(Resource.Id.txttest);

            //TextBoxes
            calendarWorkoutTextBox = view.FindViewById<TextView>(Resource.Id.txtWorkoutInput);
            calendarRepTextBox = view.FindViewById<TextView>(Resource.Id.txtRepInput);

            //CalendarViews
            calendarCalendarView = view.FindViewById<CalendarView>(Resource.Id.viewCalendar);

            //Buttons
            selfStartButton = view.FindViewById<Button>(Resource.Id.btnSelfCalendarStart);

            //Layouts
            FrameLayout workoutLayout = view.FindViewById<FrameLayout>(Resource.Id.layoutWorkout);
            workoutLayoutBottomSheetBehaviour = BottomSheetBehavior.From(workoutLayout);
            FrameLayout recyclerViewLayout = view.FindViewById<FrameLayout>(Resource.Id.layoutWorkoutBottomSheet);
            workoutRecyclerViewBottomSheetBehaviour = BottomSheetBehavior.From(recyclerViewLayout);
            calendarCoordinatorLayout = view.FindViewById<CoordinatorLayout>(Resource.Id.calendarView);

            //RecyclerView
            myRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.myRecyclerView);

            //Events
            calendarCalendarView.DateChange += CalendarCalendarView_DateChange;
            selfStartButton.Click += SelfStartButton_Click;
            calendarWorkoutTextBox.TextChanged += CalendarWorkoutTextBox_TextChanged;

            if(selectedDay == null)
            {
                string today;
                today = DateTime.Now.ToString("d/M/yyyy");

                selectedDay = today;
            }

           //RetrieveWorkout();

            return view;
        }


        private void SetupRecyclerView()
        {
            myRecyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(Activity.BaseContext));
            WorkoutAdapter adapter = new WorkoutAdapter(WorkoutList);
            myRecyclerView.SetAdapter(adapter);
        }

        private void CalendarWorkoutTextBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {         
            if(string.IsNullOrWhiteSpace(calendarWorkoutTextBox.Text))
            {
                selfStartButton.Text = "Start Workout";
                calendarRepTextBox.Visibility = ViewStates.Invisible;       
            }
            else
            {
                selfStartButton.Text = "Log Workout";
                calendarRepTextBox.Visibility = ViewStates.Visible;
            }
        }

        void InitializeFirebase()
        {
            var app = FirebaseApp.InitializeApp(Activity);

            //force creates firebase if not created already
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()

                .SetApplicationId("fitness-diary-653c2")
                .SetApiKey("AIzaSyAr8UZlPvW4pDOgvyf434A26rEFxy5t34k")
                .SetDatabaseUrl("https://fitness-diary-653c2.firebaseio.com")
                .SetStorageBucket("fitness-diary-653c2.appspot.com")
                .Build();

                app = FirebaseApp.InitializeApp(Activity, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }
        }

        void LogWorkout()
        {
            if (calendarWorkoutTextBox.Text != "" && calendarRepTextBox.Text != "")
            {
                if (selectedDay == null)
                {
                    string today = DateTime.Now.ToString("d/M/yyyy");
                    selectedDay = today;
                }

                Snackbar.Make(calendarCoordinatorLayout, "Workout was logged successfully", Snackbar.LengthShort).Show();

                string day = selectedDay;
                string workout = calendarWorkoutTextBox.Text;
                string reps = calendarRepTextBox.Text;

                //Creating HashMap to store user information to Firebase
                HashMap workoutInfo = new HashMap();

                workoutInfo.Put("date", day);
                workoutInfo.Put("workout", workout);
                workoutInfo.Put("reps", reps);

                //sets user's id to be the unique id in database
                DatabaseReference userReference = database.GetReference("workouts").Push();
                userReference.SetValue(workoutInfo);

                calendarWorkoutTextBox.Text = "";
                calendarRepTextBox.Text = "";
            }
            else
            {
                Snackbar.Make(calendarCoordinatorLayout, "Please ensure both text boxes are filled", Snackbar.LengthShort).Show();
            }
        }

        void RetrieveWorkout()
        {
            workoutListener = new WorkoutListener();
            workoutListener.Create();
            workoutListener.WorkoutRetrieved += WorkoutListener_WorkoutRetrieved;
        }

        private void WorkoutListener_WorkoutRetrieved(object sender, WorkoutListener.WorkoutDataEventArgs e)
        {
            WorkoutList = e.Workout;
            SetupRecyclerView();
        }

        private void SelfStartButton_Click(object sender, EventArgs e)
        {
            if (calendarWorkoutTextBox.Text == "")
            {
                workoutLayoutBottomSheetBehaviour.State = BottomSheetBehavior.StateExpanded;          
            } 
            else
            {
                LogWorkout();
            }
        }

        private void CalendarCalendarView_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            int day = e.DayOfMonth;
            int month = e.Month + 1;
            int year = e.Year;
            selectedDay = day + "/" + month + "/" + year;
            RetrieveWorkout();

            workoutRecyclerViewBottomSheetBehaviour.State = BottomSheetBehavior.StateExpanded;
        }
    }
}