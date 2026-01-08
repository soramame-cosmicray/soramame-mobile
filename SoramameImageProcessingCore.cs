// SoramameImageProcessingCore.cs
// Minimal, publishable "image-processing only" core extracted from CircleDetectionExample.cs
// Dependencies: OpenCVForUnity (CoreModule, ImgprocModule) and Unity (optional).
// NOTE: This file intentionally excludes any networking (UnityWebRequest), device IDs, UI, and file I/O.
//
// Example usage (Unity):
//   using OpenCVForUnity.CoreModule;
//   using Soramame.Core;
//   var cands = SoramameDetector.DetectCandidates(rgbaMat, threshValue);
//   foreach (var c in cands) {
//       using (var roi = SoramameDetector.CropAround(rgbaMat, c.x, c.y, 40)) {
//           // further analysis / optional PNG encoding...
//       }
//   }

#if !(PLATFORM_LUMIN && !UNITY_EDITOR)
using System;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

namespace Soramame.Core
{
    /// <summary>One detected candidate (cluster centroid + contour area).</summary>
    public struct Candidate
    {
        public int x;
        public int y;
        public float area;

        public Candidate(int x, int y, float area)
        {
            this.x = x;
            this.y = y;
            this.area = area;
        }
    }

    public static class SoramameDetector
    {
        /// <summary>
        /// Detect candidates by: RGBA->Gray, threshold, findContours, moments centroid.
        /// Mirrors the logic in CircleDetectionExample.cs (threshold/findContours/contourArea/moments).
        /// </summary>
        public static List<Candidate> DetectCandidates(Mat rgbaMat, int threshValue, float minArea = 1.0f)
        {
            if (rgbaMat == null || rgbaMat.empty())
                throw new ArgumentException("rgbaMat is null or empty.");

            using (Mat gray = new Mat())
            using (Mat hierarchy = new Mat())
            {
                Imgproc.cvtColor(rgbaMat, gray, Imgproc.COLOR_RGBA2GRAY);
                Imgproc.threshold(gray, gray, threshValue, 255.0, Imgproc.THRESH_BINARY);

                var contours = new List<MatOfPoint>();
                Imgproc.findContours(gray, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

                var outList = new List<Candidate>(contours.Count);
                foreach (var contour in contours)
                {
                    try
                    {
                        float area = (float)Imgproc.contourArea(contour);
                        if (area < minArea) continue;

                        Moments m = Imgproc.moments(contour);
                        if (Math.Abs(m.m00) < 1e-9) continue;

                        int cx = (int)(m.m10 / m.m00);
                        int cy = (int)(m.m01 / m.m00);

                        // Similar sanity check to the original code's guard.
                        if (cx < 0 || cy < 0) continue;

                        outList.Add(new Candidate(cx, cy, area));
                    }
                    finally
                    {
                        contour.Dispose();
                    }
                }
                return outList;
            }
        }

        /// <summary>
        /// Crop a square ROI around (centerX, centerY) with bounds checks.
        /// Mirrors the regionSize/startX/startY clamp logic in CircleDetectionExample.cs.
        /// Returned Mat is a submat view; caller should Dispose().
        /// </summary>
        public static Mat CropAround(Mat rgbaMat, int centerX, int centerY, int regionSize = 40)
        {
            if (rgbaMat == null || rgbaMat.empty())
                throw new ArgumentException("rgbaMat is null or empty.");
            if (regionSize <= 0)
                throw new ArgumentException("regionSize must be > 0.");

            int startX = centerX - regionSize / 2;
            int startY = centerY - regionSize / 2;

            if (startX < 0) startX = 0;
            if (startY < 0) startY = 0;

            if (startX + regionSize > rgbaMat.cols()) startX = Math.Max(0, rgbaMat.cols() - regionSize);
            if (startY + regionSize > rgbaMat.rows()) startY = Math.Max(0, rgbaMat.rows() - regionSize);

            int width = Math.Min(regionSize, rgbaMat.cols() - startX);
            int height = Math.Min(regionSize, rgbaMat.rows() - startY);

            // ensure non-zero sizes
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            return rgbaMat.submat(new Rect(startX, startY, width, height));
        }
    }
}
#endif
