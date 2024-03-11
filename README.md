# Theme Editor for dnGrep

Download the Release package, or build the solution locally using Visual Studio 2022.

The main window has three parts:
- A set of sample controls on the left
- The set of named colors (the theme resources) in the middle panel
- Color editor controls in the right panel.

There are two ways to start - from an existing theme file or importing a theme from VS Code (see below). The Theme Editor zip file contains three theme files (DarkBrushes.xaml, LightBrushes.xaml, or Sunset.xaml)  or you can find the source for these and other [themes in the dnGrep source repository](https://github.com/dnGrep/dnGrep/tree/master/dnGREP.WPF/Themes).

Copy an existing theme file and name the file using your new theme name. Put the new file in a location where it can be both read and written. This can be the dnGrep themes directory so you can load the file in both the Theme Editor and dnGrep from the same location (see below for the location of the dnGrep themes directory).

In the Theme Editor do File - Open and select your theme file.  That file will be loaded automatically each time you start the Theme Editor or until you open a different theme file for editing.

The existing theme files have both SolidColorBrushes and LinearGradientBrushes. Gradient brushes are now editable in the Theme Editor, and you can switch between solid color brushes and linear gradient brushes. See the existing themes for examples. There are also two drop shadow resources in the theme and a special editor for them.

To edit a color, select the color in the middle panel and adjust it using the color sliders, choosing a web color, copying the color from another named color in the theme, or select a system color.

You can use transparent or partially transparent colors.

When the color is modified, but not yet committed, click the `Revert color change` button to restore it to the previous value.

To commit the color change, click the `Commit color change` button - this just saves it the runtime, not to file!

To help locate where the theme resource color is used, change the color temporarily to something that will stand out - like Red or Cyan - then revert the color change.

When you have a set of changes you want to keep, click File - Save, or ctrl+S to write the changes back to the source file.

There are separate named colors for most controls. This follows the pattern in the standard WPF controls and allows lots of flexibility for adjusting colors.

However, it is best to re-use the same colors as much as possible for a consistent and uniform appearance.  The last tab on the left of the Theme Editor shows all the colors in the theme, sorted by color.  This gives you an indication of what colors you have re-used, and possibly where else you may want to copy a new color as you modify the theme.

When the checkbox `Synchronize changes to matching colors` is checked, all named solid colors starting with the same color value will be updated together when the select color is changed.  They will be committed together when `Commit color change` button is clicked. Gradient brushes and Drop shadow colors are not included in the synchronized color changes.

Some controls (like the dark group box) have more details (outlines) in them than are visible using the current theme files because colors are set to transparent or to the same color as an adjoining element. Making these details a different color can add these details back into the controls.

When you have something you would like to try in dnGrep, load the theme xaml file from the dnGrep theme directory:
- If dnGrep is installed from the msi to Program Files, then it is %appdata%\dnGREP (C:\Users\_user_\AppData\Roaming\dnGREP).
- If you have a portable version of dnGrep (zip download) or installed outside Program Files, then it is the "Themes" subdirectory where you extracted the dnGrep.exe files.

In either case, you should see the Sunset.xaml file there already.

Start dnGrep, open the Options dialog (F8), and select your new theme in the drop down. With your theme file loaded in dnGrep, you can edit the theme file, and reload it using the `Reload` button in the dnGrep Options dialog, or clicking ctrl+F5.

At the bottom of the theme resource list are checkboxes for the Boolean flags in the theme file - syntax colors, button images and the calendar button. For the syntax coloring in the preview window, you have the option of light coloring or dark coloring based on the colors specified in the syntax *.xshd files.  The dark syntax colors are a color inversion of the light colors in the xshd files. Set this value to true for the preview window with a dark background.

## Import themes from VS Code

The Theme Editor has an import function to use a VS Code theme as a basis for a dnGrep theme. The imported theme will need additional editing, but can be very useful in getting started with a new theme.

**In VS Code:**
1. Select the theme you want to use
1. On the View menu, select Command Palette
1. In the command palette, type 'Generate Color Theme From Current Settings'
1. This will create a new file in the VS Code editor page - save this file with the .jsonc file extension, and give it the name you want for your new theme.

Copy the jsonc file to the directory where you want the new theme to be created.

**In Theme Editor:**
1. On the File menu, choose Import Theme, then VS Code jsonc file.
1. Select your jsonc file.
1. This will create and save a new theme XAML file in the same directory where the jsonc file is located.

## Share Your Theme

If you would like to share your theme, create a discussion topic like [this one](https://github.com/dnGrep/dnGrep/discussions/1052) in dnGrep. I will link them from the dnGrep help pages.

If you are importing a theme, please check the license of the original theme and do not post commercial paid themes that were created by someone else.
