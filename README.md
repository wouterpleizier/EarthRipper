# EarthRipper
Tool for ripping high-resolution imagery and heightmaps from Google Earth Pro.

## Disclaimer and limitations
- **This tool is not affiliated with Google and is only intended for personal use. Images captured with the tool may be subject to copyright and should not be considered a valid replacement for DEM data.**
- Supports version 7.3.6 of Google Earth Pro for Windows only (the desktop application, not the web-based Google Earth!). Other Windows versions may also work to some degree but have not been tested. [Installers for specific versions can be found here.](https://support.google.com/earth/answer/168344#zippy=%2Cdownload-a-google-earth-pro-direct-installer)
- Supports both 32-bit and 64-bit, but 64-bit is recommended (especially for high-resolution captures).
- Captures use a spherical projection (same as how Google Earth renders normally, except with nullified elevation). This means that the more you zoom out, the more the outer edges of the capture will be distorted by the Earth's curvature.
- Captures can only be saved as images (JPG, 24-bit color PNG or 16-bit grayscale PNG) with a resolution of up to 8192x8192 pixels. If you need 3D geometry, try one of the following methods instead:
	- [Google's official Map Tiles API](https://developers.google.com/maps/documentation/tile), optionally via [Blosm for Blender](https://github.com/vvoovv/blosm/wiki/Import-of-Google-3D-Cities)
	- [Élie Michel's MapsModelImporter for Blender](https://github.com/eliemichel/MapsModelsImporter)

## Features
### Custom visualization/capturing
EarthRipper includes the following render presets that change the way map data is displayed and/or saved in Google Earth Pro:

| Preset | Description                                                                                                                                     |
| :----: | :---------------------------------------------------------------------------------------------------------------------------------------------- |
|   ![Tokyo Tower area captured with Default render preset](https://github.com/wouterpleizier/EarthRipper/assets/8166218/9da6b302-496a-4db9-8791-cfdbedb8ae6c) **`Default`**   | Unmodified behavior. |  |
|     ![Tokyo Tower area captured with Color render preset](https://github.com/wouterpleizier/EarthRipper/assets/8166218/7729b8b2-0b02-45ce-9788-b0624a25d529) **`Color`**     | Plain color textures without lighting or atmospheric effects. |
| ![Tokyo Tower area captured with Elevation render preset](https://github.com/wouterpleizier/EarthRipper/assets/8166218/f6c2dbbf-e594-4016-bd87-1eaa63c9033c) **`Elevation`** | 16-bit height/elevation data. By default, values range between -1 km and 9 km relative to sea level, resulting in ~15 cm of accuracy. [A different range can be specified if necessary.](#adjust-minmax-range-of-elevation-captures) <br> <br> (Example image is contrast boosted for clarity) |
|     ![Tokyo Tower area captured with Tiles render preset](https://github.com/wouterpleizier/EarthRipper/assets/8166218/52651367-8bff-403e-a119-852e35aabd07) **`Tiles`**     | Semi-random color per tile. Mainly intended as a utility to be used alongside other render presets, e.g. for aligning adjacent captures, correcting distortion or visualizing tile density/quality. |

Aside from `Default`, each render preset flattens geometry to sea level, effectively turning the Earth into a perfect sphere. This allows captures to be used without having to account for occlusion or differences in terrain/building/camera height.

[Custom render preset can also be made, allowing for further possibilities.](#customize-render-presets)

### Orthographic rendering
An orthographic camera can be toggled via a menu option. This usually isn't necessary for capturing, but it's fun to get a SimCity-style view on real places. (The camera controls may be a bit wonky, though)

![Orthographic view of Portland](https://user-images.githubusercontent.com/8166218/219865058-fa1054e0-696f-4739-9687-d668155b3e67.jpg)

## Basic usage
### Getting started
1. Launch Google Earth Pro (preferably version 7.3.6 as other versions have not been tested).
2. Open the `View` menu and set these options as follows:
	- **Atmosphere**: `on`
	- **Sun**: `off`
3. Open the `Tools` menu, click `Options...` and set these options as follows:
	- **Elevation Exaggeration**: `1`
	- **Use photorealistic atmosphere rendering (EXPERIMENTAL)**: `on`
4. Close Google Earth Pro.
5. Download the latest version of EarthRipper from https://github.com/wouterpleizier/EarthRipper/releases/latest
6. Extract the contents of the zip file to a new folder somewhere, then launch `EarthRipper.exe`.
7. Google Earth Pro should now launch automatically, and EarthRipper will inject itself into the process when ready. A new menu titled `EarthRipper` should then appear in the upper menu bar of Google Earth Pro, next to the `Help` menu: <br> <br> ![menu](https://github.com/wouterpleizier/EarthRipper/assets/8166218/07018f66-ce9e-4137-9f54-b21c00dfa0af)

If something looks wrong or doesn't work as expected (e.g. Google Earth Pro doesn't launch automatically or the map view remains black), launch Google Earth Pro manually, wait for it to finish loading and then run `EarthRipper.exe`.

### Visualize and capture using a render preset
1. Under the `EarthRipper` menu, go to `Render presets` and choose one of the available render presets: <br> <br> ![renderpresets](https://github.com/wouterpleizier/EarthRipper/assets/8166218/a0c8008e-061b-47e2-99f7-403ffb98ce71)

2. If the render preset doesn't take effect immediately, move the mouse anywhere inside the map view. (Note: to see the effects of the `Elevation` render preset clearly, you'll probably want to view an area with mountains or tall buildings)

3. To prepare for capture, press the `U` key to tilt the camera straight down. (Not strictly necessary, but recommended when capturing texture/height maps)

4. Open the image saving panel using the `Save Image` button in the toolbar (third one from the right) or `File` -> `Save` -> `Save Image...` (`Ctrl+Alt+S`).

5. Choose the desired resolution and then click `Save Image...` to pick the output file and folder. <br> <br> ![saveimage](https://github.com/wouterpleizier/EarthRipper/assets/8166218/5a6cd29a-e043-41d0-aaa7-8569ec975e5e)

6. Wait until the image has finished saving. This may take a while when a high output resolution is used, as higher quality map/tile data needs to be loaded. Also note that the chosen render preset may force a different file format and extension to be used than the one you've specified (JPG for `Color`, PNG for `Elevation`).

### Capture using multiple render presets
When saving an image (see the previous section), the names of render presets can be appended to the file name in order to perform multiple sequential captures of the same map view. For example, using the file name `MyCoolImage.default.color.elevation.tiles.jpg` will result in the following images being saved:
```
MyCoolImage.jpg
MyCoolImage.color.jpg
MyCoolImage.elevation.png
MyCoolImage.tiles.png
```
This is particularly useful for high-resolution captures, as it allows map/tile data from the initial capture to be reused for subsequent captures.

Additional notes:
- Render preset names are case insensitive.
- The desired render preset names should be placed between the file name and the file extension, and must be separated by periods (`.`).
- During capture, render presets specified by file name will override the render preset that's currently selected via the `EarthRipper` -> `Render presets` menu.
- A single render preset name may also be specified, e.g. `MyCoolImage.elevation.png`. This allows you to capture using that specific render preset, while navigating and viewing the map using a different one.
- The final file name and file format/extension may be overridden by render presets. For example, the `Elevation` render preset appends `.elevation` to the chosen file name, and forces the image to be written as a 16-bit grayscale PNG regardless of which format/extension was chosen in the file picker dialog.
- The location in which images are saved may also be overridden by render presets. This does not happen by default with any of the included render presets, but [they can be configured as such.](#customize-render-presets)

## Advanced
### Adjust min/max range of elevation captures
By default, elevation captures can range between -1 km and 9 km relative to sea level, resulting in ~15 cm of accuracy when saved as a 16-bit grayscale PNG. If you need a different range, open `RenderPresets\Elevation\earth_atmosphere_ground_sun_on.glsl` with a text/code editor and change these values:

```glsl
const float minElevationInKm = -1.0;
const float maxElevationInKm = 9.0;
```

### Customize render presets
Render presets are defined inside EarthRipper's `RenderPresets` folder. Each subfolder represents a render preset of the same name, and should contain a `Settings.json` file and any number of shader overrides. This allows for customization of existing render presets and the creation of new ones.

#### Settings.json
Controls the overall behavior of the render preset. Ommitted properties fall back to their default values and (if applicable) result in the same behavior as when Google Earth Pro runs without EarthRipper.

| Property  | Description | Allowed values | Default value |
| --------- | ----------- | -------------- | ------------- |
| `ShowImageOverlays` | Enables or disables overlays/elements when saving images (Google Earth logo, copyright notices, legend, scale, etc). | `false` <br> `true` | `true` |
| `DefaultShaderHandling` | Determines whether non-overridden shaders should render. | `"AllowAll"` <br> `"AllowSpecified"` <br> `"BlockAll"` <br> `"BlockSpecified"` | `"AllowAll"` |
| `DefaultShaders` | The shaders that should or shouldn't render when `DefaultShaderHandling` is set to `"AllowSpecified"` or `"BlockSpecified"`. See [Shader overrides](#shader-overrides) for a list of known shader names. | String array with names of built-in shaders, e.g. `[ "stars", "watersurface" ]` | Nothing |
| `ClearColor` | Overrides the background / clear color. If unset or invalid, the built-in color is used (black or grey, depending on camera angle and height) | Number array with normalized RGB values, e.g. `[1.0, 0.0, 0.0]` for bright red | Nothing |
| `OutputPath` | Determines the final name and location of captures. Recommended to make this unique for each render preset, so that render presets do not overwrite each other's output during sequential captures. Use `{Directory}` and/or `{FileName}` to reference the directory and/or file name that was chosen in the file picker dialog. | Any string that resolves to a valid relative or absolute path | `"{Directory}/{FileName}"` |
| `OutputFormat` | Image format for captures. `"UserSpecified"` indicates that the same format should be used that was chosen in the file picker dialog (24-bit color JPG/PNG). | `"UserSpecified"` <br> `"JPG"` <br> `"PNG"` <br> `"PNG_Gray16"` | `"UserSpecified"` |
| `OutputQuality` | Image quality for JPG captures. Ignored for other formats. The default value matches the quality level that Google Earth Pro normally uses for JPG images. | A number between `0` and `100` | `95` |
| `OutputScaleFactor` | Image scaling factor, which can be useful for supersampled captures. For example: capturing at 8192x8192 to load higher quality map/tile data, but using a scaling factor of `0.25` to output a 2048x2048 image. | A number greater than `0.0` and lower than or equal to `1.0`. | `1.0` |
| `OutputMaxWidth` <br> `OutputMaxHeight` | Maximum image width and/or height in pixels, which can be useful for supersampled captures (see above). Images are scaled proportionally such that neither the width nor height exceed the specified maximum. | A number greater than `0` and lower than or equal to `8192`. | `8192` |

#### Shader overrides
If a render preset folder contains `.glsl` files, they will be used to override shaders with the corresponding names. Here's a (likely incomplete) list of known shader names:
```
atmosphere_ground_sun_off
atmosphere_ground_sun_off_model
atmosphere_ground_sun_off_overlay
atmosphere_ground_sun_off_simple
atmosphere_sky_sun_off
default_program
earth_atmosphere_ground_sun_on
earth_atmosphere_ground_sun_on_model
earth_atmosphere_ground_sun_on_overlay
earth_atmosphere_sky_sun_on
earth_atmosphere_table
earth_atmosphere_water_sun_on
fade_no_atmosphere
fade_sun_no_atmosphere
ground_overlay_no_atmosphere
hammer_aitoff
precipitation_double_cone
stars
watersurface
```
Different shaders are used for different purposes. The `Color`, `Elevation` and `Tiles` render presets all override the `earth_atmosphere_ground_sun_on` shader because it has the necessary attributes and uniforms for determining and/or nullifying elevation. If you want to write a shader override from scratch, use these existing overrides as a reference. I also recommend examining the original shader source files in your `Google Earth Pro\client\shaders` folder and using [apitrace](https://apitrace.github.io/) or a similar tool to see what's happening under the hood.

The vertex and fragment shader source are both expected to be contained in the same `.glsl` file. Use the `GE_VERTEX_SHADER` and `GE_PIXEL_SHADER` constants to differentiate between the two.

When Google Earth Pro is running in DirectX mode, GLSL code is automatically transpiled to HLSL (same as with the built-in shaders). Be aware that certain language functions/features may work in one mode, but fail to compile in the other.

For render presets that output 16-bit grayscale PNGs, shader overrides should pack their data into the red and green channels of `gl_FragColor`. You can optionally use the `EARTHRIPPER_CAPTURE` constant to do this only when saving images, and to return a more user-friendly color in other cases. Check the `Elevation` render preset for an example.
