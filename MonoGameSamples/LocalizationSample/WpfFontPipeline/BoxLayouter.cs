#region Using ステートメント

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace WpfFontPipeline
{
    /// <summary>
    /// レイアウト対象になる矩形情報
    /// </summary>
    class BoxLayoutItem
    {
        /// <summary>
        /// 矩形情報
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// タグ
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 配置済みか？
        /// </summary>
        public bool Placed { get; set; }
    }

    /// <summary>
    /// 複数矩形を1つのテクスチャにまとめるヘルパークラス
    /// </summary>
    /// <remarks>
    /// 高速化の為に処理する矩形の多くが同じ高さであるという前提のアルゴリズムを使用
    /// </remarks>
    class BoxLayouter
    {
        #region 定数

        /// <summary>
        /// 矩形間の空きスペース
        /// </summary>
        readonly int PadSize = 1;

        #endregion

        #region プロパティ

        /// <summary>
        /// 登録された矩形要素の取得と設定
        /// </summary>
        public List<BoxLayoutItem> Items {get; set; }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoxLayouter()
        {
            Items = new List<BoxLayoutItem>();
        }

        /// <summary>
        /// 矩形の追加
        /// </summary>
        /// <param name="item">追加する矩形情報</param>
        public void Add(BoxLayoutItem item)
        {
            // 矩形の高さ毎に分類、格納する
            List<BoxLayoutItem> itemBacket;
            if (!itemBackets.TryGetValue(item.Bounds.Height, out itemBacket))
            {
                itemBacket = new List<BoxLayoutItem>();
                itemBackets.Add(item.Bounds.Height, itemBacket);
            }

            itemBacket.Add(item);

            // 必要な矩形面積を追加
            int w = item.Bounds.Width + PadSize;
            int h = item.Bounds.Height + PadSize;
            totalAreaSize += (w * h);

            Items.Add(item);
        }

        /// <summary>
        /// レイアウトの実行
        /// </summary>
        /// <param name="outWidth">レイアウトに必要な横幅</param>
        /// <param name="outHeight">レイアウトに必要な高さ</param>
        public void Layout(out int outWidth, out int outHeight)
        {
            // 総面積から必要となるサイズを概算
            int size = (int)Math.Sqrt(totalAreaSize);
            int h = (int)(Math.Pow(2, (int)(Math.Log(size, 2) - 0.5)));
            int w = (int)(Math.Pow(2, (int)(Math.Log(size, 2) + 0.5)));

            while ((long)w * (long)h < totalAreaSize)
            {
                if (w <= h)
                    w *= 2;
                else
                    h *= 2;
            }

            // 高さ、幅の順に並び替える
            var keys = from key in itemBackets.Keys orderby key descending select key;
            sortedKeys = keys.ToList();

            foreach (int key in sortedKeys)
            {
                var items = from item in itemBackets[key]
                            orderby item.Bounds.Width descending
                            select item;

                itemBackets[key] = items.ToList();
            }

            // 現在のサイズで配置してみる
            while (TryLayout(w, h) == false)
            {
                // 全部配置しきれなかったので、サイズを大きくして再試行
                ClearPlacedInfo();

                if (w <= h)
                    w *= 2;
                else
                    h *= 2;
            }

            outWidth = w;
            outHeight = h;
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 配置処理
        /// </summary>
        bool TryLayout(int width, int height)
        {
            // 配置位置
            int x = PadSize;
            int y = PadSize;

            // 列の高さ
            int lineHeight = sortedKeys[0];

            // 高さの順に配置
            foreach (int key in sortedKeys)
            {
                var itemBacket = itemBackets[key];

                for (int i = 0; i < itemBacket.Count; ++i)
                {
                    var item = itemBacket[i];

                    // 既に配置済みか？
                    if (item.Placed)
                        continue;

                    // 現在の列に配置できるか？
                    if (x + item.Bounds.Width + PadSize < width)
                    {
                        // 現在の列の右端に追加
                        var bounds = item.Bounds;
                        bounds.X = x;
                        bounds.Y = y;
                        item.Bounds = bounds;
                        item.Placed = true;

                        x += item.Bounds.Width + PadSize;
                    }
                    else
                    {
                        // 右端の空きスペースに配置できるか試す
                        // 幅の狭いものから試す
                        for (int j = itemBacket.Count - 1; i < j; --j)
                        {
                            var narrowItem = itemBacket[j];

                            // 配置済みのものは無視
                            if (narrowItem.Placed)
                                continue;

                            // この列に、これ以上配置できない
                            if (x + narrowItem.Bounds.Width + PadSize >= width)
                                break;

                            var bounds = narrowItem.Bounds;
                            bounds.X = x;
                            bounds.Y = y;
                            narrowItem.Bounds = bounds;
                            narrowItem.Placed = true;

                            x += narrowItem.Bounds.Width + PadSize;
                        }

                        // 次の行へ移動
                        y += lineHeight + PadSize;

                        // おっと、サイズ足らずで配置しきれなかった
                        if (y + lineHeight > height)
                            return false;

                        lineHeight = key;
                        x = PadSize;
                        --i;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 配置情報の初期化
        /// </summary>
        void ClearPlacedInfo()
        {
            foreach (var itemBacket in itemBackets.Values)
            {
                foreach (var item in itemBacket)
                    item.Placed = false;
            }
        }

        #endregion

        #region プライベートフィールド

        // 現在の行情報をDictionary<height:int, BoxLayoutItem[]>として格納
        Dictionary<int, List<BoxLayoutItem>> itemBackets =
                                        new Dictionary<int, List<BoxLayoutItem>>();

        // 追加された矩形の総面積
        long totalAreaSize;

        // 高さ毎に並び替える為のソートキー
        List<int> sortedKeys;

        #endregion

    }
}
