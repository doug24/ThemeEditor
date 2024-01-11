# Theme Editor for dnGrep

Build the solution locally using Visual Studio 2022.

The main window has three parts:
- A set of sample controls on the left
- The set of named colors in the middle panel
- Color editor controls in the right panel.

To begin, copy an existing theme file (DarkBrushes.xaml, LightBrushes.xaml, or Sunset.xaml) and name the file using your new theme name. Put the new file in a location where it can be both read and written.

In the Theme Editor do File - Open and select your theme file.  That file will be loaded automatically each time you start the Theme Editor or until you open a different theme file for editing.

To edit a color, select the color in the middle panel and adjust it using the color sliders, choosing a web color, copying the color from another named color in the theme, or select a system color.
Note you can use transparent or partially transparent colors.

When the color is modified, but not yet committed, click the `Revert color change` button to restore it to the previous value.

To commit the color change, click the `Commit color change` button - this just saves it the runtime, not to file!

When you have a set of changes you want to keep, click File - Save, or ctrl+S to write the changes back to the source file.

There are separate named colors for most controls. This follows the pattern in the standard WPF controls and allows full flexibility for adjusting colors.
However, it is best to re-use the same colors as much as possible for a consistent and uniform appearance.  The last tab on the left of the Theme Editor
shows all the colors in the theme, sorted by color.  This gives you an indication of what colors you have re-used, and possibly where else you may want to copy
a new color as you modify the theme.

Some controls (like the dark group box) have more details (outlines) in them than are visible using the current theme files because colors are set to transparent or to the 
same color as an adjoining element. Making these details a different color can add these details back into the controls.

When you have something you would like to try in dnGrep, make a copy of the theme xaml file and put it in theme directory:
- If dnGrep is installed from the msi to Program Files, then it is %appdata%\dnGREP (C:\Users\_user_\AppData\Roaming\dnGREP).
- If you have a portable version of dnGrep (zip download) or installed outside Program Files, then it is the "Themes" subdirectory where you extracted the dnGrep.exe files.

In either case, you should see the Sunset.xaml file there already.

Modify the theme.xaml file to change the toggle button images (for the Bookmarks dialog) to this so it they will load correctly when the theme is loaded dynamically:
```
    <BitmapImage x:Key="pinBitmap" UriSource="pack://application:,,,/dnGrep;component/Images/pinDark.png" />
    <BitmapImage x:Key="unpinBitmap" UriSource="pack://application:,,,/dnGrep;component/Images/unpinDark.png" />
```
Or use the light images if your theme is light
```
    <BitmapImage x:Key="pinBitmap" UriSource="pack://application:,,,/dnGrep;component/Images/pin.png" />
    <BitmapImage x:Key="unpinBitmap" UriSource="pack://application:,,,/dnGrep;component/Images/unpin.png" />
```

Start dnGrep, open the Options dialog (F8), and select your new theme in the drop down. With your theme file loaded in dnGrep, you can edit the theme file,
and reload it using the `Reload` button in the dnGrep Options dialog, or clicking ctrl+F5.

Note: for the syntax coloring in the preview window, you have the option of light coloring or dark coloring based on the colors specified in the syntax *.xshd files.  The dark syntax colors are a color inversion of the light colors in the xshd files. Set this value to true for the preview window with a dark background:
```
    <sys:Boolean x:Key="AvalonEdit.SyntaxColor.Invert">true</sys:Boolean>
```

You can change any of the colors from SolidColorBrush to GradientBrush - see the LinearGradientBrush in the existing themes as examples.  Gradient brushes are visible but not editable in the Theme Editor.


