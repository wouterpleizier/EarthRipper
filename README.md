# EarthRipper
Tool for ripping high-resolution imagery and heightmaps from Google Earth Pro.

## Disclaimer and limitations
- **This tool is not affiliated with Google and is only intended for personal use. Images captured with the tool may be subject to copyright and should not be considered a valid replacement for DEM data.**
- Only supports version 7.3.6 of the 32-bit Windows version of Google Earth Pro (the desktop application, not the web-based Google Earth!). Other Windows versions may also work to some degree but have not been tested. [Installers for specific versions can be found here.](https://support.google.com/earth/answer/168344#zippy=%2Cdownload-a-google-earth-pro-direct-installer) 
- Captures are currently only supported in OpenGL mode.
- Captures do not use any kind of flattened map projection and will appear in the same way as the view is rendered in Google Earth Pro. This means that the more you zoom out, the more the outer edges of the capture will be distorted by the Earth's curvature, and the worse the accuracy of the estimated elevation extents (since points further from the capture's center are also further from the camera's near rendering plane, the tool perceives them to be lower in elevation than they actually are).
- Only imagery and heightmaps are captured. If you want to capture 3D geometry, use Élie Michel's method and tools described [here.](https://github.com/eliemichel/MapsModelsImporter)

## Features
### Capture color and height
The current view can be captured at any time by pressing a hotkey, including in areas with photogrammetry data (3D buildings). The following files are then written to disk:
- A 24-bit PNG of the view as it appears in Google Earth Pro, without any labels or watermarks.
- A 16-bit grayscale PNG of the corresponding heightmap, remapped to utilize the full 16-bit range so that the lowest/farthest point is fully black and the highest/nearest point is fully white.
- A text file containing the estimated extents and the coordinates of each corner.

Here's an example capture of Lower Manhattan:
| Color | Height |
| ----- | ------ |
| ![manhattan_color](https://user-images.githubusercontent.com/8166218/219864749-62d7b551-a2e7-47ec-883e-636b380cd2a8.jpg) | ![manhattan_height](https://user-images.githubusercontent.com/8166218/219864763-3eb10bc1-b869-4fc5-bc84-72430484f44e.jpg) |

And the corresponding text file:
```
Elevation in meters:     -15.0148526095327 to 303.565817545423
Size in meters:          1814.15867818412 x 1133.5065894551
Top-left coordinate:     lat 40.7129788857998, lon -74.0185080834957
Top-right coordinate:    lat 40.7055100737699, lon -73.9993935907769
Bottom-left coordinate:  lat 40.7039254857158, lon -74.0246634728241
Bottom-right coordinate: lat 40.6964572891588, lon -74.0055492680982
```

Note: min/max elevation values are estimates that become less accurate as you zoom out (see also [limitations](#disclaimer-and-limitations)). The size is calculated using the distance from the top-left coordinate to the top-right and bottom-left coordinates.

### Render scale
The tool allows you to increase the rendering resolution and downsample it to the current window size. The increased resolution makes Google Earth Pro download and render images/models with a higher level of detail, which is especially beneficial when making captures.

### Extras
Hotkeys are provided for hiding interface watermarks/overlays and for enabling an orthographic camera. Not necessary for capturing, but it's fun to get a SimCity-style view on real places. (The camera controls may be a bit wonky, though)

![portland_ortho](https://user-images.githubusercontent.com/8166218/219865058-fa1054e0-696f-4739-9687-d668155b3e67.jpg)

## Installation
1. Download the latest version from https://github.com/wouterpleizier/EarthRipper/releases/latest
2. Extract the contents of the zip file to a new folder somewhere

## Usage
### Capture
To capture some imagery and a heightmap, follow these steps:

1. Launch the 32-bit version of Google Earth Pro (preferably version 7.3.6, as other versions have not been tested).
2. Go to `Tools` -> `Options...`.
3. Set `Elevation Exaggeration` to 0.01, to minimize the effects of perspective and occlusion when capturing.
4. Turn off `Use photorealistic atmosphere rendering` if you don't want atmospheric haze to appear in your capture.
5. If `Graphics Mode` is set to DirectX, change it to OpenGL and then restart the application.
6. Launch EarthRipper.exe and verify that the message `Hooks installed, awaiting input` is shown in the console window.
7. In Google Earth Pro, navigate to the area you want to capture.
8. Press the `U` key to tilt the camera straight down.
9. To change the rendering resolution, use the `[` and `]` keys. The new scale/resolution will be shown in the console window. When using a higher resolution, it can take some time before the desired level of detail is fully loaded. Keep an eye on your network traffic in Task Manager if you want to see when it's complete.
10. Press `C` to capture the current view. The console window will show some messages about the capture's progress, followed by the path where the resulting files are saved once it's complete.

If the capture appears to hang during the `Sampling coordinates and elevation` stage, try moving the mouse cursor a little inside the map view. If an error occurs during the capture, you may need to change the camera's position and/or zoom level slightly and then try again.

Note that setting `Elevation Exaggeration` to 0.01 and pointing the camera straight down is not strictly necessary when making a capture, but it's recommended to do so when you want a top-down heightmap. If you don't need this, you can skip those steps.

### Hotkeys
The following hotkeys are available inside Google Earth Pro when the tool is running alongside it. Make sure the map view has focus (click somewhere inside it to make sure) or the hotkeys won't work.
| Key         | Command                                               |
| ----------- | ----------------------------------------------------- |
| `C`         | Capture current view                                  |
| `H`         | Toggle interface/overlay display                      |
| `O`         | Toggle orthographic camera (does not affect captures) |
| `[` and `]` | Decrease/increase rendering resolution                |

### Command line options
By default, the tool captures color, height and metadata and saves these to a subfolder named Output inside the directory where EarthRipper.exe is located. If you want to change this, use the following command line options:

| Option             | Effect |
| ------------------ | ------ |
| `-output <path>`   | Set capture output directory to `<path>`. |
| `-capture <flags>` | Specify which things should be captured and saved to disk. `<flags>` is a comma-separated list of one or more of these values: `color`, `height`, `metadata` |

Example: `EarthRipper.exe -output C:\EarthRipperCaptures -capture color,height`

This would only capture the color and height (no metadata) and save them to the directory `C:\EarthRipperCaptures`.

## How it works
The tool uses [EasyHook](https://github.com/EasyHook/EasyHook) to inject itself into the Google Earth Pro process and then intercept certain function calls of its GUI and rendering libraries.

When a capture is requested, specific draw calls are suppressed for a single frame to prevent anything other than the map geometry being rendered. The contents of the color and depth buffer are then read back so they can be saved as images. Mouse events are also simulated at specific screen coordinates in order to find the following data:
- The latitude and longitude at each of the screen's corners
- The elevation at the screen coordinate where the depth buffer has the lowest value
- The elevation at the screen coordinate where the depth buffer has the highest value
