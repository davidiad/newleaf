using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ara{
    [CustomEditor(typeof (AraTrail))]
    [CanEditMultipleObjects]
    internal class AraTrailEditor : Editor
    {
   

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            Editor.DrawPropertiesExcluding(serializedObject,"m_Script");
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}

