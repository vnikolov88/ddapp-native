# DataDriven Application Framework

System build to create mobile apps from JSON and Data Driven design, the AppCreator generates native applications for iOS, Android and UWP.

## Getting Started

The Resources folder holds all data for the given app, to create a new app make a subfolder using the app code as the folder name.
In the folder place the necessary resources, and define the app.json
```json
{
	"AppVersion": "1.0",
	"BundleIdentifier": "com.example.appname",
	"AppHostBaseUrl": "http://ddapp.doctor-help.com",
	"AppDisplayName": "Example App"
}
```

### Prerequisites

To build the native apps you are going to need Xamarin.

#### iOS Build
You are going to need to setup remote build on the MacOS device and connect to it before you build
#### Android Build
You are going to need Android SDK installed
### Creating the app

Run the AppCreator with the given app code

```
AppCreator.exe {appCode}
```
The builds for all platforms are going to be in the output directory under the {appCode}\{buildNumber}

## Built With

* [Xamarin](https://github.com/xamarin) - Cross plafrotm build framework
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - Json parser utility
* [Android Studio](https://developer.android.com/studio/) - Android Studio for Android builds
* [Visual Studio For Mac](https://docs.microsoft.com/bg-bg/visualstudio/mac/installation) - Visual Studio port for MacOS