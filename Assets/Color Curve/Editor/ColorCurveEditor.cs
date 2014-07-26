//
// Copyright (C) 2013, 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ColorCurve)), CanEditMultipleObjects]
public class ColorCurveEditor : Editor
{
    SerializedProperty propRCurve;
    SerializedProperty propGCurve;
    SerializedProperty propBCurve;
    SerializedProperty propLCurve;
    SerializedProperty propBrightness;
    SerializedProperty propSaturation;
    SerializedProperty propContrast;

    void OnEnable()
    {
        propRCurve     = serializedObject.FindProperty("rCurve");
        propGCurve     = serializedObject.FindProperty("gCurve");
        propBCurve     = serializedObject.FindProperty("bCurve");
        propLCurve     = serializedObject.FindProperty("lCurve");
        propBrightness = serializedObject.FindProperty("brightness");
        propSaturation = serializedObject.FindProperty("saturation");
        propContrast   = serializedObject.FindProperty("contrast");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Curves (Red, Green, Blue, Luminosity)");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(propRCurve, GUIContent.none);
        EditorGUILayout.PropertyField(propGCurve, GUIContent.none);
        EditorGUILayout.PropertyField(propBCurve, GUIContent.none);
        EditorGUILayout.PropertyField(propLCurve, GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Slider(propBrightness, -1, 1);
        EditorGUILayout.Slider(propSaturation, 0, 3);
        EditorGUILayout.Slider(propContrast, -4, 4);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            foreach (var t in targets)
                (t as ColorCurve).SendMessage("UpdateParameters");
    }
}
