using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Fitness_Diary.Models;
using System.Collections.Generic;
using Fitness_Diary.Fragments;

namespace Fitness_Diary.Adapter
{
    class WorkoutDatabaseAdapter : RecyclerView.Adapter
    {
        public event EventHandler<WorkoutDatabaseAdapterClickEventArgs> ItemClick;
        public event EventHandler<WorkoutDatabaseAdapterClickEventArgs> ItemLongClick;
        List<WorkoutDatabase> Items;

        public WorkoutDatabaseAdapter(List<WorkoutDatabase> Data)
        {
            Items = Data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.workout_row, parent, false);

            var vh = new WorkoutDatabaseAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            CalendarFragment calendar = new CalendarFragment();

            var holder = viewHolder as WorkoutDatabaseAdapterViewHolder;

            if (Items[position].Date == CalendarFragment.selectedDay)
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

        void OnClick(WorkoutDatabaseAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(WorkoutDatabaseAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class WorkoutDatabaseAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView workoutText { get; set; }
        public TextView repText { get; set; }
        public LinearLayout workoutLayout { get; set; }


        public WorkoutDatabaseAdapterViewHolder(View itemView, Action<WorkoutDatabaseAdapterClickEventArgs> clickListener,
                            Action<WorkoutDatabaseAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView
            workoutText = (TextView)itemView.FindViewById(Resource.Id.txtCalendarWorkout);
            repText = (TextView)itemView.FindViewById(Resource.Id.txtCalendarReps);

            //Layout
            workoutLayout = (LinearLayout)itemView.FindViewById(Resource.Id.layoutWorkoutRow);

            itemView.Click += (sender, e) => clickListener(new WorkoutDatabaseAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new WorkoutDatabaseAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class WorkoutDatabaseAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}