using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using GUIScrollTree = KIZ.GUITree<ScrollTree>;

public class Kiz_GenericMenu : PopupWindowContent
{
    public delegate void OnSelect(object userData);

    GUIScrollTreeDrawer mDrawer = new GUIScrollTreeDrawer();
    GUIScrollTree mScrollTree = new GUIScrollTree("Root");

    OnSelect onSelect;

    public void Add(string[] splitedValues, object userData)
    {
        var leaf = GUIScrollTree.Add(mScrollTree, splitedValues, 0);
        leaf.GenericValue.mUserData = userData;
    }

    public void Show(Rect rect, OnSelect callback)
    {
        onSelect = callback;
        mDrawer.onSelect += OnItemSelect;
        GUIScrollTree.Sort(mScrollTree, (x, y) => string.Compare(x.Name, y.Name));
        PopupWindow.Show(rect, this);
    }
    string t;

    public override void OnGUI(Rect rect)
    {
        Rect searchBoxRect = new Rect(rect.x, rect.y, rect.width, 36);
        searchBoxRect = new RectOffset(10, 10, 10, 10).Remove(searchBoxRect);
        t = Kiz_GUIUtility.SearchField(searchBoxRect, t);

        Rect viewRect = rect;
        viewRect.y += 32;
        viewRect.yMax = rect.yMax;
        mDrawer.Draw(viewRect, mScrollTree);
        editorWindow.Repaint();
    }
    public override Vector2 GetWindowSize()
    {
        return new Vector2(240, 300);
    }
    public void OnItemSelect(GUIScrollTree node)
    {
        onSelect.Invoke(node.GenericValue.mUserData);
        editorWindow.Close();
    }
}

public class ScrollTree
{
    public Vector2 mVerticalScrollPos;
    public object mUserData;
}


class GUIScrollTreeDrawer
{
    public delegate void OnSelect(GUIScrollTree node);

    List<int> mSelectedIndex = new List<int>();
    int mCurrentDepth = -1;
    Vector2 mHorizontalScrollPosition;
    Rect mLastViewRect;
    Vector2 mEndPos;
    System.DateTime mStartTime;
    public OnSelect onSelect;

    void UpdateAnimation()
    {
        var delta = System.DateTime.Now - mStartTime;
        float second = Mathf.Clamp01((float)delta.TotalSeconds);
        mHorizontalScrollPosition = Vector2.Lerp(mHorizontalScrollPosition, mEndPos, second);
    }
    public void Draw(Rect rect, GUIScrollTree root)
    {
        mLastViewRect = rect;

        UpdateAnimation();

        KIZ.Utility.ListAutoResize(mSelectedIndex, 1);
        using (new GUI.ClipScope(rect))
        {
            Rect viewRect = new Rect(0, 0, rect.width, rect.height);
            var node = root;
            viewRect.x -= mHorizontalScrollPosition.x;
            DrawChildren(viewRect, root);

            for (int i = 0; i < mSelectedIndex.Count; ++i)
            {
                viewRect.x += mLastViewRect.width;
                int index = mSelectedIndex[i];
                if (index < node.Children.Count)
                {
                    node = node.Children[index];
                    DrawChildren(viewRect, node);
                }
            }
        }
    }
    void DrawChildren(Rect rect, GUIScrollTree root)
    {
        GUIStyle rightArrow = new GUIStyle("AC RightArrow");
        GUIStyle leftArrow = new GUIStyle("AC LeftArrow");
        GUIStyle background = new GUIStyle("grey_border");
        Rect scrollViewRect = new Rect(0, 0, rect.width - 20, root.Children.Count * 16);
        Rect viewRect = rect;
        
        viewRect.y += 20;
        viewRect.yMax = rect.yMax;

        bool mouseRepaint = false;
        switch (Event.current.type)
        {
            case EventType.Repaint:
                {
                    mouseRepaint = true;
                }
                break;
        }

        if (!root.isRoot)
        {
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width, 20), root.Name, EditorStyles.miniButtonMid))
            {
                Back(root.Parent.Depth);
            }
            GUI.Box(new Rect(rect.x, rect.y + 4, 16, 12), GUIContent.none, leftArrow);
        }
        else
        {
            GUI.Box(new Rect(rect.x, rect.y, rect.width, 20), root.Name, EditorStyles.miniButtonMid);
        }
        using (var scrollView = new GUI.ScrollViewScope(viewRect, root.GenericValue.mVerticalScrollPos, scrollViewRect))
        {
            Rect nodeRect = new Rect(Vector2.zero, new Vector2(rect.width - GUI.skin.verticalScrollbar.fixedWidth, 16));
            for (int i = 0; i < root.Children.Count; ++i)
            {
                if (mouseRepaint)
                {
                    if (nodeRect.Contains(Event.current.mousePosition))
                    {
                        Rect cu = nodeRect;
                        cu.width = rect.width;
                        Kiz_GUIUtility.DrawSelectRect(cu);
                        mouseRepaint = false;
                    }
                }

                var child = root.Children[i];
                Rect nodeRect2 = new RectOffset(12, -12, 0, 0).Remove(nodeRect);
                if (child.isLeaf)
                {
                    if(GUI.Button(nodeRect2, child.Name, EditorStyles.label))
                    {
                        onSelect.Invoke(child);
                    }
                }
                else
                {
                    if (GUI.Button(nodeRect2, child.Name, EditorStyles.label))
                    {
                        SetSelectedIndex(root.Depth, i, child.Depth);
                    }
                    
                    GUI.Label(new Rect(nodeRect2.xMax - 16, nodeRect.y, 16, 16), GUIContent.none, rightArrow);
                }
                nodeRect.y += nodeRect.height;
            }
            root.GenericValue.mVerticalScrollPos = scrollView.scrollPosition;
        }
    }
    void SetSelectedIndex(int index, int value, int currentDepth)
    {
        if (mSelectedIndex == null) mSelectedIndex = new List<int>();
        KIZ.Utility.ListAutoResize(mSelectedIndex, currentDepth);
        mSelectedIndex[index] = value;
        mCurrentDepth = currentDepth;

        mStartTime = System.DateTime.Now;
        mEndPos = new Vector2(mLastViewRect.width * currentDepth, 0);
    }
    void Back(int currentDepth)
    {
        mCurrentDepth = currentDepth;

        mStartTime = System.DateTime.Now;
        mEndPos = new Vector2(mLastViewRect.width * currentDepth, 0);
    }
}
