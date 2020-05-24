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
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Fitness_Diary.EventListeners;

namespace Fitness_Diary.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/FitnessTheme", MainLauncher = true)]
    public class LoginActivity : AppCompatActivity
    {
        TextInputLayout emailText;
        TextInputLayout passwordText;
        TextView clickToRegisterText;
        Button loginButton;
        CoordinatorLayout rootView;
        FirebaseAuth mAuth;

        TaskCompletionListener taskCompletionListener = new TaskCompletionListener();

        string email, password;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);

            emailText = (TextInputLayout)FindViewById(Resource.Id.txtLoginEmail);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.txtLoginPassword);
            loginButton = (Button)FindViewById(Resource.Id.btnLogin);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            clickToRegisterText = (TextView)FindViewById(Resource.Id.txtClickToRegister);

            InitializeFirebase();

            clickToRegisterText.Click += ClickToRegisterText_Click;
            loginButton.Click += LoginButton_Click;

        }

        //login button click event
        private void LoginButton_Click(object sender, EventArgs e)
        {
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please provide a valid email", Snackbar.LengthShort).Show();
                return;
            }
            else if(password.Length < 6)
            {
                Snackbar.Make(rootView, "Please provide a valid password", Snackbar.LengthShort).Show();
                return;
            }

            taskCompletionListener.Success += TaskCompletionListener_Success;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(taskCompletionListener)
                .AddOnFailureListener(taskCompletionListener);
        }

        //if email and password do not match firebase data then login failed
        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "Login Failed", Snackbar.LengthShort).Show();
        }

        //if email and password matches firebase data then user is sent to the home screen
        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        //haven't registered text view click event
        private void ClickToRegisterText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RegisterActivity));
        }

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
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
            }
        }
    }
}