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
using FR.Ganfra.Materialspinner;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Android.Content.Res;
using System.Collections;

namespace Fitness_Diary.Fragments
{
    public class CalendarFragment : Android.Support.V4.App.Fragment
    {
        //TextViews
        TextView workoutText;
        TextView repText;
        TextView testText;
        TextView beginnerText;
        TextView intermediateText;

        //CalendarViews
        CalendarView calendarCalendarView;

        //TextBoxes
        EditText calendarWorkoutTextBox;
        EditText calendarRepTextBox;

        //Buttons
        Button selfStartButton;

        //Radio Button
        CheckBox beginnerCheckBox;
        CheckBox intermediateCheckBox;

        //Spinner
        MaterialSpinner workoutSpinner;

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
        List<WorkoutDatabase> workoutDatabaseList;
        List<string> muscleGroupList;
        List<WorkoutJson> workoutJsonList;

        //ArrayAdapter
        ArrayAdapter<string> muscleGroupAdapter;

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
        public string jsonData { get; set; }
        private string spinnerData { get; set; }

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
            beginnerText = view.FindViewById<TextView>(Resource.Id.txtBeginnerSelector);
            intermediateText = view.FindViewById<TextView>(Resource.Id.txtIntermediateSelector);

            //TextBoxes
            calendarWorkoutTextBox = view.FindViewById<EditText>(Resource.Id.txtWorkoutInput);
            calendarRepTextBox = view.FindViewById<EditText>(Resource.Id.txtRepInput);

            //CalendarViews
            calendarCalendarView = view.FindViewById<CalendarView>(Resource.Id.viewCalendar);

            //Buttons
            selfStartButton = view.FindViewById<Button>(Resource.Id.btnSelfCalendarStart);

            //RadioButton
            beginnerCheckBox = view.FindViewById<CheckBox>(Resource.Id.cbBeginnerSelector);
            intermediateCheckBox = view.FindViewById<CheckBox>(Resource.Id.cbIntermediateSelector);

            //Spinner
            workoutSpinner = view.FindViewById<MaterialSpinner>(Resource.Id.workoutSpinner);

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
            beginnerCheckBox.Click += BeginnerButton_Click;
            intermediateCheckBox.Click += IntermediateButton_Click;
            workoutSpinner.ItemSelected += WorkoutSpinner_ItemSelected;

            SetupWorkoutSpinner();

            return view;
        }

        private void WorkoutSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            spinnerData = workoutSpinner.GetItemAtPosition(e.Position).ToString();

