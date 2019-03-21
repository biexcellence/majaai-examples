using MajaMobile.Utilities;
using MajaMobile.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace MajaMobile.Pages
{
    public partial class MainPageMaster : ContentPageBase
    {
        private MainPageMasterViewModel _viewModel;

        public MainPageMaster(MainPageMasterViewModel viewmodel)
        {
            InitializeComponent();
            ViewModel = _viewModel = viewmodel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.ExpandStateChanged += ViewModel_ExpandStateChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.ExpandStateChanged -= ViewModel_ExpandStateChanged;
        }

        private void ViewModel_ExpandStateChanged(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => ExpanderCanvasView.InvalidateSurface());
        }

        private void SKCanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var height = e.Info.Height * 0.2f;
            var offset = e.Info.Height * 0.4f;
            var width = e.Info.Width;
            var canvas = e.Surface.Canvas;
            canvas.Clear();
            using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = ColorScheme.TextColor.ToSKColor(), StrokeWidth = 0 })
            using (var path = new SKPath())
            {
                if (_viewModel.UserExpanded)
                {
                    path.MoveTo(width - height * 0.75f, height + offset);
                    path.LineTo(width - height * 1.5f, offset);
                    path.LineTo(width, offset);
                    path.Close();
                }
                else
                {
                    path.MoveTo(width - height * 0.75f, 0 + offset);
                    path.LineTo(width - height * 1.5f, height + offset);
                    path.LineTo(width, height + offset);
                    path.Close();
                }
                canvas.DrawPath(path, paint);
            }
        }
    }
}