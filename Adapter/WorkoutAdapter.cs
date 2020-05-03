using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Fitness_Diary.Models;
using Fitness_Diary.Fragments;
using Android.Graphics;
using Android.Support.Design.Widget;

namespace Fitness_Diary.Adapter
{
    class WorkoutAdapter : RecyclerView.Adapter
    {
        public event EventHandler<WorkoutAdapterClickEventArgs> ItemClick;
        public event EventHandler<WorkoutAdapterClickEventArgs> ItemLongClick;
        List<Workout> Items;

        public WorkoutAdapter(List<Workout> Data)
        {
           Items = Data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.workout_row, parent, false);

            var vh = new WorkoutAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            CalendarFragment calendar = new CalendarFragment();

            var holder = viewHolder as WorkoutAdapterViewHolder;

            if(Items[position].Date == CalendarFragment.selectedDay)
            {
                holder.workoutText.Text = Items[position].Workouts;
                holder.repText.Text = Items[position].Rep;
            }
            else
            {
                holder.workoutLayout.LayoutParameters.Height = 0;
            }
        }

        public override int ItemCount => Items.Count;

        void OnClick(WorkoutAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(WorkoutAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class WorkoutAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView workoutText { get; set; }
        public TextView repText { get; set; }
        public LinearLayout workoutLayout { get; set; }

        public WorkoutAdapterViewHolder(View itemView, Action<WorkoutAdapterClickEventArgs> clickListener,
                            Action<WorkoutAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView
            workoutText = (TextView)itemView.FindViewById(Resource.Id.txtCalendarWorkout);
            repText = (TextView)itemView.FindViewById(Resource.Id.txtCalendarReps);

            //Layout
            workoutLayout = (LinearLayout)itemView.FindViewById(Resource.Id.layoutWorkoutRow);

            itemView.Click += (sender, e) => clickListener(new WorkoutAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new WorkoutAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class WorkoutAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}