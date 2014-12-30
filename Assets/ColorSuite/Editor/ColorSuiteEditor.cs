//
// Copyright (C) 2014 Keijiro Takahashi
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

[CustomEditor(typeof(ColorSuite)), CanEditMultipleObjects]
public class ColorSuiteEditor : Editor
{
    SerializedProperty propColorTemp;
    SerializedProperty propColorTint;

    SerializedProperty propToneMapping;
    SerializedProperty propExposure;

    SerializedProperty propSaturation;

    SerializedProperty propRCurve;
    SerializedProperty propGCurve;
    SerializedProperty propBCurve;
    SerializedProperty propCCurve;

    SerializedProperty propDitherMode;

    GUIContent labelColorTemp;
    GUIContent labelColorTint;

    void OnEnable()
    {
        propColorTemp = serializedObject.FindProperty("_colorTemp");
        propColorTint = serializedObject.FindProperty("_colorTint");

        propToneMapping = serializedObject.FindProperty("_toneMapping");
        propExposure    = serializedObject.FindProperty("_exposure");

        propSaturation = serializedObject.FindProperty("_saturation");

        propRCurve = serializedObject.FindProperty("_rCurve");
        propGCurve = serializedObject.FindProperty("_gCurve");
        propBCurve = serializedObject.FindProperty("_bCurve");
        propCCurve = serializedObject.FindProperty("_cCurve");

        propDitherMode = serializedObject.FindProperty("_ditherMode");

        labelColorTemp = new GUIContent("Color Temperature");
        labelColorTint = new GUIContent("Tint (green-purple)");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(propToneMapping);
        if (propToneMapping.hasMultipleDifferentValues || propToneMapping.boolValue)
        {
            EditorGUILayout.Slider(propExposure, 0, 5);
            if (QualitySettings.activeColorSpace != ColorSpace.Linear)
                EditorGUILayout.HelpBox("Linear space lighting should be enabled for tone mapping.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        EditorGUILayout.Slider(propColorTemp, -1.0f, 1.0f, labelColorTemp);
        EditorGUILayout.Slider(propColorTint, -1.0f, 1.0f, labelColorTint);

        EditorGUILayout.Space();

        EditorGUILayout.Slider(propSaturation, 0, 2);
        
        EditorGUILayout.LabelField("Curves (R, G, B, Combined)");
        EditorGUILayout.BeginHorizontal();
        var doubleHeight = GUILayout.Height(EditorGUIUtility.singleLineHeight * 2);
        EditorGUILayout.PropertyField(propRCurve, GUIContent.none, doubleHeight);
        EditorGUILayout.PropertyField(propGCurve, GUIContent.none, doubleHeight);
        EditorGUILayout.PropertyField(propBCurve, GUIContent.none, doubleHeight);
        EditorGUILayout.PropertyField(propCCurve, GUIContent.none, doubleHeight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(propDitherMode);

        if (targets.Length == 1)
            EditorGUILayout.HelpBox(ShowInfo(), MessageType.None);

        serializedObject.ApplyModifiedProperties();
    }

    string ShowInfo()
    {
        var cs = target as ColorSuite;
        var text = "Current Pipeline: [Texture Fetch] - ";
        var linear = (QualitySettings.activeColorSpace == ColorSpace.Linear);

        if (cs.colorTemp != 0.0f || cs.colorTint != 0.0f)
            text += "[RGB to LMS] - [White Balance] - [LMS to RGB] - ";

        if (cs.toneMapping)
            text += "[Tone Mapping (Hable)] - ";
        else if (linear)
            text += "[Gamma Compression] - ";

        text += "[Saturation] - ";
        text += "[Curves] - ";

        if (cs.ditherMode == ColorSuite.DitherMode.Ordered)
            text += "[Dither (Ordered)] - ";
        else if (cs.ditherMode == ColorSuite.DitherMode.Triangular)
            text += "[Dither (Triangular)] - ";

        if (linear)
            text += "[Gamma Expansion] - ";

        return text + "[Output]";
    }
}
