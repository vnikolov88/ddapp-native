using DDAppNative.Common;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Plugin.CurrentActivity;

namespace DDAppNative.Android
{
    [Activity (Label = "{{DISPLAY_NAME}}", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ScreenOrientation = ScreenOrientation.Portrait,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : 
	global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity // superclass new in 1.3
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			CrossCurrentActivity.Current.Init(this, bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);
			LoadApplication (new App("{{SERVICE_HOST}}", "{{SERVICE_INITIAL_URL}}", "{{ONE_SIGNAL_IDENTIFIER}}", new string[] {
				{{IGNORE_URLS}} 
			}));
		}
		
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, global::Android.Content.PM.Permission[] grantResults)
		{
			Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}


