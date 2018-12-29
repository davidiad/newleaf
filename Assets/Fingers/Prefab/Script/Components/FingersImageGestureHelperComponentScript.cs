//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// A little higher level script that includes an option to allow line renderers to draw the path for you.
    /// You can leave the line renderers and match text as null if you will be drawing your own lines
    /// and text for when there is a match.
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Image Recognizer Helper", 8)]
    public class FingersImageGestureHelperComponentScript : ImageGestureRecognizerComponentScript
    {
        [Tooltip("The line renderers to show drawn lines or NULL if you are drawing your own lines. Make sure to add " +
            "enough line renderers to match the max path count for your image gesture recognizer.")]
        public LineRenderer[] LineRenderers;

        [Tooltip("Label to show match in, null for none")]
        public UnityEngine.UI.Text MatchText;

        [Tooltip("Executed whenever lines need updating. Use the gesture property FocusX and FocusY to determine the current point.")]
        public System.EventHandler LinesUpdated;

        [Tooltip("Executed whenever lines are cleared.")]
        public System.EventHandler LinesCleared;

        private Color savedColor;
        private ImageGestureImage matchedImage;
        private const float lineAnimationDurationSeconds = 1.0f;
        private float animationTime;

        private void ClearLineRenderers()
        {
            foreach (LineRenderer lineRenderer in LineRenderers)
            {
                lineRenderer.positionCount = 0;
            }
            if (LinesCleared != null)
            {
                LinesCleared.Invoke(this, System.EventArgs.Empty);
            }
        }

        private void Gesture_MaximumPathCountExceeded(object sender, System.EventArgs e)
        {
            ClearLineRenderers();
        }

        private void UpdateLines()
        {
            int idx = Gesture.PathCount - 1;
            if (idx >= 0 && idx < LineRenderers.Length)
            {
                Vector3 worldPos = new Vector3(Gesture.FocusX, Gesture.FocusY, 0.0f);
                worldPos = Camera.main.ScreenToWorldPoint(worldPos);
                worldPos.z = 0.0f;
                LineRenderers[idx].positionCount++;
                LineRenderers[idx].SetPosition(LineRenderers[idx].positionCount - 1, worldPos);
            }
            if (LinesUpdated != null)
            {
                LinesUpdated.Invoke(this, System.EventArgs.Empty);
            }
        }

        private void BeginAnimateOutLines()
        {
            if (LineRenderers.Length != 0)
            {
                savedColor = LineRenderers[0].startColor;
                animationTime = lineAnimationDurationSeconds;

                // remove gesture until animation finishes
                Gesture.Enabled = false;
            }
        }

        private void UpdateLineAnimation()
        {
            if (animationTime > 0.0f)
            {
                animationTime -= Time.deltaTime;
                if (animationTime <= 0.0f)
                {
                    ClearLineRenderers();
                    foreach (LineRenderer lineRenderer in LineRenderers)
                    {
                        lineRenderer.startColor = lineRenderer.endColor = savedColor;
                    }
                    Gesture.Enabled = true;
                }
                else
                {
                    foreach (LineRenderer lineRenderer in LineRenderers)
                    {
                        float lerp = animationTime / lineAnimationDurationSeconds;
                        Color color = Color.Lerp(Color.clear, savedColor, lerp);
                        lineRenderer.startColor = lineRenderer.endColor = color;
                    }
                }
            }
        }

        public ImageGestureImage CheckForImageMatch()
        {
            if (matchedImage == null)
            {
                if (MatchText != null)
                {
                    MatchText.text = "No match found!";
                }
            }
            else
            {
                if (MatchText != null)
                {
                    MatchText.text = "You drew a " + matchedImage.Name;
                }

                // image gesture must be manually reset when a shape is recognized
                Gesture.Reset();

                // clear out lines with animation
                BeginAnimateOutLines();
            }

            return matchedImage;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // if we exceed the max path count, we clear all lines immediately
            Gesture.MaximumPathCountExceeded += Gesture_MaximumPathCountExceeded;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            UpdateLineAnimation();
        }

        public void GestureCallback(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began)
            {
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                // save off the matched image, the gesture may reset if max path count has been reached
                matchedImage = Gesture.MatchedGestureImage;
            }
            else
            {
                // don't update lines
                return;
            }
            UpdateLines();
        }

        /// <summary>
        /// Reset state, clear all lines
        /// </summary>
        public void Reset()
        {
            ClearLineRenderers();
            Gesture.Reset();
        }
    }
}
