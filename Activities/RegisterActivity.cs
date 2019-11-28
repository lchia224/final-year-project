using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Java.Lang;
using Fitness_Diary.EventListeners;
using Java.Util;

namespace Fitness_Diary.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/FitnessTheme", MainLauncher = false)]
    public class RegisterActivity : AppCompatActivity
    {
        TextInputLayout fullNameText;
        TextInputLayout emailText;
        TextInputLayout passwordText;
        Button registerButton;
        CoordinatorLayout rootView;
        TextView clickToLoginText;

        FirebaseAuth mAuth;
        FirebaseDatabase database;
        TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
        string fullname, email, password;

        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
            ConnectControl();
        }

        void ConnectControl()
        {
            fullNameText = (TextInputLayout)FindViewById(Resource.Id.txtRegisterFullName);
            emailText = (TextInputLayout)FindViewById(Resource.Id.txtRegisterEmail);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.txtRegisterPassword);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            registerButton = (Button)FindViewById(Resource.Id.btnRegister);
            clickToLoginText = (TextView)FindViewById(Resource.Id.txtClickToLogin);

            registerButton.Click += RegisterButton_Click;
            clickToLoginText.Click += ClickToLoginText_Click;
        }

        //initializing firebase
        void InitializeFirebase()
        {
            var app = FirebaseApp.InitializeApp(this);

            //force creates firebase if not created already
            if (app == null)
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
        }

        //already registered text view click event
        private void ClickToLoginText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        //register button click event
        private void RegisterButton_Click(object sender, EventArgs e)
        {           
            fullname = fullNameText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            //condition checks
            if(fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Please enter a valid name", Snackbar.LengthShort).Show();
                return;
            }
            else if(!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please enter a valid email contain @", Snackbar.LengthShort).Show();
                return;
            }
            else if(password.Length < 6)
            {
                Snackbar.Make(rootView, "Please enter a password up to 6 characters", Snackbar.LengthShort).Show();
                return;
            }

            RegisterUser(fullname, email, password);
        }

        void RegisterUser(string name, string email, string password)
        {
            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, taskCompletionListener)
                .AddOnFailureListener(this, taskCompletionListener);
        }

        //implements interfaces from TaskCompletionListener for events if success or failure 
        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User registration failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User registration was successful", Snackbar.LengthShort).Show();

            //Creating HashMap to store user information to Firebase
            HashMap userMap = new HashMap();
            userMap.Put("fullname", fullname);
            userMap.Put("email", email);

            //sets user's id to be the unique id in database
            DatabaseReference userReference = database.GetReference("users/" + mAuth.CurrentUser.Uid);
            userReference.SetValue(userMap);
        }

        //saving user information to shared preference when internet is down
        void SaveToSharedPreference()
        {
            editor = preferences.Edit();

            editor.PutString("fullname", fullname);
            editor.PutString("email", email);
            editor.Apply();
        }

        void RetrieveData()
        {
            string email = preferences.GetString("email", "");
        }
    }
}
