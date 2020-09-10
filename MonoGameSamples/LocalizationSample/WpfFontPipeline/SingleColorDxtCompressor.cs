#region Using ステートメント

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace WpfFontPipeline
{
    /// <summary>
    /// 単色テクスチャイメージに特化したDXT3テクスチャ圧縮クラス
    /// </summary>
    /// <remarks>
    /// フォントなどの高周波成分を含んだ画像はDXT圧縮するべきではないが、
    /// 乗算済みアルファの単色画像であればDXT圧縮ブロックを直接生成することで
    /// 劣化が少なく、かつテクスチャメモリ使用量を1/4にすることができる。
    /// </remarks>
    public static class SingleColorDxtCompressor
    {
        /// <summary>
        /// 指定された単色ビットマップ画像をDXT3テクスチャへ変換する
        /// </summary>
        /// <param name="source">変換元画像</param>
        /// <param name="color0">単色カラー</param>
        /// <returns>DXT3圧縮された画像</returns>
        public static Dxt3BitmapContent Compress(PixelBitmapContent<Color> source,
                                                    Color color0)
        {
            // DXT3ブロックデータを格納するためのバッファを確保
            byte[] outputData = new byte[source.Width * source.Height];

            // 単色カラーをBGR565に変換する
            ushort packedColor = new Bgr565(color0.ToVector3()).PackedValue;

            // 指定された画像を圧縮ブロック単位に処理をする
            int outputIndex = 0;
            for (int blockY = 0; blockY < source.Height; blockY += 4)
            {
                for (int blockX = 0; blockX < source.Width; blockX += 4)
                {
                    CompressDxt3Block(source, blockX, blockY, packedColor,
                                        outputData, outputIndex);
                    outputIndex += 16;
                }
            }

            // DXT3テクスチャの生成と圧縮したブロックデータの設定
            var result = new Dxt3BitmapContent(source.Width, source.Height);
            result.SetPixelData(outputData);

            return result;
        }

        /// <summary>
        /// 圧縮ブロックの処理
        /// </summary>
        /// <param name="source">元画像</param>
        /// <param name="blockX">Xブロック位置</param>
        /// <param name="blockY">Yブロック位置</param>
        /// <param name="color0">単色カラー</param>
        /// <param name="outputData">出力先</param>
        /// <param name="outputIndex">出力オフセット</param>
        private static void CompressDxt3Block(PixelBitmapContent<Color> source,
            int blockX, int blockY, ushort color0, byte[] outputData, int outputIndex)
        {
            long alphaBits = 0;
            int rgbBits = 0;
            int pixelCount = 0;

            // 4x4ブロック内の処理
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    // 元のアルファ値の取得
                    int value = source.GetPixel(blockX + x, blockY + y).A;
                    int alpha = 0;
                    int rgb = 0;

                    // アルファ値によって、出力値を決定。
                    // ここでは単純にアルファ値領域を4分割するのではなく、6分割にして
                    // 1/6、1/2、5/6の非線形の閾値を使っている
                    if (value < 256 / 6)
                    {
                        alpha = 0;
                        rgb = 1;    // c1色 = 0
                    }
                    else if (value < 256 * 3 / 6)
                    {
                        alpha = 5;
                        rgb = 3;    // c3色 = 1/3(c0) + 2/3(c1) = 85
                    }
                    else if (value < 256 * 5 / 6)
                    {
                        alpha = 10;
                        rgb = 2;    // c2色 = 1/2(c0) + 1/2(c1) = 127
                    }
                    else
                    {
                        alpha = 15;
                        rgb = 0;    // c0色 = 255
                    }

                    // 計算結果ビットを格納
                    alphaBits |= (long)alpha << (pixelCount * 4);
                    rgbBits |= rgb << (pixelCount * 2);

                    pixelCount++;
                }
            }

            // DXT3ブロック情報の出力

            // アルファ 8バイト
            for (int i = 0; i < 8; ++i)
            {
                outputData[outputIndex + i] = (byte)(alphaBits >> (i * 8));
            }

            // カラー値(c0, c1) 4バイト
            // c0
            outputData[outputIndex + 8] = (byte)(color0 & 0xff);
            outputData[outputIndex + 9] = (byte)((color0 >> 8) & 0xff);
            // c1
            outputData[outputIndex + 10] = 0x00;
            outputData[outputIndex + 11] = 0x00;

            // RGB情報 4バイト
            for (int i = 0; i < 4; ++i)
            {
                outputData[outputIndex + 12 + i] = (byte)(rgbBits >> (i * 8));
            }
        }

    }
}
