using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Kiz_GUIUtility
{
    private static GUIStyle toolbarSearchField;
    private static GUIStyle toolbarSearchFieldCancelButton;
    private static GUIStyle toolbarSearchFieldCancelButtonEmpty;
    private static int searchFieldHash = "SearchBoxTestWindow_SearchField".GetHashCode();
    private string text = "";
    private static readonly Color32 s_SelectedColorAlpha = new Color32(45, 120, 200, 200);
    private static readonly Color32 s_SelectedColor = new Color32(45, 120, 200, 255);
    private static readonly Color32 s_RectToolColor = new Color32(120, 200, 120, 150);

    public static Color SelectedColorAlpha { get { return s_SelectedColorAlpha; } }
    public static Color SelectedColor { get { return s_SelectedColor; } }
    public static Color RectToolColor { get { return s_RectToolColor; } }

    public static void DrawSelectRect(Rect rect)
    {
        GUI.color = SelectedColorAlpha;
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;
    }
    
    public static string SearchField(Rect position, string text)
    {
        if (toolbarSearchField == null) { toolbarSearchField = GetStyle("ToolbarSeachTextField"); }
        if (toolbarSearchFieldCancelButton == null) { toolbarSearchFieldCancelButton = GetStyle("ToolbarSeachCancelButton"); }
        if (toolbarSearchFieldCancelButtonEmpty == null) { toolbarSearchFieldCancelButtonEmpty = GetStyle("ToolbarSeachCancelButtonEmpty"); }

        Rect position2 = position;
        position2.width -= 14f;
        Rect position3 = position;
        position3.x += position2.width;
        position3.width = 14f;

        text = EditorGUI.TextField(position2, text, toolbarSearchField);
        
        if (string.IsNullOrEmpty(text))
        {
            GUI.Button(position3, GUIContent.none, toolbarSearchFieldCancelButtonEmpty);
        }
        else
        {
            if (GUI.Button(position3, GUIContent.none, toolbarSearchFieldCancelButton))
            {
                text = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
        }

        return text;
    }

    static private GUIStyle GetStyle(string styleName)
    {
        GUIStyle gUIStyle = GUI.skin.FindStyle(styleName);
        if (gUIStyle == null)
        {
            gUIStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }
        if (gUIStyle == null)
        {
            Debug.LogError("Missing built-in guistyle " + styleName);
            gUIStyle = new GUIStyle();
        }
        return gUIStyle;
    }

}