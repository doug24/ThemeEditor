using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for ColorEditor.xaml
    /// </summary>
    public partial class ColorEditor : UserControl
    {
        private ColorEditorViewModel vm;

        public ColorEditor()
        {
            InitializeComponent();
            vm = new ColorEditorViewModel();
            DataContext = vm;
        }

        public Color Color
        {
            get => vm.Color;
            set => vm.Color = value;
        }
    }
}
