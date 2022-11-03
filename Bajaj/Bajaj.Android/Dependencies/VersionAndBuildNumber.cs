using Android.Content.PM;
using Bajaj.Droid.Dependencies;
using Bajaj.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(VersionAndBuildNumber))]
namespace Bajaj.Droid.Dependencies
{
    public class VersionAndBuildNumber : IVersionAndBuildNumber
    {
        PackageInfo _appInfo;
        public VersionAndBuildNumber()
        {
            var context = Android.App.Application.Context;
            _appInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
        }
        public string GetVersionNumber()
        {
            return _appInfo.VersionName;
        }
        public string GetBuildNumber()
        {
            return _appInfo.VersionCode.ToString();
        }
    }
}