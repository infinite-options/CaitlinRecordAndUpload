using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.App;
using Android.Media;
using Android.Util;
using Android.Support.V4.Content;

namespace AudioRecorderSample.Droid
{
    [Activity(Label = "AudioRecorderSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private bool isGrantedPermission;
        private string pathSave;

        public int REQUEST_PERMISSION_CODE { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted
&& CheckSelfPermission(Manifest.Permission.RecordAudio) != Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                        Manifest.Permission.WriteExternalStorage,
                        Manifest.Permission.RecordAudio
                }, REQUEST_PERMISSION_CODE);
            }
            else
            {
                isGrantedPermission = true;
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

