using Android.App;
using Android.Widget;
using Android.OS;
using Utils;
using System.Net.Sockets;

namespace App
{
    [Activity(Label = "Build Server App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            Comms comms = new Comms(new TcpClient("192.168.0.30", 1490));

            button.Click += delegate { comms.Send("Request Build"); };
        }
    }
}

