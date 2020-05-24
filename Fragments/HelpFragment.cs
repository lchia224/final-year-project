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

namespace Fitness_Diary.Fragments
{
    public class HelpFragment : Android.Support.V4.App.Fragment
    {

        EditText contactName;
        EditText contactEmail;
        EditText contactSubject;
        EditText contactDescription;
        Button contactSubmit;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.help, container, false);

            contactName = view.FindViewById<EditText>(Resource.Id.txtContactName);
            contactEmail = view.FindViewById<EditText>(Resource.Id.txtContactEmail);
            contactSubject = view.FindViewById<EditText>(Resource.Id.txtContactSubject);
            contactDescription = view.FindViewById<EditText>(Resource.Id.txtContactDescription);
            contactSubmit = view.FindViewById<Button>(Resource.Id.btnContactSubmit);

            contactSubmit.Click += ContactSubmit_Click;

            return view;
        }

        private void ContactSubmit_Click(object sender, EventArgs e)
        {
            contactName.Text = "";
            contactEmail.Text = "";
            contactSubject.Text = "";
            contactDescription.Text = "";
        }
    }
}