using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public partial class LinearGradientBrushViewModel : ObservableObject
    {
        public static readonly Messenger LinearGradientBrushMessenger = new();

        private static readonly LinearGradientBrushComparer linearGradientBrushComparer = new();

        private readonly BrushEditorViewModel brushEditorVM;
        private bool inInitializeFromBrush = false;

        public LinearGradientBrushViewModel(BrushEditorViewModel brushEditor)
        {
            brushEditorVM = brushEditor;
            LinearGradientBrushMessenger.Register("GradientStopChanged", WriteToBrush);
        }

        private LinearGradientBrush brush = BrushEditorViewModel.EmptyGradientBrush;

        public LinearGradientBrush Brush
        {
            get { return brush; }
            private set
            {
                SetProperty(ref brush, value, linearGradientBrushComparer, nameof(Brush));
            }
        }

        public void Clear()
        {
            inInitializeFromBrush = true;

            brush = BrushEditorViewModel.EmptyGradientBrush;
            GradientStops.Clear();

            inInitializeFromBrush = false;
        }

        public void InitializeFromBrush(LinearGradientBrush brush)
        {
            if (inInitializeFromBrush) return;
            inInitializeFromBrush = true;

            Brush = brush;

            GradientStops.Clear();
            foreach (GradientStop stop in brush.GradientStops)
            {
                GradientStops.Add(new GradientStopViewModel(stop));
            }
            if (GradientStops.Count > 0)
            {
                SelectedStop = GradientStops.First();
            }

            StartX = brush.StartPoint.X;
            StartY = brush.StartPoint.Y;
            EndX = brush.EndPoint.X;
            EndY = brush.EndPoint.Y;
            Opacity = brush.Opacity;

            inInitializeFromBrush = false;
        }

        public void SetColor(Color color)
        {
            if (SelectedStop != null)
            {
                SelectedStop.Color = color;
            }
        }

        private void WriteToBrush()
        {
            if (inInitializeFromBrush) return;

            Brush = new()
            {
                GradientStops = [.. GradientStops.Select(s => s.GradientStop)],
                StartPoint = new(StartX, StartY),
                EndPoint = new(EndX, EndY),
                Opacity = Opacity,
            };
        }

        public ObservableCollection<GradientStopViewModel> GradientStops { get; } = [];

        [ObservableProperty]
        private GradientStopViewModel? selectedStop;

        [ObservableProperty]
        private double startX;
        partial void OnStartXChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double startY;
        partial void OnStartYChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double endX;
        partial void OnEndXChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double endY;
        partial void OnEndYChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double opacity;
        partial void OnOpacityChanged(double value) => WriteToBrush();

        public ICommand AddCommand => new RelayCommand(
            p => AddStop());

        public ICommand RemoveCommand => new RelayCommand(
            p => RemoveStop(),
            q => GradientStops.Count > 1);

        private void AddStop()
        {
            GradientStop stop = new(brushEditorVM.Color, 1.0);
            GradientStops.Add(new GradientStopViewModel(stop));
            SelectedStop = GradientStops.Last();
            WriteToBrush();
        }

        private void RemoveStop()
        {
            if (GradientStops.Count > 1)
            {
                if (SelectedStop != null)
                {
                    int idx = GradientStops.IndexOf(SelectedStop);
                    GradientStops.Remove(SelectedStop);
                    if (idx < GradientStops.Count - 1)
                        SelectedStop = GradientStops[idx];
                    else
                        SelectedStop = GradientStops.Last();
                }
                else
                {
                    GradientStops.RemoveAt(GradientStops.Count - 1);
                    SelectedStop = GradientStops.Last();
                }
                WriteToBrush();
            }
        }
    }

    public partial class GradientStopViewModel : ObservableObject
    {
        public GradientStopViewModel(GradientStop stop)
        {
            GradientStop = stop;
            Color = stop.Color;
            Brush = new SolidColorBrush(stop.Color);
            Offset = stop.Offset;
        }

        private void WriteToGradientStop()
        {
            Brush = new SolidColorBrush(Color);
            GradientStop = new GradientStop(Color, Offset);
            LinearGradientBrushViewModel.LinearGradientBrushMessenger.NotifyColleagues("GradientStopChanged");
        }

        [ObservableProperty]
        private GradientStop gradientStop;

        [ObservableProperty]
        private Color color;
        partial void OnColorChanged(Color value) => WriteToGradientStop();

        [ObservableProperty]
        private SolidColorBrush brush;

        [ObservableProperty]
        private double offset;
        partial void OnOffsetChanged(double value) => WriteToGradientStop();
    }

}
