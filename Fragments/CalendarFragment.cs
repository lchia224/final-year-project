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

namespace Fitness_Diary.Fragments
{
    public class CalendarFragment : Android.Support.V4.App.Fragment
    {
        //TextViews
        TextView workoutText;

        //CalendarViews
        CalendarView calendarCalendarView;

        //TextBoxes
        TextInputLayout calendarWorkoutTextBox;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.calendar, container, false);

            //TextViews
            workoutText = view.FindViewById<TextView>(Resource.Id.txtCalendarDisplay);

            //TextBoxes
            calendarWorkoutTextBox = view.FindViewById<TextInputLayout>(Resource.Id.txtWorkoutInput);

            //CalendarViews
            calendarCalendarView = view.FindViewById<CalendarView>(Resource.Id.viewCalendar);

            //Events
            calendarCalendarView.DateChange += CalendarCalendarView_DateChange;

            return view;
        }

        private void CalendarCalendarView_DateChange(object sender, CalendarView.DateChangeEventArgs e)
        {
            int day = e.DayOfMonth;
            int month = e.Month + 1;
            int year = e.Year;

            workoutText.Text = "Date: " + day + "/" + month + "/" + year;
        }
    }
}