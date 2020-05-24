using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Firebase;
using Firebase.Database;
using System;
using Fitness_Diary.Adapter;
using Fitness_Diary.Fragments;
using Android.Graphics;
using Android.Support.V4.View;
using Com.Ittianyu.Bottomnavigationviewex;
using Android.Content;

namespace Fitness_Diary
{
    [Activity(Label = "@string/app_name", Theme = "@style/FitnessTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        //Views
        ViewPager viewpager;
        BottomNavigationViewEx bnve;

        //Fragments
        CalendarFragment calendarFragment = new CalendarFragment();
        MapFragment mapFragment = new MapFragment();
        DietPlannerFragment dietPlannerFragment = new DietPlannerFragment();
        HelpFragment helpFragment = new HelpFragment();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ConnectViews();
        }

        void ConnectViews()
        {
            //Setting up the bottom navigation bar
            bnve = (BottomNavigationViewEx)FindViewById(Resource.Id.bnve);

            //makes words to be showing in the nav bar         
            bnve.EnableItemShiftingMode(false);
            bnve.EnableShiftingMode(false);
            bnve.NavigationItemSelected += Bnve_NavigationItemSelected;

            BnveToAccentColor(0);

            viewpager = (ViewPager)FindViewById(Resource.Id.viewpager);
            viewpager.OffscreenPageLimit = 4;
            viewpager.BeginFakeDrag();

            SetupViewPager();
        }

        //method to cycle between different pages via nav bar buttons
        private void Bnve_NavigationItemSelected(object sender, Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            if (e.Item.ItemId == Resource.Id.navCalendar)
            {
                viewpager.SetCurrentItem(0, true);
                BnveToAccentColor(0);
            }
            else if (e.Item.ItemId == Resource.Id.navMap)
            {
                viewpager.SetCurrentItem(1, true);
                BnveToAccentColor(1);
            }
            else if (e.Item.ItemId == Resource.Id.navDiet)
            {
                viewpager.SetCurrentItem(2, true);
                BnveToAccentColor(2);
            }
            else if (e.Item.ItemId == Resource.Id.navHelp)
            {
                viewpager.SetCurrentItem(3, true);
                BnveToAccentColor(3);
            }
        }

        void BnveToAccentColor(int index)
        {
            //Setting icon on nav bar to colour white
            var img0 = bnve.GetIconAt(0);
            var txt0 = bnve.GetLargeLabelAt(0);
            img0.SetColorFilter(Color.Rgb(255, 255, 255));
            txt0.SetTextColor(Color.Rgb(255, 255, 255)); 

            var img1 = bnve.GetIconAt(1);
            var txt1 = bnve.GetLargeLabelAt(1);
            img1.SetColorFilter(Color.Rgb(255, 255, 255));
            txt1.SetTextColor(Color.Rgb(255, 255, 255));

            var img3 = bnve.GetIconAt(2);
            var txt3 = bnve.GetLargeLabelAt(2);
            img3.SetColorFilter(Color.Rgb(255, 255, 255));
            txt3.SetTextColor(Color.Rgb(255, 255, 255));

            var img4 = bnve.GetIconAt(3);
            var txt4 = bnve.GetLargeLabelAt(3);
            img4.SetColorFilter(Color.Rgb(255, 255, 255));
            txt4.SetTextColor(Color.Rgb(255, 255, 255));

            //Sets different colour on selected icon on nav bar
            var imgindex = bnve.GetIconAt(index);
            var textindex = bnve.GetLargeLabelAt(index);
            imgindex.SetColorFilter(Color.Rgb(24, 191, 242));
            textindex.SetTextColor(Color.Rgb(24, 191, 242)); 
        }

        //creating fragments for the nav bar
        private void SetupViewPager()
        {
            ViewPagerAdapter adapter = new ViewPagerAdapter(SupportFragmentManager);
            adapter.AddFragment(calendarFragment, "Calendar");
            adapter.AddFragment(mapFragment, "Map");
            adapter.AddFragment(dietPlannerFragment, "Diet Planner");
            adapter.AddFragment(helpFragment, "Help & Support");
            viewpager.Adapter = adapter;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}