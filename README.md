# SORAMAME image-processing core (C#)

This repository provides a **minimal subset** of the SORAMAME smartphone detector:
the **core image-processing logic** used to extract **event candidates** from CMOS sensor frames.

This repo intentionally excludes application-specific components such as UI, camera control,
network/cloud upload, device identifiers, and deployment settings.

## What this code does

Given an input camera frame (RGBA `Mat`), the pipeline:

1. Converts RGBA → grayscale  
2. Applies binary thresholding  
3. Extracts connected bright clusters via contour detection  
4. Computes each cluster centroid (moments) and area  
5. (Optional) Crops a fixed-size ROI around each candidate for further analysis

The implementation is provided in:
- `SoramameImageProcessingCore.cs`

## Dependencies

This code is written for Unity and uses:

- **Unity** (C#)
- **OpenCVForUnity** (CoreModule, ImgprocModule)

> Note: OpenCVForUnity is a third-party Unity asset. This repository does **not** include
> the plugin binaries; users should obtain it separately.

## Quick usage (Unity / OpenCVForUnity)

```csharp
using OpenCVForUnity.CoreModule;
using Soramame.Core;

int threshValue = 50; // example
var candidates = SoramameDetector.DetectCandidates(rgbaMat, threshValue, minArea: 1.0f);

foreach (var c in candidates)
{
    using (Mat roi = SoramameDetector.CropAround(rgbaMat, c.x, c.y, regionSize: 40))
    {
        // further analysis (e.g., feature extraction, visualization, export)
    }
}


## Input / output

- **Input:** `Mat rgbaMat` (RGBA image matrix)
- **Output:** `List<Candidate>` where each `Candidate` contains:
  - `x, y`: centroid coordinates [pixels]
  - `area`: contour area [pixel^2]

## Sample data and reproducibility

`sample_data/` (to be added) contains a small set of light-shielded sample frames (PNG) for testing.
These files are intended for verifying candidate extraction behavior and output format.

## License

MIT License (see `LICENSE`).
