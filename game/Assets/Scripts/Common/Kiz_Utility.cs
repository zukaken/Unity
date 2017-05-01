using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KIZ
{
    public static class Utility
    {
        /// <summary>
        /// リストの要素数が足りなければ増やします。
        /// </summary>
        /// <typeparam name="T">リスト型</typeparam>
        /// <param name="list">リスト</param>
        /// <param name="newSize">要素数</param>
        public static void ListAutoResize<T>(List<T> list, int newSize) 
        {
            if (newSize < 0) return;
            if (newSize >= list.Count)
            {
                int diff = newSize - list.Count;
                T[] newData = new T[diff];

                if (typeof(T).IsClass)
                {
                    for (int i = 0; i < newData.Length; ++i)
                    {
                        newData[i] = System.Activator.CreateInstance<T>();
                    }
                }
                else
                {
                    for (int i = 0; i < newData.Length; ++i)
                    {
                        newData[i] = default(T);
                    }
                }
                list.AddRange(newData);
            }
        }

        /// <summary>
        /// リストの要素数が足りなければ増やします。
        /// </summary>
        /// <typeparam name="T">リスト型</typeparam>
        /// <param name="list">リスト</param>
        /// <param name="newSize">要素数</param>
        /// <param name="creator">インスタンスの生成を行うデリゲート</param>
        public static void ListAutoResize<T>(List<T> list, int newSize, System.Func<T> creator)
        {
            if (newSize < 0) return;
            if (newSize >= list.Count)
            {
                int diff = newSize - list.Count;
                T[] newData = new T[diff];
                for (int i = 0; i < newData.Length; ++i)
                {
                    newData[i] = creator.Invoke();
                }
                list.AddRange(newData);
            }
        }
    }
}