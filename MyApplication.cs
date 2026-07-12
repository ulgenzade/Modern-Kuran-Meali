// C#
using System;
using Android.App;
using Android.Util;

[Application]
public class MyApplication : Application
{
    const string Tag = "APP_UNHANDLED";

    public MyApplication(IntPtr handle, Android.Runtime.JniHandleOwnership transfer) : base(handle, transfer) { }

    public override void OnCreate()
    {
        base.OnCreate();

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            Log.Error(Tag, ex?.ToString() ?? "(null)");
            if (ex is Android.Runtime.JavaProxyThrowable jp)
                Log.Error(Tag, "Java stack:\n" + jp.JavaStackTrace);
        };

        AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
        {
            var ex = e.Exception;
            Log.Error(Tag, ex?.ToString() ?? "(null)");
            if (ex is Android.Runtime.JavaProxyThrowable jp)
                Log.Error(Tag, "Java stack:\n" + jp.JavaStackTrace);
            // e.Handled = true; // test amaçlı gerekirse aktif edin
        };
    }
}
