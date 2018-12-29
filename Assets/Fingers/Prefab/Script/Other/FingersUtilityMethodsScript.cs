using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Utility methods
    /// </summary>
    public static class FingersUtility
    {
        //****************************************************************************************************
        //  static function DrawLine(rect : Rect) : void
        //  static function DrawLine(rect : Rect, color : Color) : void
        //  static function DrawLine(rect : Rect, width : float) : void
        //  static function DrawLine(rect : Rect, color : Color, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
        //****************************************************************************************************

        public static void DrawLine(this Image img, Rect rect) { DrawLine(img, rect, Color.white, 1.0f); }
        public static void DrawLine(this Image img, Rect rect, Color color) { DrawLine(img, rect, color, 1.0f); }
        public static void DrawLine(this Image img, Rect rect, float width) { DrawLine(img, rect, Color.white, width); }
        public static void DrawLine(this Image img, Rect rect, Color color, float width) { DrawLine(img, new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB) { DrawLine(img, pointA, pointB, Color.white, 1.0f); }
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, Color color) { DrawLine(img, pointA, pointB, color, 1.0f); }
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, float width) { DrawLine(img, pointA, pointB, Color.white, width); }
        public static void DrawLine(this Image img, Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            RectTransform imageRectTransform = img.rectTransform;
            img.material.color = color;
            Vector3 differenceVector = pointB - pointA;
            float distance = differenceVector.magnitude;
            if (img.canvas.scaleFactor > 0.0f)
            {
                distance /= img.canvas.scaleFactor;
            }
            imageRectTransform.sizeDelta = new Vector2(distance, width);
            imageRectTransform.pivot = new Vector2(0.0f, 0.5f);
            imageRectTransform.position = pointA;
            float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
            imageRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
