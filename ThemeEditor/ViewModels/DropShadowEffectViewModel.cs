using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public partial class DropShadowEffectViewModel : ObservableObject
    {
        private DropShadowEffect dropShadowEffect = BrushEditorViewModel.EmptyDropShadow;
        private static readonly DropShadowEffectComparer dropShadowEffectComparer = new();
        private bool inInitializeFromBrush = false;

        public DropShadowEffect DropShadowEffect
        {
            get { return dropShadowEffect; }
            set
            {
                SetProperty(ref dropShadowEffect, value, dropShadowEffectComparer, nameof(DropShadowEffect));
            }
        }

        public void Clear()
        {
            inInitializeFromBrush = true;

            dropShadowEffect = BrushEditorViewModel.EmptyDropShadow;

            inInitializeFromBrush = false;
        }

        public void InitializeFromDropShadowEffect(string name, DropShadowEffect dropShadowEffect)
        {
            if (inInitializeFromBrush) return;
            inInitializeFromBrush = true;

            DropShadowEffect = dropShadowEffect;

            Color = dropShadowEffect.Color;
            BlurRadius = dropShadowEffect.BlurRadius;
            ShadowDepth = dropShadowEffect.ShadowDepth;
            Direction = dropShadowEffect.Direction;
            Opacity = dropShadowEffect.Opacity;

            if (name.StartsWith("Menu"))
            {
                OptionMarkVisible = Visibility.Collapsed;
                MenuVisible = Visibility.Visible;
            }
            else if (name.StartsWith("OptionMark"))
            {
                MenuVisible = Visibility.Collapsed;
                OptionMarkVisible = Visibility.Visible;
            }

            inInitializeFromBrush = false;
        }

        private void WriteToBrush()
        {
            if (inInitializeFromBrush) return;

            DropShadowEffect = new()
            {
                Color = Color,
                BlurRadius = BlurRadius,
                ShadowDepth = ShadowDepth,
                Direction = Direction,
                Opacity = Opacity,
            };
        }

        [ObservableProperty]
        private Color color;
        partial void OnColorChanged(Color value) => WriteToBrush();

        [ObservableProperty]
        private double blurRadius;
        partial void OnBlurRadiusChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double shadowDepth;
        partial void OnShadowDepthChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double direction;
        partial void OnDirectionChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private double opacity;
        partial void OnOpacityChanged(double value) => WriteToBrush();

        [ObservableProperty]
        private Visibility optionMarkVisible = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility menuVisible = Visibility.Collapsed;

        public ICommand ResetCommand => new RelayCommand(
            p => Reset(),
            q => !IsDefault);

        private void Reset()
        {
            Color = Colors.Black;
            BlurRadius = 5d;
            ShadowDepth = 5d;
            Direction = 315d;
            Opacity = 1d;
        }

        private bool IsDefault =>
            Color == Colors.Black &&
            BlurRadius == 5d &&
            ShadowDepth == 5d &&
            Direction == 315d &&
            Opacity == 1d;
    }
}