            if (workoutSpinner.GetItemAtPosition(e.Position).ToString() == "Select workout")
            {
                EnableSelfFunction();
            }
            else 
            {
                DisableSelfFunction();
            }
        }

        public void SetupWorkoutSpinner()
        {
            muscleGroupList = new List<string>();
            muscleGroupList.Add("Core");
            muscleGroupList.Add("HIITs");
            muscleGroupList.Add("Glutes");

            muscleGroupAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleSpinnerDropDownItem, muscleGroupList);
            muscleGroupAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            workoutSpinner.Adapter = muscleGroupAdapter;

        }

        private void IntermediateButton_Click(object sender, EventArgs e)
        {
            beginnerCheckBox.Checked = false;

            if (intermediateCheckBox.Checked == true)
            {
                DisableSelfFunction();
            }
            else
            {
                EnableSelfFunction();
            }
        }

        private void BeginnerButton_Click(object sender, EventArgs e)
        {
            intermediateCheckBox.Checked = false;

            if (beginnerCheckBox.Checked == true)
            {
                DisableSelfFunction();
            }
            else
            {
                EnableSelfFunction();
            }
        }

        private void SetupRecyclerView()
        {
            myRecyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(Activity.BaseContext));
            WorkoutDatabaseAdapter adapter = new WorkoutDatabaseAdapter(workoutDatabaseList);
            myRecyclerView.SetAdapter(adapter);
        }

        private void CalendarWorkoutTextBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        { 
            if(string.IsNullOrWhiteSpace(calendarWorkoutTextBox.Text))
            {
                selfStartButton.Text = "Start Workout";
                calendarRepTextBox.Visibility = ViewStates.Invisible;
                EnableAutoFunction();
            }
            else
            {
                selfStartButton.Text = "Log Workout";
                calendarRepTextBox.Visibility = ViewStates.Visible;
                DisableAutoFunction();
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
            if (selectedDay == null)
            {
                string today = DateTime.Now.ToString("d/M/yyyy");
                selectedDay = today;
            }  

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
            Snackbar.Make(calendarCoordinatorLayout, "Workout was logged successfully", Snackbar.LengthShort).Show();

            calendarWorkoutTextBox.Text = "";
            calendarRepTextBox.Text = "";          
        }

        void RetrieveWorkout()
        {
            workoutListener = new WorkoutListener();
            workoutListener.Create();
            workoutListener.WorkoutRetrieved += WorkoutListener_WorkoutRetrieved;
        }

        private void WorkoutListener_WorkoutRetrieved(object sender, WorkoutListener.WorkoutDataEventArgs e)
        {
            workoutDatabaseList = e.Workout;
            SetupRecyclerView();
        }

        private void SelfStartButton_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(calendarWorkoutTextBox.Text))
            {
                LogWorkout();
            }
            else if (spinnerData != "Select workout")
            {
                if(beginnerCheckBox.Checked)
                {
                    LogJsonWorkout(beginnerText.Text);
                    beginnerCheckBox.Checked = false;
                    workoutSpinner.SetSelection(0);
                }
                else if(intermediateCheckBox.Checked)
                {
                    LogJsonWorkout(intermediateText.Text);
                    intermediateCheckBox.Checked = false;
                    workoutSpinner.SetSelection(0);
                }
            }
            else
            {
                Snackbar.Make(calendarCoordinatorLayout, "Please ensure requirements are filled", Snackbar.LengthShort).Show();
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

        void LogJsonWorkout(string jsonFile)
        {
            if (selectedDay == null)
            {
                string today = DateTime.Now.ToString("d/M/yyyy");
                selectedDay = today;
            }

            string day = selectedDay;

            AssetManager assets = Activity.Assets;

            using (StreamReader stream = new StreamReader(assets.Open(jsonFile + ".json")))
            {
                jsonData = stream.ReadToEnd();

                WorkoutJson workoutList = JsonConvert.DeserializeObject<WorkoutJson>(jsonData);

                HashMap workoutInfo = new HashMap();

                workoutInfo.Put("date", day);

                if(spinnerData == "Core")
                {
                    foreach (var w in workoutList.Core)
                    {
                        workoutInfo.Put("workout", w.Workout);
                        workoutInfo.Put("reps", w.Reps);
                    }
                } else if(spinnerData == "HIITs")
                {
                    foreach (var w in workoutList.Hiits)
                    {
                        workoutInfo.Put("workout", w.Workout);
                        workoutInfo.Put("reps", w.Reps);
                    }
                } else if(spinnerData == "Glutes")
                {
                    foreach (var w in workoutList.Glutes)
                    {
                        workoutInfo.Put("workout", w.Workout);
                        workoutInfo.Put("reps", w.Reps);
                    }
                }

                DatabaseReference userReference = database.GetReference("workouts").Push();
                userReference.SetValue(workoutInfo);
                Snackbar.Make(calendarCoordinatorLayout, "Workout was logged successfully", Snackbar.LengthShort).Show();
            }           
        }

        void DisableAutoFunction()
        {
            beginnerCheckBox.Enabled = false;
            intermediateCheckBox.Enabled = false;
            workoutSpinner.Enabled = false;
        }

        void DisableSelfFunction()
        {
            calendarWorkoutTextBox.Enabled = false;
            calendarRepTextBox.Enabled = false;             
        }

        void EnableAutoFunction()
        {
            beginnerCheckBox.Enabled = true;
            intermediateCheckBox.Enabled = true;
            workoutSpinner.Enabled = true;
        }
        
        void EnableSelfFunction()
        {
            calendarWorkoutTextBox.Enabled = true;
            calendarRepTextBox.Enabled = true;
        }
    }
}