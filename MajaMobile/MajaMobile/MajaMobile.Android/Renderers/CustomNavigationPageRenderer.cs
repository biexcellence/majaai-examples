using System.Threading.Tasks;
using Android.Content;
using MajaMobile.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;
using static Android.Views.View;

//https://github.com/MvvmCross/MvvmCross/issues/3124
[assembly: ExportRenderer(typeof(NavigationPage), typeof(MajaMobile.Droid.Renderers.CustomNavigationPageRenderer))]
namespace MajaMobile.Droid.Renderers
{
    public class CustomNavigationPageRenderer : NavigationPageRenderer, IOnClickListener
    {
        public CustomNavigationPageRenderer(Context context) : base(context)
        {
        }

        protected override Task<bool> OnPopViewAsync(Page page, bool animated)
        {
            if(page is UserProfilePage profilePage && profilePage.OnBackPressed())
            {
                return Task.FromResult(false);
            }
            return base.OnPopViewAsync(page, animated);
        }
    }
}