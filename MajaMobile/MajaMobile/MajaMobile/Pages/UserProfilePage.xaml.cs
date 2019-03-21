using BiExcellence.OpenBi.Api.Commands.Users;
using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class UserProfilePage : ContentPageBase
    {
        public UserProfilePage(IUser user)
        {
            InitializeComponent();
            BindingContext = ViewModel = new UserProfileViewModel(user);
        }

        //TODO: cancel back in iOS when DataChanged

        protected override bool OnBackButtonPressed()
        {
            var changed = ((UserProfileViewModel)ViewModel).DataChanged();
            if (changed)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (await DisplayAlert("Profil bearbeiten", "Änderungen verwerfen?", "VERWERFEN", "ABBRECHEN"))
                    {
                        await Navigation.PopAsync();
                    }
                });
                return true;
            }
            return false;
        }

        private void ImageCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();
            var radius = e.Info.Height * (5f / 12f);
            var centerX = e.Info.Width / 2f;
            var centerY = e.Info.Height / 2f;
            var halfVer = 0.3f * radius;
            centerY += halfVer * 0.25f;
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.White, StrokeWidth = radius * 0.05f })
            {
                var top = centerY - halfVer;
                var halfHor = halfVer * 1.32f;
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(centerX - halfHor, top, centerX + halfHor, centerY + halfVer), 10, 10), paint);
                paint.Style = SKPaintStyle.Fill;
                halfHor = halfHor * 0.5f;
                var innerCenterY = centerY - 0.2f * halfHor;
                canvas.DrawCircle(centerX, innerCenterY, halfHor, paint);
                using (var eraserPaint = new SKPaint() { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = SKColors.Transparent, StrokeWidth = paint.StrokeWidth * 1.05f, BlendMode = SKBlendMode.Src, StrokeCap = SKStrokeCap.Round })
                using (var eraserPath = new SKPath())
                {
                    var h2 = 0.4f * halfHor;
                    eraserPath.MoveTo(centerX, innerCenterY - h2);
                    eraserPath.LineTo(centerX, innerCenterY + h2);
                    eraserPath.MoveTo(centerX - h2, innerCenterY);
                    eraserPath.LineTo(centerX + h2, innerCenterY);
                    canvas.DrawPath(eraserPath, eraserPaint);
                    eraserPath.Reset();
                    eraserPaint.StrokeCap = SKStrokeCap.Butt;
                    eraserPath.MoveTo(centerX - halfHor, top);
                    eraserPath.LineTo(centerX + halfHor, top);
                    canvas.DrawPath(eraserPath, eraserPaint);
                }
                paint.Style = SKPaintStyle.Stroke;
                using (var path = new SKPath())
                {
                    top = centerY - halfVer * 1.5f;
                    var left = centerX - halfHor;
                    var right = centerX + halfHor;
                    path.AddArc(new SKRect(left, top, right, top + right - left), 187.5f, 165);
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void OverlayCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear();
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ColorScheme.ImageOverlayColor.ToSKColor(), StrokeWidth = 0 })
            {
                canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, e.Info.Height * (5f / 12f), paint);
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            //TODO: open picture from camera or file, crop and save to Viewmodel.User
        }
    }
}

namespace MajaMobile.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        public IUser User { get; }
        private IUser _originalUser;
        public ICommand SaveCommand { get; }

        public UserProfileViewModel(IUser user)
        {
            _originalUser = user;
            User = new MajaUser(user);
            SaveCommand = new Command(Save);
        }

        public bool DataChanged()
        {
            return !User.Equals(_originalUser);
        }

        private async void Save()
        {
            if (IsBusy)
                return;
            User.Firstname = User.Firstname.Trim();
            User.Lastname = User.Lastname.Trim();
            if (!DataChanged())
                GoBack();
            IsBusy = true;
            try
            {
                await SessionHandler.Instance.ExecuteOpenbiCommand((s, t) => s.CreateUser(User));
                SessionHandler.Instance.OpenBiUser = User;
                GoBack();
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private class MajaUser : User, IEquatable<IUser>
        {
            public MajaUser(IUser user) : base(user.Username)
            {
                foreach (var prop in typeof(IUser).GetTypeInfo().DeclaredProperties)
                {
                    if (prop.CanWrite)
                        prop.SetValue(this, prop.GetValue(user));
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj == this) return true;
                if (obj is IUser user)
                    return Equals(user);
                return false;
            }

            public bool Equals(IUser other)
            {
                if (other == null) return false;
                return Firstname == other.Firstname && Lastname == other.Lastname && Birthdate == other.Birthdate && Picture == other.Picture;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 13;
                    hashCode = (hashCode * 397) ^ Username.GetHashCode();
                    hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Firstname) ? Firstname.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (!string.IsNullOrEmpty(Lastname) ? Lastname.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }
    }
}