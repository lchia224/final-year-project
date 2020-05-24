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
using Fitness_Diary.Models;
using Microcharts;
using Microcharts.Droid;
using SkiaSharp;

namespace Fitness_Diary.Fragments
{
    public class DietPlannerFragment : Android.Support.V4.App.Fragment
    {
        ChartView chartView;
        TextView foodTextBox;
        TextView fatTextBox;
        TextView proteinTextBox;
        TextView carbTextBox;
        TextView caloriesTextBox;
        TextView chartOptionsText;
        ImageView sortImageView;
        Button submitFoodButton;

        List<Diet> dietList = new List<Diet>();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.diet_planner, container, false);

            chartView = view.FindViewById<ChartView>(Resource.Id.chartView);
            foodTextBox = view.FindViewById<TextView>(Resource.Id.txtFood);
            fatTextBox = view.FindViewById<TextView>(Resource.Id.txtFat);
            proteinTextBox = view.FindViewById<TextView>(Resource.Id.txtProtein);
            carbTextBox = view.FindViewById<TextView>(Resource.Id.txtCarb);
            caloriesTextBox = view.FindViewById<TextView>(Resource.Id.txtCalories);
            submitFoodButton = view.FindViewById<Button>(Resource.Id.btnSubmitFood);
            chartOptionsText = view.FindViewById<TextView>(Resource.Id.txtChartOption);
            sortImageView = view.FindViewById<ImageView>(Resource.Id.sortdownImage);

            submitFoodButton.Click += SubmitFoodButton_Click;
            chartOptionsText.Click += ChartOptionsText_Click;
            sortImageView.Click += ChartOptionsText_Click;

            return view;
        }


        private void ChartOptionsText_Click(object sender, EventArgs e)
        {
            PopupMenu popupMenu = new PopupMenu(Activity, chartOptionsText);
            popupMenu.MenuItemClick += PopupMenu_MenuItemClick;

            popupMenu.Menu.Add(Menu.None, 1, 1, "BarChart");
            popupMenu.Menu.Add(Menu.None, 2, 2, "LineChart");
            popupMenu.Menu.Add(Menu.None, 3, 3, "PointChart");
            popupMenu.Menu.Add(Menu.None, 4, 4, "RadarChart");

            popupMenu.Show();
        }

        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            chartOptionsText.Text = e.Item.TitleFormatted.ToString();
            string charttype = e.Item.TitleFormatted.ToString();
            DrawChart(charttype);
        }

        private void SubmitFoodButton_Click(object sender, EventArgs e)
        {
            Diet diet = new Diet();

            diet.Food = foodTextBox.Text;
            diet.Calories = int.Parse(caloriesTextBox.Text);
            dietList.Add(diet);

            foodTextBox.Text = "";
            caloriesTextBox.Text = "";

            DrawChart("BarChart");       
        }

        void DrawChart(string charttype)
        {
            List<Entry> DataList = new List<Entry>();

            foreach (Diet diet in dietList)
            {
                DataList.Add(new Entry(diet.Calories)
                {
                    Label = diet.Food,
                    ValueLabel = diet.Calories.ToString(),
                    Color = SKColor.Parse("#266489")
                });
            }

            if(charttype == "PointChart")
            {
                var chart = new PointChart() { Entries = DataList, LabelTextSize = 40f };
                chartView.Chart = chart;
            }
            else if(charttype == "LineChart")
            {
                var chart = new LineChart() { Entries = DataList, LabelTextSize = 40f };
                chartView.Chart = chart;
            }
            else if (charttype == "BarChart")
            {
                var chart = new BarChart() { Entries = DataList, LabelTextSize = 40f };
                chartView.Chart = chart;
            }
            else if (charttype == "RadarChart")
            {
                var chart = new RadarChart() { Entries = DataList, LabelTextSize = 40f };
                chartView.Chart = chart;
            }
        }
    }
}