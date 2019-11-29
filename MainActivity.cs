using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Firebase;
using Firebase.Database;

namespace Fitness_Diary
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        Button btntestConnection;
        FirebaseDatabase database;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            btntestConnection = (Button)FindViewById(Resource.Id.mybutton);
            btntestConnection.Click += BtntestConnection_Click;
        }

        private void BtntestConnection_Click(object sender, System.EventArgs e)
        {
            InitializeDatabase();
        }

        void InitializeDatabase()
        {
            var app = FirebaseApp.InitializeApp(this);

            if(app == null)
            {
                var options = new FirebaseOptions.Builder()

                .SetApplicationId("fitness-diary-653c2")
                .SetApiKey("AIzaSyAr8UZlPvW4pDOgvyf434A26rEFxy5t34k")
                .SetDatabaseUrl("https://fitness-diary-653c2.firebaseio.com")
                .SetStorageBucket("fitness-diary-653c2.appspot.com")
                .Build();

                app = FirebaseApp.InitializeApp(this, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }

            DatabaseReference dbref = database.GetReference("UserSupport");
            dbref.SetValue("Ticket1");

            Toast.MakeText(this, "Completed", ToastLength.Short).Show();
        }
    }
}