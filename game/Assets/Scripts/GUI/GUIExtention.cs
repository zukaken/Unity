using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GUIExtention
{

    public static Color LerpColor(DateTime src, DateTime dst, Color srcColor, Color dstColor)
    {
        return Color.Lerp(srcColor, dstColor, Mathf.Clamp01((float)(src - dst).TotalSeconds));
    }
}

public class ColorScope : System.IDisposable
{
    Color before;
    public ColorScope(Color color)
    {
        before = GUI.color;
        GUI.color = color;
    }

    public void Dispose()
    {
        GUI.color = before;
    }
}

