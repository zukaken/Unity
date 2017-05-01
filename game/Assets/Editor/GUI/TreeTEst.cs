using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KIZ
{
    public class TreeTEst : EditorWindow
    {
        GUIFoldoutTreeDrawer mDrawer = new GUIFoldoutTreeDrawer();
        GUITree<FoldoutTree> mFoldoutTree = new GUITree<FoldoutTree>("Root");

        GUICell mCells = new GUICell();
        GUICellDrawer mCellDrawer = new GUICellDrawer();

        [MenuItem("Tools/Tree Test")]
        public static void Open()
        {
            GetWindow<TreeTEst>().Show();
        }

        public static List<string> GetFileList(string path)
        {
            List<string> result = new List<string>();
            try
            {
                string[] values = System.IO.Directory.GetFiles(path);
                for (int i = 0; i < values.Length; ++i)
                {
                    result.Add(values[i]);
                }
                values = System.IO.Directory.GetDirectories(path);
                for (int i = 0; i < values.Length; ++i)
                {
                    var list = GetFileList(values[i]);
                    result.AddRange(list);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            return result;
        }

        private void OnEnable()
        {
            mFoldoutTree = new GUITree<FoldoutTree>("Root");
            
            var lsit = GetFileList(Application.dataPath);
            string[] values = lsit.ToArray();
            for (int i = 0; i < values.Length; ++i)
            {
                GUITree<FoldoutTree>.Add(mFoldoutTree, values[i], separator: '\\');
            }

            string[] columns = new string[]
            {
                "番号",
                "パス",
                "更新日時",
                "作成日時",
                "読み書き",
                "サイズ",
            };

            mCells.SetCapacity(values.Length, columns.Length);
            mCells.SetColumnNames(columns);
            for (int i = 0; i < values.Length; ++i)
            {
                var fi = new System.IO.FileInfo(values[i]);
                int index = 0;
                index = 0;
                mCells.SetCellValue(i, index++, i.ToString());
                mCells.SetCellValue(i, index++, fi.FullName);
                mCells.SetCellValue(i, index++, fi.LastWriteTime.ToString("yyyy/MM/dd hh:mm"));
                mCells.SetCellValue(i, index++, fi.CreationTime.ToString("yyyy/MM/dd hh:mm"));
                mCells.SetCellValue(i, index++, fi.IsReadOnly?"Read Only":"Read Write");
                mCells.SetCellValue(i, index++, fi.Length.ToString("#,0"));
            }

        }

        private void OnGUI()
        {
            if (mFoldoutTree == null) return;
            if (mDrawer == null) return;
            Rect left = new RectOffset(20, 20, 20, 40).Remove(new Rect(0, 0, Screen.width / 2, Screen.height));
            Rect right = new RectOffset(20, 20, 20, 40).Remove(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height));
            GUI.Box(left, GUIContent.none);
            mDrawer.DrawTree(left, mFoldoutTree);

            if (mCells == null) return;
            if (mCellDrawer == null) return;
            mCellDrawer.DrawCells(right, mCells);

            Repaint();
        }
    }
}