using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;

namespace Kairos.App.Platforms.Android
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Android.Content.Intent.ActionView },
        Categories = new[]
        {
            Android.Content.Intent.CategoryDefault,
            Android.Content.Intent.CategoryBrowsable
        },
        DataScheme = "com.googleusercontent.apps.313140633739-nt3so9u1gpcp921o16veet4r2vfhet5h",
        DataHost = "oauth2redirect")]
    public class WebAuthenticatorCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
    }
}
