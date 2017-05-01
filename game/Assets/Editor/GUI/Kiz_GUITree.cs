using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KIZ
{
    using GUIFoldoutTree = GUITree<FoldoutTree>;

    public class FoldoutTree
    {
        public bool mFoldout;
    }

    /// <summary>
    /// GUIのツリークラス
    /// </summary>
    /// <typeparam name="T">拡張用</typeparam>
    public class GUITree<T> where T : new()
    {
        private T mGenericValue;
        private GUITree<T> mParent = null;
        private List<GUITree<T>> mChildren = new List<GUITree<T>>();
        private int mDepth = 0;
        private string mName;

        public T GenericValue { get { return mGenericValue; } }
        public List<GUITree<T>> Children { get { return mChildren; } }
        public int Depth { get { return mDepth; } }
        public string Name { get { return mName; } }
        public GUITree<T> Parent { get { return mParent; } }

        public bool isRoot { get { return mParent == null; } }
        public bool isLeaf { get { return mChildren.Count == 0; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">ノード名</param>
        /// <param name="parent">親ノードの参照</param>
        public GUITree(string name, GUITree<T> parent = null)
        {
            mName = name;
            mParent = parent;
            mGenericValue = new T();
            if (mParent != null)
            {
                mDepth = mParent.mDepth + 1;
            }
        }

        /// <summary>
        /// 指定した名前のノードを追加します。すでにノードが存在する場合は追加されません。
        /// </summary>
        /// <param name="name">ノード名</param>
        /// <returns>新しく追加したノードを返します。既に存在する場合はそのノードを返します。</returns>
        private GUITree<T> Add(string name)
        {
            var node = FindChild(name);
            if (node == null)
            {
                node = new GUITree<T>(name, this);
                mChildren.Add(node);
            }
            return node;
        }

        /// <summary>
        /// 子ノードを探す
        /// </summary>
        /// <param name="name">ノード名</param>
        /// <returns>見つからなかったらnullを返します</returns>
        public GUITree<T> FindChild(string name)
        {
            return mChildren.Find(node => string.Equals(node.mName, name));
        }

        /// <summary>
        /// あるノードの子階層にノードを追加するメソッド
        /// </summary>
        /// <param name="parent">親となるノード</param>
        /// <param name="splitedValues">追加するノードのパス</param>
        /// <param name="depth">splitedValuesの添え字として使用します。デフォルトでは0が指定されます。</param>
        public static GUITree<T> Add(GUITree<T> parent, string[] splitedValues, int depth = 0)
        {
            // 不正なパラメータのチェック
            if (parent == null) return parent;
            if (splitedValues == null) return parent;
            if (depth >= splitedValues.Length) return parent;
            if (depth < 0) return parent;

            // 新しいノードを追加する
            var result = parent.Add(splitedValues[depth]);

            // 再起的にノードを追加する
            return Add(result, splitedValues, depth + 1);
        }

        /// <summary>
        /// あるノードの子階層にノードを追加するメソッド
        /// </summary>
        /// <param name="parent">親となるノード</param>
        /// <param name="path">ツリーのパス</param>
        /// <param name="depth">splitedValuesの添え字として使用します。デフォルトでは0が指定されます。</param>
        /// <param name="separator">パスのデリミタ</param>
        public static void Add(GUITree<T> parent, string path, int depth = 0, char separator = '/')
        {
            // 不正なパラメータのチェック
            if (parent == null) return;
            if (string.IsNullOrEmpty(path)) return;
            if (depth < 0) return;

            // 再起的にノードを追加する
            Add(parent, path.Split(separator), depth);
        }

        /// <summary>
        /// 再起的にノードのソートを行う
        /// </summary>
        /// <param name="root">ノードのルート</param>
        /// <param name="comparer">比較用のメソッド</param>
        public static void Sort(GUITree<T> root, System.Comparison<GUITree<T>> comparer)
        {
            root.mChildren.Sort(comparer);
            for (int i = 0; i < root.mChildren.Count; ++i)
            {
                Sort(root.Children[i], comparer);
            }
        }
    }

    /// <summary>
    /// FoldoutTree用のDrawer
    /// </summary>
    public class GUIFoldoutTreeDrawer
    {
        public delegate void OnSelect(GUIFoldoutTree selected);

        // 描画サイズ
        private Rect mLastScrollViewContentRect;

        // スクロール値を加味した描画エリア（判定用）
        private Rect mScrolledViewRect;

        // 描画命令が呼ばれているノードの数（処理負荷軽減の為、範囲外のものは表示しないように調整している。）
        private int mDebugDrawCount;

        // 葉ノードが選択された時に呼ばれるイベント
        public OnSelect onSelect;

        const float INDENT_SIZE = 12;
        const float SCROLL_BAR_WIDTH = 20;
        const float SCROLL_BAR_HEIGHT = 20;

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

        #region Draw

        /// <summary>
        /// インデントの計算
        /// </summary>
        /// <param name="node">ノード</param>
        /// <returns>x座標</returns>
        private static float CalcIndent(GUIFoldoutTree node)
        {
            return (node.isLeaf ? (node.Depth + 1) * INDENT_SIZE : (node.Depth * INDENT_SIZE));
        }

        /// <summary>
        /// ラベルのサイズを計算する（nullの場合は空文字で計算される)
        /// </summary>
        /// <param name="node">ノード</param>
        /// <returns></returns>
        private static Vector2 CalcLabelSize(GUIFoldoutTree node)
        {
            if (node == null)
            {
                return GUI.skin.label.CalcSize(GUIContent.none);
            }
            return GUI.skin.label.CalcSize(new GUIContent(node.Name));
        }

        /// <summary>
        /// ツリーを描画します。foldoutがtrueの時、子階層の描画を再起的に実行します。
        /// </summary>
        /// <param name="rect">描画範囲</param>
        /// <param name="root">ツリーのルートノード</param>
        public void DrawTree(Rect rect, GUIFoldoutTree root)
        {
            using (var scrollView = new GUI.ScrollViewScope(rect, ScrollPosition, mLastScrollViewContentRect))
            {
                ViewSize = rect.size;

                Vector2 size = CalcLabelSize(root);

                // ※おまじない
                // 　描画前に高さをリセットしておく
                // 　（あらかじめルートノードの高さを引いておく）
                mLastScrollViewContentRect.height = -size.y;

                // 描画範囲は縦スクロールバーの幅を引いた値にしておく
                mLastScrollViewContentRect.width = ViewSize.x - SCROLL_BAR_WIDTH;

                // 描画数のリセット
                mDebugDrawCount = 0;

                // 階層の描画
                DrawItems(root);

                // ※おまじない
                // 　ルートノード分の高さを足しておく
                mLastScrollViewContentRect.height += size.y;

                // スクロール座標の更新
                ScrollPosition = scrollView.scrollPosition;
            }
        }

        /// <summary>
        /// 再起的に描画します
        /// </summary>
        /// <param name="node"></param>
        private void DrawItems(GUIFoldoutTree node)
        {
            if (node == null) return;

            Vector2 size = CalcLabelSize(node);

            // 描画範囲の計算
            Rect drawRect = new Rect(0, 0, 0, 0);
            drawRect.x = CalcIndent(node);
            drawRect.y = mLastScrollViewContentRect.height + size.y;
            drawRect.size = size;

            // 高さを加算する
            mLastScrollViewContentRect.height += size.y;

            // 幅を計算する
            mLastScrollViewContentRect.width = Mathf.Max(mLastScrollViewContentRect.width, drawRect.xMax);

            // 処理負荷軽減の為、描画範囲外のものは描画処理を呼ばないようにする
            if(mScrolledViewRect.Overlaps(drawRect))
            {
                // 描画数を加算
                ++mDebugDrawCount;

                // 葉ノードの場合
                if (node.isLeaf)
                {
                    GUI.SetNextControlName(node.Name);
                    EditorGUI.SelectableLabel(drawRect, node.Name);
                }
                // それ以外のノードはFoldoutで描画
                else
                {
                    GUI.SetNextControlName(node.Name);
                    node.GenericValue.mFoldout = EditorGUI.Foldout(drawRect, node.GenericValue.mFoldout, node.Name);
                }
            }

            // 子ノードの描画
            if (node.GenericValue.mFoldout)
            {
                for (int i = 0; i < node.Children.Count; ++i)
                {
                    DrawItems(node.Children[i]);
                }
            }
        }

        #endregion
    }
}