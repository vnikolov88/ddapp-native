using DDAppNative.Common;
using Foundation;
using UIKit;

namespace DDAppNative.iOS
{
    [Register ("AppDelegate")]
	public partial class AppDelegate : 
	global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate // superclass new in 1.3
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();
			LoadApplication (new App ("{{APP_CODE}}", "{{SERVICE_HOST}}"));  // method is new in 1.3
            app.StatusBarHidden = true;
            
            return base.FinishedLaunching (app, options);
		}
	}
}

