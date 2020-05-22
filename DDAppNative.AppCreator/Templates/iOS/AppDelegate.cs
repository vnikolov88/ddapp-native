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
			LoadApplication(new App("{{SERVICE_HOST}}", "{{SERVICE_INITIAL_URL}}", "{{ONE_SIGNAL_IDENTIFIER}}", new string[] {
				{{IGNORE_URLS}}
			}));
			app.StatusBarHidden = true;

            return base.FinishedLaunching (app, options);
		}
	}
}

