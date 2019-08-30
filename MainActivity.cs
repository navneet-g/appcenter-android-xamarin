using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Auth;

using Microsoft.AppCenter.Data;
using Android.Content.PM;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace adnroid_xamarin
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            AppCenter.LogLevel = LogLevel.Verbose;
            AppCenter.SetLogUrl("https://in-integration.dev.avalanch.es");
            Data.SetTokenExchangeUrl("https://token-exchange-mbaas-integration.dev.avalanch.es/v0.1");
            Auth.SetConfigUrl("https://config-integration.dev.avalanch.es");

            AppCenter.Start("43c640a2-f91b-4cd8-9d9d-f49f241b22d9",
                   typeof(Analytics), typeof(Crashes), typeof(Auth));

            Auth.SetEnabledAsync(true);

            base.OnCreate(savedInstanceState);

            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            
            
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            SignIn(sender);            
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        async void SignIn(object sender, bool onRetry = false)
        {
            try
            {
                var enabled = await Auth.IsEnabledAsync();
                UserInformation userInfo = null;
                string email = "Unknown";
                if (enabled)
                {
                    // Sign-in succeeded.
                    userInfo = await Auth.SignInAsync();
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwToken = tokenHandler.ReadJwtToken(userInfo.IdToken);

                    // Get email name.
                    email = jwToken.Claims.FirstOrDefault(t => t.Type == "emails")?.Value;
                    if (string.IsNullOrEmpty(email))
                    {
                        if (onRetry)
                        {
                            email = "Unknown display name.";
                        }
                        else
                        { 
                            Auth.SignOut();
                            SignIn(sender, true);
                            return;
                        }                        
                    }

                }
                else
                {
                    await Auth.SetEnabledAsync(true);
                }
                View view = (View)sender;
                Snackbar.Make(view, "User is logged in as: " + email, Snackbar.LengthLong)
                    .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }

    }
}

