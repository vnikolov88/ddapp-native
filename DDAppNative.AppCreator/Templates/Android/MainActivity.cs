using DDAppNative.Common;
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace DDAppNative.Android
{
    [Activity (Label = "DoctorHelpMobile.Android.Android", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : 
	global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity // superclass new in 1.3
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new App ("{{APP_CODE}}", "{{SERVICE_HOST}}")); // method is new in 1.3
		}
	}


}

