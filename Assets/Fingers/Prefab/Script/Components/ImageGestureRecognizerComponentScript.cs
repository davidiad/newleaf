//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace DigitalRubyShared
{
    [AddComponentMenu("Fingers Gestures/Gesture/Image Recognition Gesture", 8)]
    public class ImageGestureRecognizerComponentScript : GestureRecognizerComponentScript<ImageGestureRecognizer>
    {
        [Header("Image gesture properties")]
        [Tooltip("The maximum number of distinct paths for each image. Gesture will reset when max path count is hit.")]
        [Range(1, 5)]
        public int MaximumPathCount = 1;

        [Tooltip("The amount that the path must change direction (in radians) to count as a new direction (0.39 is 1.8 of PI).")]
        [Range(0.01f, 1.0f)]
        public float DirectionTolerance = 0.3f;

        [Tooltip("The distance in units that the touch must move before the gesture begins.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.4f;

        [Tooltip("Minimum difference beteen points in units to count as a new point.")]
        [Range(0.01f, 1.0f)]
        public float MinimumDistanceBetweenPointsUnits = 0.1f;

        [Tooltip("The amount that the gesture image must match an image from the set to count as a match (0 - 1).")]
        [Range(0.01f, 1.0f)]
        public float SimilarityMinimum = 0.8f;

        [Tooltip("The minimum number of points before the gesture will recognize.")]
        [Range(2, 10)]
        public int MinimumPointsToRecognize = 2;

        [Tooltip("The images that should be compared against to find a match. The values are a ulong which match the bits of each generated image. See DemoSceneImage & DemoScriptImage.cs for an example.")]
        public List<ImageGestureRecognizerComponentScriptImageEntry> GestureImages;

        /// <summary>
        /// Allows looking up a key from a matched image
        /// </summary>
        public Dictionary<ImageGestureImage, string> GestureImagesToKey { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.MaximumPathCount = MaximumPathCount;
            Gesture.DirectionTolerance = DirectionTolerance;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumDistanceBetweenPointsUnits = MinimumDistanceBetweenPointsUnits;
            Gesture.SimilarityMinimum = SimilarityMinimum;
            Gesture.MinimumPointsToRecognize = MinimumPointsToRecognize;
            Gesture.GestureImages = new List<ImageGestureImage>();
            GestureImagesToKey = new Dictionary<ImageGestureImage, string>();
            foreach (ImageGestureRecognizerComponentScriptImageEntry img in GestureImages)
            {
                List<ulong> rows = new List<ulong>();
                foreach (string ulongs in img.Images.Split('\n'))
                {
                    string trimmed = ulongs.Trim();
                    try
                    {
                        // trim out scripting code
                        trimmed = Regex.Replace(trimmed, @" *?\{ new ImageGestureImage\(new ulong\[\] *?\{ *?", string.Empty);
                        trimmed = Regex.Replace(trimmed, @" *?\}.+$", string.Empty);

                        if (trimmed.Length != 0)
                        {
                            string[] rowStrings = trimmed.Trim().Split(',');
                            foreach (string rowString in rowStrings)
                            {
                                string _rowString = rowString.Trim();
                                if (_rowString.StartsWith("0x"))
                                {
                                    _rowString = _rowString.Substring(2);
                                }
                                rows.Add(ulong.Parse(_rowString, System.Globalization.NumberStyles.HexNumber));
                            }
                            ImageGestureImage image = new ImageGestureImage(rows.ToArray(), ImageGestureRecognizer.ImageColumns, img.ScorePadding);
                            image.Name = img.Key;
                            Gesture.GestureImages.Add(image);
                            GestureImagesToKey[image] = img.Key;
                            rows.Clear();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogFormat("Error parsing image gesture image: {0} - {1}", trimmed, ex);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct ImageGestureRecognizerComponentScriptImageEntry
    {
        [Tooltip("Key")]
        public string Key;

        [Tooltip("Score padding, makes it easier to match")]
        [Range(0.0f, 0.5f)]
        public float ScorePadding;

        [TextArea(1, 8)]
        [Tooltip("Comma separated list of hex format ulong for each row, separated by newlines.")]
        public string Images;
    }
}
