using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace KIZ
{
    public class GUICell
    {
        List<string> mColumnNames = new List<string>();
        List<CellData> mRows = new List<CellData>();

        public List<CellData> Rows { get { return mRows; } }
        public List<string> ColumnNames { get { return mColumnNames; } }
        public int RowCount { get { return mRows.Count; } }
        public int ColumnCount { get { return mColumnNames.Count; } }


        public void SetCapacity(int row, int column)
        {
            Utility.ListAutoResize(mRows, row);
            Utility.ListAutoResize(mColumnNames, column, EmptyString);

            for (int i = 0; i < mRows.Count; ++i)
            {
                Utility.ListAutoResize(Rows[i].Columns, column);
            }
        }

        public string EmptyString()
        {
            return string.Empty;
        }

        /// <summary>
        /// カラムの名前をセットします
        /// </summary>
        /// <param name="columns">カラム名</param>
        public void SetColumnNames(string[] columns)
        {
            // 不正パラメータのチェック
            if (columns == null) return;
            for (int i = 0; i < columns.Length; ++i)
            {
                // 要素数オーバーは新規追加する
                if (i >= mColumnNames.Count)
                {
                    mColumnNames.Add(columns[i]);
                }
                else
                {
                    mColumnNames[i] = columns[i];
                }
            }
        }

        /// <summary>
        /// カラムの名前をセットします
        /// </summary>
        /// <param name="column">カラム名</param>
        /// <param name="index">要素番号</param>
        public void SetColumnName(string column, int index)
        {
            // 不正パラメータのチェック
            if (index < 0) return;
            // リストのリサイズ
            Utility.ListAutoResize(mColumnNames, index, EmptyString);
            mColumnNames[index] = column;
        }

        /// <summary>
        /// セルに値をセットします
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        /// <param name="value">値</param>
        /// <param name="autoResize">セルを自動インサートするか</param>
        public void SetCellValue(int row, int column, string value, bool autoResize = true)
        {
            // 不正パラメータのチェック
            if (row < 0) return;
            if (column< 0) return;

            // 自動リサイズが無効の場合はインデックスが不正でないかチェック
            if (!autoResize)
            {
                if (row >= mRows.Count) return;
                if (column >= mRows[row].ColumnCount) return;
            }
            // 自動リサイズが有効の場合は
            else
            {
                // リストのリサイズ
                Utility.ListAutoResize(mRows, row);
                Utility.ListAutoResize(mRows[row].Columns, column);

                // カラム名の方もリサイズしとく
                Utility.ListAutoResize(mColumnNames, column, EmptyString);
            }

            mRows[row].Columns[column].StringValue = value;
        }
    }

    public class GUICellDrawer
    {
        // スクロール値を加味した描画エリア（判定用）
        private Rect mScrolledViewRect;

        // 描画命令が呼ばれているノードの数（処理負荷軽減の為、範囲外のものは表示しないように調整している。）
        private int mDebugDrawCount;

        private List<float> mColumnWidth = new List<float>();

        private float TotalColumnWidth
        {
            get
            {
                return mColumnWidth.Sum() + 20;
            }
        }

        #region Properties

        // スクロール座標
        private Vector2 ScrollPosition
        {
            get
            {
                return mScrolledViewRect.position;
            }
            set
            {
                mScrolledViewRect.position = value;
            }
        }

        // ビューのサイズ
        private Vector2 ViewSize
        {
            get
            {
                return mScrolledViewRect.size;
            }
            set
            {
                mScrolledViewRect.size = value;
            }
        }

        // 描画命令が呼ばれているノードの数
        public int DrawCount { get { return mDebugDrawCount; } }

        #endregion

        public void DrawCells(Rect rect, GUICell cells)
        {
            GUI.Box(rect, GUIContent.none);
            Rect scrollViewContentRect = new Rect(0, 0, rect.width - 20, cells.RowCount * 16);
            Utility.ListAutoResize(mColumnWidth, cells.ColumnCount, DefatultWidth);

            Rect clipRect = new Rect(rect.x, rect.y, rect.width, 20);
            Rect columnRect = clipRect;
            columnRect.x = -ScrollPosition.x;
            columnRect.y = 0;
            columnRect.width = TotalColumnWidth;

            GUI.Button(new Rect(columnRect.y - 20, 0,120,20), mDebugDrawCount.ToString(), EditorStyles.miniButtonMid);
            mDebugDrawCount = 0;
            using (new GUI.ClipScope(clipRect))
            {
                Rect hitRect = new Rect(0, 0, rect.width, clipRect.height);
                // カラム名の描画
                for (int i = 0; i < cells.ColumnCount; ++i)
                {
                    columnRect.width = mColumnWidth[i];
                    if (hitRect.Overlaps(columnRect))
                    {
                        ++mDebugDrawCount;
                        GUI.Button(columnRect, cells.ColumnNames[i], EditorStyles.miniButtonMid);
                    }
                    columnRect.x += mColumnWidth[i];
                }
            }
            scrollViewContentRect.width = TotalColumnWidth - 40;

            Rect viewRect = new RectOffset(0, 0, (int)columnRect.height, 0).Remove(rect);

            ViewSize = viewRect.size - new Vector2(20, 20);
            // セルの描画
            using (var scrollView = new GUI.ScrollViewScope(viewRect, ScrollPosition, scrollViewContentRect))
            {
                DrawItems(cells);

                // スクロール座標の更新
                ScrollPosition = scrollView.scrollPosition;
            }


            clipRect = new Rect(rect.x, rect.y, rect.width - 20, viewRect.height + 1);
            columnRect.x = -ScrollPosition.x;
            columnRect.y = 0;
            columnRect.width = TotalColumnWidth;
            using (new GUI.ClipScope(clipRect))
            {
                Rect hitRect = new Rect(0, 0, rect.width, clipRect.height);
                Handles.BeginGUI();
                Handles.color = Color.gray;
                for (int i = 0; i < cells.ColumnCount; ++i)
                {
                    columnRect.width = mColumnWidth[i];
                    if (hitRect.Contains(new Vector2(columnRect.x + 0.5f, 1)))
                    {
                        ++mDebugDrawCount;
                        Handles.DrawLine(new Vector2(columnRect.x + 0.5f, 1), new Vector2(columnRect.x + 0.5f, viewRect.height + 0.5f));
                    }
                    columnRect.x += mColumnWidth[i];
                }
                Handles.color = Color.white;
                Handles.EndGUI();
            }
        }
        public float DefatultWidth()
        {
            return 100;
        }
        private void DrawItems(GUICell cells)
        {
            int startRowIndex = Mathf.CeilToInt(mScrolledViewRect.yMin / 16.0f);
            int endRowIndex = Mathf.CeilToInt(mScrolledViewRect.yMax / 16.0f);

            //int startColumnIndex = Mathf.CeilToInt(mScrolledViewRect.xMin / 16.0f);
            //int endColumnIndex = Mathf.CeilToInt(mScrolledViewRect.xMax / 16.0f);

            for (int i = 0; i < cells.Rows.Count; ++i)
            {
                if (i >= cells.Rows.Count) break;
                var row = cells.Rows[i];

                Rect rect = new Rect(0, i * 16, 0, 16);

                for (int j = 0; j < row.Columns.Count; ++j)
                {
                    var column = row.Columns[j];
                    var size = GUI.skin.label.CalcSize(new GUIContent(column.StringValue));
                    mColumnWidth[j] = Mathf.Max(size.x + 20, mColumnWidth[j]);
                    rect.width = mColumnWidth[j];
                    if(mScrolledViewRect.Overlaps(new Rect(rect.position, size)))
                    {
                        ++mDebugDrawCount;
                        GUI.SetNextControlName("cell." + i + "." + j);
                        EditorGUI.SelectableLabel(rect, column.StringValue);
                    }
                    rect.x += rect.width;
                }
            }
        }
    }



    public class CellData
    {
        private string mStringValue;
        private List<CellData> mColumns = new List<CellData>();
        public int ColumnCount { get { return mColumns.Count; } }

        public List<CellData> Columns
        {
            get
            {
                return mColumns;
            }
        }

        public string StringValue
        {
            set
            {
                mStringValue = value;
            }
            get
            {
                return mStringValue;
            }
        }

        public CellData()
        {
        }

        public CellData(string stringValue)
        {
            mStringValue = stringValue;
        }

        public CellData AddColumn(string stringValue)
        {
            var newData = new CellData(stringValue);
            mColumns.Add(newData);
            return newData;
        }
    }

}