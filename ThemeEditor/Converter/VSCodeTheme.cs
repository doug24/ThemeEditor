using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThemeEditor
{
    internal class VSCodeTheme
    {
        public static Dictionary<string, string> ThemeColors { get; } = [];

        private const byte b0 = 0;
        private const byte b255 = 255;

        public static string Convert(string name)
        {
            string path = $@"C:\Repos\aatemp\{name}.jsonc";
            if (!File.Exists(path))
                return string.Empty;

            ThemeColors.Clear();

            JsonDocumentOptions options = new()
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            string theme = File.ReadAllText(path);
            string clean = CleanJson(theme);
            using JsonDocument doc = JsonDocument.Parse(clean, options);

            JsonElement root = doc.RootElement;
            string type = root.GetProperty("type").GetString() ?? string.Empty;

            LoadResource(type);

            JsonElement colors = root.GetProperty("colors");


            foreach (JsonProperty property in colors.EnumerateObject())
            {
                ThemeColors.Add(property.Name, property.Value.GetString() ?? string.Empty);
            }

            if (Application.Current is App app)
            {
                foreach (ThemeMap themeMap in ThemeMaps)
                {
                    Color? source = null;
                    if (themeMap.StaticColor != null)
                    {
                        source = themeMap.StaticColor;
                    }
                    else if (!string.IsNullOrEmpty(themeMap.ForeignKey) &&
                        ThemeColors.TryGetValue(themeMap.ForeignKey, out string? hex) &&
                        TryConvert(hex, out Color c))
                    {
                        source = c;
                    }

                    if (source != null)
                    {
                        Color color = source.Value;
                        if (themeMap.ColorChanges.Count > 0)
                        {
                            byte initialTransparency = color.A;
                            ColorHSV hsv = ColorHSV.ConvertFrom(color);

                            foreach (ColorChange change in themeMap.ColorChanges)
                            {
                                int sign = type == "dark" && change.InverseForDark ? -1 : 1;
                                switch (change.Element)
                                {
                                    case Element.H:
                                        double hue = change.Change == Change.Absolute ? 0 : hsv.Hue;
                                        hsv.Hue = Math.Max(0, Math.Min(1.0, hue + sign * change.Value));
                                        break;
                                    case Element.S:
                                        double sat = change.Change == Change.Absolute ? 0 : hsv.Saturation;
                                        hsv.Saturation = Math.Max(0, Math.Min(1.0, sat + sign * change.Value));
                                        break;
                                    case Element.V:
                                        double val = change.Change == Change.Absolute ? 0 : hsv.Value;
                                        hsv.Value = Math.Max(0, Math.Min(1.0, val + sign * change.Value));
                                        break;
                                }
                            }

                            color = hsv.ToColor();
                            color.A = initialTransparency;

                            foreach (ColorChange change in themeMap.ColorChanges)
                            {
                                int sign = type == "dark" && change.InverseForDark ? -1 : 1;
                                switch (change.Element)
                                {
                                    case Element.A:
                                        byte aaa = change.Change == Change.Absolute ? b0 : color.A;
                                        color.A = (byte)(Math.Max(b0, Math.Min(b255, aaa + sign * (byte)change.Value)));
                                        break;
                                    case Element.R:
                                        byte rrr = change.Change == Change.Absolute ? b0 : color.R;
                                        color.R = (byte)(Math.Max(b0, Math.Min(b255, rrr + sign * change.Value)));
                                        break;
                                    case Element.G:
                                        byte ggg = change.Change == Change.Absolute ? b0 : color.G;
                                        color.G = (byte)(Math.Max(b0, Math.Min(b255, ggg + sign * change.Value)));
                                        break;
                                    case Element.B:
                                        byte bbb = change.Change == Change.Absolute ? b0 : color.B;
                                        color.B = (byte)(Math.Max(b0, Math.Min(b255, bbb + sign * change.Value)));
                                        break;
                                }
                            }


                            //if (themeMap.ModType == Modify.Offset)
                            //{
                            //    int sign = type == "dark" && themeMap.ReverseValuesForDark ? -1 : 1;
                            //    hsv.Saturation = Math.Max(0, Math.Min(1.0, hsv.Saturation + sign * themeMap.Saturation));
                            //    hsv.Value = Math.Max(0, Math.Min(1.0, hsv.Value + sign * themeMap.Value));
                            //    color = hsv.ToColor();
                            //}
                            //else
                            //{
                            //    ColorHSV hsv = ColorHSV.ConvertFrom(color);
                            //    if (themeMap.ModType.HasFlag(Modify.AbsoluteSaturation))
                            //        hsv.Saturation = Math.Max(0, Math.Min(1.0, themeMap.Saturation));
                            //    if (themeMap.ModType.HasFlag(Modify.AbsoluteValue))
                            //        hsv.Value = Math.Max(0, Math.Min(1.0, themeMap.Value));
                            //    color = hsv.ToColor();
                            //}
                        }

                        app.ThemeResources[themeMap.LocalKey] = new SolidColorBrush(color);
                    }
                }
            }
            return name;
        }

        internal static readonly char[] separator = ['\r', '\n'];

        private static string CleanJson(string json)
        {
            var lines = json.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                var clean = line.Trim().TrimStart('/');
                sb.AppendLine(clean);
            }

            return sb.ToString();
        }

        private static void LoadResource(string name)
        {
            if ((name.Equals("Dark", StringComparison.OrdinalIgnoreCase) ||
                 name.Equals("Light", StringComparison.OrdinalIgnoreCase)) &&
                Application.Current is App app)
            {
                app.ThemeResources.Clear();
                app.ThemeResources.Source = new Uri($"/Themes/{name}Brushes.xaml", UriKind.Relative);
            }
        }

        private static bool TryConvert(string value, out Color color)
        {
            color = Colors.Transparent;

            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            try
            {
                color = (Color)ColorConverter.ConvertFromString(value);
                return true;
            }
            catch { }
            return false;
        }

        private static readonly Color MidGray = Color.FromRgb(148, 148, 148);

        public static List<ThemeMap> ThemeMaps { get; } =
        [
            //new ThemeMap("base.focusBorder", "focusBorder"),
            //new ThemeMap("base.foreground", "foreground"),
            //new ThemeMap("base.disabledForeground", "disabledForeground"),
            //new ThemeMap("base.widget.border", "widget.border"),
            //new ThemeMap("base.widget.shadow", "widget.shadow"),
            //new ThemeMap("base.errorForeground", "errorForeground"),
            //new ThemeMap("base.icon.foreground", "icon.foreground"),
            //new ThemeMap("base.sash.hoverBorder", "icon.sash.hoverBorder"),

            new ThemeMap("PreviewText.Foreground", "editor.foreground"),
            new ThemeMap("PreviewText.Background", "editor.background"),
            new ThemeMap("PreviewText.Link", "textLink.foreground"),
            new ThemeMap("PreviewText.BigEllipsis"),
            new ThemeMap("PreviewText.Marker.Global.Background"),
            new ThemeMap("PreviewText.Marker.Local.Background"),
            //<sys:Boolean x:Key="PreviewText.SyntaxColor.Invert">true</sys:Boolean>

            new ThemeMap("Match.Highlight.Foreground"),
            new ThemeMap("Match.Highlight.Background"),
            new ThemeMap("Match.Group.0.Highlight.Background"),
            new ThemeMap("Match.Group.1.Highlight.Background"),
            new ThemeMap("Match.Group.2.Highlight.Background"),
            new ThemeMap("Match.Group.3.Highlight.Background"),
            new ThemeMap("Match.Group.4.Highlight.Background"),
            new ThemeMap("Match.Group.5.Highlight.Background"),
            new ThemeMap("Match.Group.6.Highlight.Background"),
            new ThemeMap("Match.Group.7.Highlight.Background"),
            new ThemeMap("Match.Group.8.Highlight.Background"),
            new ThemeMap("Match.Group.9.Highlight.Background"),
            new ThemeMap("Match.Skip.Foreground"),
            new ThemeMap("Match.Skip.Background"),
            new ThemeMap("Match.Replace.Foreground"),
            new ThemeMap("Match.Replace.Background"),

            new ThemeMap("Window.Background", "sideBar.background"),
            new ThemeMap("Dialog.Background", "panel.background"),
            new ThemeMap("Splitter.Background", "settings.sashBorder"),

            //<!--  Window Caption  -->
            new ThemeMap("Window.Border.Active", "window.activeBorder"),
            new ThemeMap("Window.Border.Inactive", "window.inactiveBorder"),
            new ThemeMap("Caption.Background", "titleBar.activeBackground"),
            new ThemeMap("Caption.Foreground", "titleBar.activeForeground"),
            new ThemeMap("Caption.Background.Inactive", "titleBar.activeBackground", ColorChange.FixedOffset(Element.S, -0.05), ColorChange.FixedOffset(Element.V, 0.05)),
            new ThemeMap("Caption.Foreground.Inactive", "titleBar.inactiveForeground"),
            new ThemeMap("Caption.Dialog.Background", "titleBar.activeBackground"),
            new ThemeMap("Caption.Button.Background"), // transparent
            new ThemeMap("Caption.Button.Foreground", "textPreformat.foreground"),
            new ThemeMap("Caption.Button.MouseOver.Background", "activityBar.background"),
            new ThemeMap("Caption.Button.MouseOver.Foreground", "foreground"),
            new ThemeMap("Caption.Button.Background.Inactive"),
            new ThemeMap("Caption.Button.Foreground.Inactive"),
            new ThemeMap("Caption.Button.MouseOver.Background.Inactive"),
            new ThemeMap("Caption.Button.MouseOver.Foreground.Inactive"),

            //<!--  Status Bar  -->
            new ThemeMap("StatusBar.Static.Background", "statusBar.background"),
            new ThemeMap("StatusBar.Static.Foreground", "statusBar.foreground"),
            new ThemeMap("StatusBar.Static.Border", "statusBar.border"),

            //<!--  Menu  -->
            new ThemeMap("Menu.TopLevel.Background", "menu.background"),
            new ThemeMap("Menu.Normal.Background", "menu.background"),
            new ThemeMap("Menu.Normal.Border", "menu.border"),
            new ThemeMap("Menu.Item.Foreground", "menu.foreground"),
            new ThemeMap("Menu.Item.Highlighted.Foreground", "menubar.selectionForeground"),
            new ThemeMap("Menu.Item.Highlighted.Background", "menubar.selectionBackground"),
            new ThemeMap("Menu.Item.Highlighted.Border", "menubar.selectionBorder"),
            new ThemeMap("Menu.Item.Selected.Background", "pickerGroup.border"), // little right arrow tick
            new ThemeMap("Menu.Item.Disabled.Foreground", "disabledForeground"),
            new ThemeMap("Menu.Separator.Border", "menu.separatorBackground"),
            new ThemeMap("Menu.SubMenu.Border", "menu.border"),
            new ThemeMap("Menu.SubMenu.Item.Background", "menu.background"),
            new ThemeMap("Menu.SubMenu.Item.Highlighted.Foreground", "menu.selectionForeground"),
            new ThemeMap("Menu.SubMenu.Item.Highlighted.Background", "menu.selectionBackground"),
            new ThemeMap("Menu.SubItem.Item.Highlighted.Border", "menu.selectionBorder"),
            new ThemeMap("Menu.SubMenu.Item.Disabled.Foreground", "disabledForeground"),
            new ThemeMap("Menu.CheckMark.Background", "checkbox.selectBackground"),
            new ThemeMap("Menu.CheckMark.Border", "checkbox.border"),
            new ThemeMap("Menu.CheckMark.Foreground", "checkbox.foreground"),
            new ThemeMap("Menu.CheckMark.Disabled.Background", "checkbox.background"),
            new ThemeMap("Menu.CheckMark.Disabled.Border", "disabledForeground"),
            new ThemeMap("Menu.Context.Shadow.Background", "widget.shadow"),
            new ThemeMap("Menu.DropShadowEffect"),

            //<!--  Group Box  -->
            new ThemeMap("GroupBox.Foreground", "foreground"),
            new ThemeMap("GroupBox.Border", "pickerGroup.border"),
            new ThemeMap("GroupBox.Border.Inner", Colors.Transparent),
            new ThemeMap("GroupBox.Border.Outer", Colors.Transparent),

            //<!--  RadioButton  -->
            new ThemeMap("Radio.Static.Background", "checkbox.background"),
            new ThemeMap("Radio.Static.Border", "checkbox.selectBorder"),
            new ThemeMap("Radio.Static.OptionMark", "checkbox.foreground"),
            new ThemeMap("Radio.MouseOver.Background", "checkbox.background", ColorChange.Offset(Element.V, -0.2)),
            new ThemeMap("Radio.MouseOver.Border", "focusBorder"),
            new ThemeMap("Radio.Pressed.Background","checkbox.foreground", ColorChange.FixedOffset(Element.S, 0.2), ColorChange.FixedOffset(Element.V, 0.2)),
            new ThemeMap("Radio.Pressed.Border", "focusBorder"),
            new ThemeMap("Radio.Disabled.Background", "checkbox.background", ColorChange.Offset(Element.V,-0.2)),
            new ThemeMap("Radio.Disabled.Border", "checkbox.selectBorder", ColorChange.Offset(Element.V,0.2)),
            new ThemeMap("Radio.Disabled.OptionMark", "checkbox.foreground", ColorChange.Offset(Element.V,0.2)),

            //<!--  CheckBox  -->
            new ThemeMap("OptionMark.Static.Background", "checkbox.background"),
            new ThemeMap("OptionMark.Static.Border", "checkbox.border"),
            new ThemeMap("OptionMark.Static.Glyph", "checkbox.foreground"),
            new ThemeMap("OptionMark.Checked.Background", "checkbox.selectBackground"),
            new ThemeMap("OptionMark.MouseOver.Background", "checkbox.background", ColorChange.Offset(Element.V, -0.2)),
            new ThemeMap("OptionMark.MouseOver.Border", "focusBorder"),
            new ThemeMap("OptionMark.MouseOver.Glyph", "checkbox.foreground"),
            new ThemeMap("OptionMark.Disabled.Background", "checkbox.background", ColorChange.Offset(Element.V, -0.2)),
            new ThemeMap("OptionMark.Disabled.Border", "checkbox.selectBorder", ColorChange.Offset(Element.V, 0.2)),
            new ThemeMap("OptionMark.Disabled.Glyph", "checkbox.foreground", ColorChange.Offset(Element.V, 0.2)),
            new ThemeMap("OptionMark.Pressed.Background","checkbox.foreground", ColorChange.FixedOffset(Element.S, 0.2), ColorChange.FixedOffset(Element.V, 0.2)),
            new ThemeMap("OptionMark.Pressed.Border", "focusBorder"),
            new ThemeMap("OptionMark.Pressed.Glyph", "focusBorder"),
            new ThemeMap("OptionMark.DropShadowEffect"),

            //<!--  Button  -->
            new ThemeMap("Button.Static.Foreground", "button.foreground"),
            new ThemeMap("Button.Static.Background", "button.background"),
            new ThemeMap("Button.Static.Border", "button.border"),
            new ThemeMap("Button.Default.Border", "focusBorder"),
            new ThemeMap("Button.MouseOver.Background", "button.hoverBackground", ColorChange.Offset(Element.V, -0.20)),
            new ThemeMap("Button.MouseOver.Border", "focusBorder"),
            new ThemeMap("Button.Pressed.Background", "button.foreground", ColorChange.FixedOffset(Element.S, 0.2), ColorChange.FixedOffset(Element.V, 0.2)),
            new ThemeMap("Button.Pressed.Border", "focusBorder"),
            new ThemeMap("Button.Disabled.Background", "button.background", ColorChange.FixedAbsolute(Element.S, 0.05)),
            new ThemeMap("Button.Disabled.Border", "button.selectBorder", ColorChange.Offset(Element.V, 0.2)),
            new ThemeMap("Button.Disabled.Foreground", "disabledForeground"),

            //<!-- Toggle Button -->
            //<sys:Boolean x:Key="ToggleButton.DarkImages">true</sys:Boolean>

            //<!--  Text Box  -->
            new ThemeMap("Control.Foreground", "input.foreground"),
            new ThemeMap("Control.Static.Background", "input.background"),
            new ThemeMap("Control.Static.Border", "input.border"),
            new ThemeMap("Control.Focused.Background", "input.background"),
            new ThemeMap("Control.Disabled.Foreground", "disabledForeground"),
            new ThemeMap("Control.MouseOver.InputBackground", "input.background"),
            new ThemeMap("Control.MouseOver.InputBorder", "focusBorder"),
            new ThemeMap("Control.MouseOver.Foreground", "input.foreground"),
            new ThemeMap("Control.GrayText", "input.foreground", ColorChange.Offset(Element.V, 0.2)),
            new ThemeMap("Control.InfoBackground", "inputValidation.infoBackground"),
            new ThemeMap("Control.InfoForeground", "inputForeground"),
            new ThemeMap("Control.FrameBorder", "inputValidation.infoBorder"),
            new ThemeMap("Control.Highlight", "selection.background"),

            //<!--  ComboBox  -->
            // for both drop-down list and editable, not disabled [dark gray/med- gray][dark gray/very light yellow]
            new ThemeMap("ComboBox.Static.Glyph", "dropdown.foreground", ColorChange.Offset(Element.V, 0.2)), 
            // drop-down list (not editable, like encodings) [light gray gradient/very dark gray][white smoke/very dark gray]
            new ThemeMap("ComboBox.Static.Background", "dropdown.background"), 
            // drop-down list and mouse over of editable [med gray/dark gray][light gray/darker gray]
            new ThemeMap("ComboBox.Static.Border", "dropdown.border", ColorChange.Offset(Element.V, -0.3)), 
            // editable inner border and outer button - surrounds text area [white/dark gray]
            new ThemeMap("ComboBox.Static.Editable.Background", "input.background"), 
            // editable outer border - surrounds everything [medium gray/dark- gray]
            new ThemeMap("ComboBox.Static.Editable.Border", "dropdown.border", ColorChange.Offset(Element.V, -0.3)), 
            // editable button - topmost  [transparent/transparent]
            new ThemeMap("ComboBox.Static.Editable.Button.Background"), 
            // editable border - not really visible [transparent/transparent]
            new ThemeMap("ComboBox.Static.Editable.Button.Border"), 
            // for both drop-down list and editable, not disabled [black/white]
            new ThemeMap("ComboBox.MouseOver.Glyph", "dropdown.foreground"), 
            // mouse over for drop-down list, not editable [light blue gradient/dark blue]
            new ThemeMap("ComboBox.MouseOver.Background", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.35)),
            // mouse over for drop-down list, not editable [med blue/med blue]
            new ThemeMap("ComboBox.MouseOver.Border", "pickerGroup.border"), 
            // surrounds text area of editable [white/dark gray]
            new ThemeMap("ComboBox.MouseOver.Editable.Background", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.35)),
            // editable button [light blue gradient/dark blue]
            new ThemeMap("ComboBox.MouseOver.Editable.Button.Background", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.35)), 
            // editable button [med blue/dark- blue]
            new ThemeMap("ComboBox.MouseOver.Editable.Button.Border", "pickerGroup.border"), 
            // both editable and dropdown but only on clicked while dropped, pretty worthless [black/white]
            new ThemeMap("ComboBox.Pressed.Glyph", "dropdown.foreground"),  
            // dropdown but only on clicked while dropped, pretty worthless [light+ blue gradient/dark blue]
            new ThemeMap("ComboBox.Pressed.Background", "input.background"), 
            // dropdown but only on clicked while dropped, pretty worthless [med blue/light blue]
            new ThemeMap("ComboBox.Pressed.Border", "dropdown.border"), 
            // editable but only on clicked while dropped, pretty worthless [white/black]
            new ThemeMap("ComboBox.Pressed.Editable.Background", "input.background"), 
            // editable but only on clicked while dropped, pretty worthless [med blue/med blue]
            new ThemeMap("ComboBox.Pressed.Editable.Border", "dropdown.border"), 
            // editable but only on clicked while dropped, pretty worthless [light+ blue gradient/dark blue]
            new ThemeMap("ComboBox.Pressed.Editable.Button.Background", "input.background"),
            // editable but only on clicked while dropped, pretty worthless [med blue/med blue]
            new ThemeMap("ComboBox.Pressed.Editable.Button.Border", "dropdown.border"),
            // both editable and drop-down [med gray/light gray]
            new ThemeMap("ComboBox.Disabled.Glyph", "disabledForeground"), 
            // drop-down only [light gray/dark gray]
            new ThemeMap("ComboBox.Disabled.Background"), 
            // drop-down only [light+ gray/med gray]
            new ThemeMap("ComboBox.Disabled.Border"), 
            // editable only, semi-transparent overlay [white/black]
            new ThemeMap("ComboBox.Disabled.Editable.Background"), 
            // editable only [med gray/med gray]
            new ThemeMap("ComboBox.Disabled.Editable.Border"), 
            // editable only [transparent/transparent]
            new ThemeMap("ComboBox.Disabled.Editable.Button.Background"), 
            // editable only [transparent/transparent]
            new ThemeMap("ComboBox.Disabled.Editable.Button.Border"), 
            // both editable and drop-down - the drop down part [white/med-dark gray]
            new ThemeMap("ComboBox.DropDown.Background", "dropdown.background"), 

            //<!-- ComboBox Item -->
            // editable
            new ThemeMap("ComboBoxItem.Hover.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 31)),
            new ThemeMap("ComboBoxItem.Hover.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2), ColorChange.FixedAbsolute(Element.A, 168)),
            // both types, hover over the selected item
            new ThemeMap("ComboBoxItem.SelectedHover.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 46)),
            new ThemeMap("ComboBoxItem.SelectedHover.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2), ColorChange.FixedAbsolute(Element.A, 153)),
            // editable, the selected item
            new ThemeMap("ComboBoxItem.SelectedNoFocus.Background", Color.FromArgb(61, 218, 218, 218)),
            new ThemeMap("ComboBoxItem.SelectedNoFocus.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2), ColorChange.FixedAbsolute(Element.A, 168)),
            // keyboard focus
            new ThemeMap("ComboBoxItem.Focus.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2)),
            // drop-down, the selected item
            new ThemeMap("ComboBoxItem.Selected.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 61)),
            new ThemeMap("ComboBoxItem.Selected.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2)),
            // drop-down
            new ThemeMap("ComboBoxItem.HoverFocus.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 84)),
            new ThemeMap("ComboBoxItem.HoverFocus.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2)),

            //<!--  Scroll Bar  -->
            new ThemeMap("ScrollBar.Static.Background", "sideBar.background"),//A 240
            new ThemeMap("ScrollBar.Static.Border", "sideBar.background"),//A 240
            new ThemeMap("ScrollBar.Static.Glyph", MidGray, ColorChange.Offset(Element.V, -0.20)),// C 96
            new ThemeMap("ScrollBar.Static.Thumb", "scrollbarSlider.background", ColorChange.FixedAbsolute(Element.A, 60)),// 200
            new ThemeMap("ScrollBar.MouseOver.Background", MidGray, ColorChange.Offset(Element.V, 0.25)),//B 211
            new ThemeMap("ScrollBar.MouseOver.Border", MidGray, ColorChange.Offset(Element.V, 0.25)),//B 211
            new ThemeMap("ScrollBar.MouseOver.Glyph"),// 000
            new ThemeMap("ScrollBar.MouseOver.Thumb", "scrollbarSlider.hoverBackground"),// 172
            new ThemeMap("ScrollBar.Pressed.Background", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.S, 0.2)),//C 96
            new ThemeMap("ScrollBar.Pressed.Border", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.S, 0.2)),//C 96
            new ThemeMap("ScrollBar.Pressed.Glyph"),// 255
            new ThemeMap("ScrollBar.Pressed.Thumb", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.S, 0.2)),//C 96
            new ThemeMap("ScrollBar.Disabled.Background", "sideBar.background"),//A 240
            new ThemeMap("ScrollBar.Disabled.Border", "sideBar.background"),//A 240
            new ThemeMap("ScrollBar.Disabled.Glyph", MidGray, ColorChange.Offset(Element.V, 0.16)),// 188

            //<!--  Slider  -->
            new ThemeMap("SliderThumb.Static.Foreground", MidGray, ColorChange.Offset(Element.V, 0.32)),// 230
            new ThemeMap("SliderThumb.Static.Background", MidGray, ColorChange.Offset(Element.V, 0.36)),// 240
            new ThemeMap("SliderThumb.Static.Border", MidGray, ColorChange.Offset(Element.V, 0.10)),// 172
            new ThemeMap("SliderThumb.MouseOver.Background", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.S, -0.2), ColorChange.FixedAbsolute(Element.A, 255)),// light blue 232
            new ThemeMap("SliderThumb.MouseOver.Border", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.V, -0.2), ColorChange.FixedAbsolute(Element.A, 255)),// blue 173
            new ThemeMap("SliderThumb.Pressed.Background", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.S, -0.2), ColorChange.FixedAbsolute(Element.A, 255)),// light blue 232
            new ThemeMap("SliderThumb.Pressed.Border", "scrollbarSlider.hoverBackground", ColorChange.Offset(Element.V, -0.2), ColorChange.FixedAbsolute(Element.A, 255)),//C 96// blue 149
            new ThemeMap("SliderThumb.Disabled.Background", MidGray, ColorChange.Offset(Element.V, 28)),//240
            new ThemeMap("SliderThumb.Disabled.Border", MidGray, ColorChange.Offset(Element.V, 0.25)),// 211
            new ThemeMap("SliderThumb.Track.Background", MidGray, ColorChange.Offset(Element.V, 0.34)),// 234 bar
            new ThemeMap("SliderThumb.Track.Border", MidGray, ColorChange.Offset(Element.V, 0.25)),// 211 bar

            //<!--  Progress Bar  -->
            new ThemeMap("ProgressBar.Background", MidGray, ColorChange.Offset(Element.V, 0.36)),
            new ThemeMap("ProgressBar.Border", MidGray, ColorChange.Offset(Element.V, 0.16)),
            new ThemeMap("ProgressBar.Progress", "progressBar.background"),

            //<!--  Tree View  -->
            new ThemeMap("TreeView.Foreground", "editor.foreground"),
            new ThemeMap("TreeView.Background", "editor.background"),
            new ThemeMap("TreeView.Border"),
            new ThemeMap("TreeView.Section.Border"),
            new ThemeMap("TreeView.LineNumber.Deselected.Background"),
            new ThemeMap("TreeView.LineNumber.Selected.Background"),
            new ThemeMap("TreeView.LineNumber.Empty.Background"),
            new ThemeMap("TreeView.Message.Highlight.Foreground", "editor.foreground"),
            new ThemeMap("TreeView.Message.Highlight.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 90)),
            new ThemeMap("TreeView.Disabled.Background"),
            new ThemeMap("TreeViewItem.TreeArrow.Static.Fill", "dropdown.foreground"),
            new ThemeMap("TreeViewItem.TreeArrow.Static.Stroke", "dropdown.foreground"),
            new ThemeMap("TreeViewItem.TreeArrow.Static.Checked.Fill", "pickerGroup.border"),
            new ThemeMap("TreeViewItem.TreeArrow.Static.Checked.Stroke", "pickerGroup.border"),// ColorChange.Offset(Element.V, .20)),
            new ThemeMap("TreeViewItem.TreeArrow.MouseOver.Fill", "pickerGroup.border"),
            new ThemeMap("TreeViewItem.TreeArrow.MouseOver.Stroke", "pickerGroup.border", ColorChange.Offset(Element.V, .20)),
            new ThemeMap("TreeViewItem.TreeArrow.MouseOver.Checked.Fill", "dropdown.foreground"),
            new ThemeMap("TreeViewItem.TreeArrow.MouseOver.Checked.Stroke", "dropdown.foreground"),
            new ThemeMap("TreeViewItem.Highlight.Foreground", "editor.foreground"),
            new ThemeMap("TreeViewItem.Highlight.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 50)),
            new ThemeMap("TreeViewItem.Highlight.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2), ColorChange.FixedAbsolute(Element.A, 240)),
            new ThemeMap("TreeViewItem.InactiveHighlight.Foreground", "editor.foreground"),
            new ThemeMap("TreeViewItem.InactiveHighlight.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 50)),
            new ThemeMap("TreeViewItem.InactiveHighlight.Border", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.2), ColorChange.FixedAbsolute(Element.A, 128)),

            //<!--  Expander  -->
            new ThemeMap("Expander.Static.Arrow.Stroke", "button.foreground"),
            new ThemeMap("Expander.Static.Circle.Fill", "button.background"),
            new ThemeMap("Expander.Static.Circle.Stroke", "button.background", ColorChange.Offset(Element.V, -0.35)),
            new ThemeMap("Expander.MouseOver.Arrow.Stroke", "button.foreground"),
            new ThemeMap("Expander.MouseOver.Circle.Fill", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.35)),
            new ThemeMap("Expander.MouseOver.Circle.Stroke", "pickerGroup.border"),
            new ThemeMap("Expander.Pressed.Arrow.Stroke", "button.foreground"),
            new ThemeMap("Expander.Pressed.Circle.Fill", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.15)),
            new ThemeMap("Expander.Pressed.Circle.Stroke", "pickerGroup.border"),
            new ThemeMap("Expander.Disabled.Arrow.Stroke"),
            new ThemeMap("Expander.Disabled.Circle.Fill"),
            new ThemeMap("Expander.Disabled.Circle.Stroke"),

            //<!--  DatePicker  -->
            new ThemeMap("DatePicker.Static.Foreground"),
            new ThemeMap("DatePicker.Static.Border"),
            new ThemeMap("DatePicker.Disabled.Overlay"),
            new ThemeMap("DatePicker.TextBox.Border"),

            //<!--  DataGrid  -->
            new ThemeMap("DataGrid.Foreground", "editor.foreground"),
            new ThemeMap("DataGrid.Background", "editor.background", ColorChange.Offset(Element.V, -0.03)),
            new ThemeMap("DataGrid.Border", MidGray, ColorChange.Offset(Element.V, 0.06)),
            new ThemeMap("DataGrid.Cell.Foreground", "editor.foreground"),
            new ThemeMap("DataGrid.Cell.Background", "editor.background"),
            new ThemeMap("DataGrid.Header.Background", "editor.background", ColorChange.Offset(Element.V, -0.1)),
            new ThemeMap("DataGrid.Header.MouseOver.Background", "pickerGroup.border", ColorChange.Offset(Element.S, -0.2), ColorChange.Offset(Element.V, 0.35)),
            new ThemeMap("DataGrid.Header.Pressed.Background", "pickerGroup.border", ColorChange.FixedOffset(Element.V, 0.2)),
            new ThemeMap("DataGrid.Header.Foreground", "editor.foreground"),
            new ThemeMap("DataGrid.Header.Border", MidGray, ColorChange.Offset(Element.V, .30)),
            new ThemeMap("DataGrid.Header.Glyph","editor.foreground"),
            new ThemeMap("DataGrid.FocusBorder", "editor.foreground", ColorChange.Offset(Element.V, 0.20)),
            new ThemeMap("DataGrid.Highlight.Foreground", "editor.foreground"),
            new ThemeMap("DataGrid.Highlight.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 128)),
            new ThemeMap("DataGrid.InactiveSelectionHighlight.Foreground", "editor.foreground"),
            new ThemeMap("DataGrid.InactiveSelectionHighlight.Background", "pickerGroup.border", ColorChange.FixedAbsolute(Element.A, 80)),
        ];
    }

    public class ThemeMap
    {
        public string LocalKey { get; private set; }

        public string ForeignKey { get; private set; } = string.Empty;

        public List<ColorChange> ColorChanges { get; } = [];

        public Color? StaticColor { get; private set; }

        public ThemeMap(string localKey)
        {
            LocalKey = localKey;
        }

        public ThemeMap(string localKey, string foreignKey)
        {
            LocalKey = localKey;
            ForeignKey = foreignKey;
        }

        public ThemeMap(string localKey, string foreignKey, params ColorChange[] colorChanges)
        {
            LocalKey = localKey;
            ForeignKey = foreignKey;
            ColorChanges.AddRange(colorChanges);
        }

        public ThemeMap(string localKey, Color staticColor)
        {
            LocalKey = localKey;
            StaticColor = staticColor;
        }

        public ThemeMap(string localKey, Color staticColor, params ColorChange[] colorChanges)
        {
            LocalKey = localKey;
            StaticColor = staticColor;
            ColorChanges.AddRange(colorChanges);
        }
    }

    public record ColorChange(Element Element, double Value, Change Change, bool InverseForDark)
    {
        public static ColorChange FixedOffset(Element element, double value)
        {
            return new ColorChange(element, value, Change.Offset, false);
        }
        public static ColorChange Offset(Element element, double value)
        {
            return new ColorChange(element, value, Change.Offset, true);
        }
        public static ColorChange FixedAbsolute(Element element, double value)
        {
            return new ColorChange(element, value, Change.Absolute, false);
        }
        public static ColorChange Absolute(Element element, double value)
        {
            return new ColorChange(element, value, Change.Absolute, true);
        }
    }


    //[Flags]
    //public enum Modify
    //{
    //    Offset = 0,
    //    AbsoluteSaturation = 1,
    //    AbsoluteValue = 2,
    //}

    public enum Element { A, R, G, B, H, S, V }

    public enum Change { Offset, Absolute }
}
