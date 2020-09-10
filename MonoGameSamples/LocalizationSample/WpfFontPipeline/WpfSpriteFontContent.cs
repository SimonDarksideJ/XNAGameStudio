#region Using ステートメント

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;

#endregion

namespace WpfFontPipeline
{
    /// <summary>
    /// WPFスプライトコンテント
    /// </summary>
    /// <remarks>
    /// XNBファイルフォーマットドキュメントに書かれているSpriteFontと同じ物だが、
    /// SpriteFontContentのフィールドは全て非公開フィールドなので、同じものを宣言している
    /// http://create.msdn.com/en-US/sample/xnb_format
    /// </remarks>
    public class WpfSpriteFontContent
    {
        #region プロパティ

        /// <summary>
        /// テクスチャの取得/設定
        /// </summary>
        public Texture2DContent Texture { get; set; }

        /// <summary>
        /// 文字グリフ領域の取得/設定
        /// </summary>
        public List<Rectangle> Glyphs { get; set; }

        /// <summary>
        /// 文字グリフクリップ領域の取得/設定
        /// </summary>
        public List<Rectangle> Cropping { get; set; }

        /// <summary>
        /// 文字マップの取得/設定
        /// </summary>
        public List<char> CharacterMap { get; set; }

        /// <summary>
        /// 行間スペースの取得/設定
        /// </summary>
        public int LineSpacing { get; set; }

        /// <summary>
        /// 文字間スペースの取得/設定
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// カーニングの取得/設定
        /// </summary>
        public List<Vector3> Kerning { get; set; }

        /// <summary>
        /// デフォルト文字の取得/設定
        /// </summary>
        [ContentSerializer(AllowNull = true)]
        public char? DefaultCharacter { get; set; }

        #endregion

        #region 初期化

        public WpfSpriteFontContent()
        {
            Texture = new Texture2DContent();
            Glyphs = new List<Rectangle>();
            Cropping = new List<Rectangle>();
            CharacterMap = new List<char>();
            Kerning = new List<Vector3>();
        }

        #endregion
    }

    /// <summary>
    /// WpfSpriteFontContentをXNBファイルへ出力するContentWriter
    /// </summary>
    [ContentTypeWriter]
    class WpfSpriteFontWriter : ContentTypeWriter<WpfSpriteFontContent>
    {
        /// <summary>
        /// XNBファイルへ出力する
        /// </summary>
        protected override void Write(ContentWriter output,
            WpfSpriteFontContent value)
        {
            output.WriteObject(value.Texture);
            output.WriteObject(value.Glyphs);
            output.WriteObject(value.Cropping);
            output.WriteObject(value.CharacterMap);
            output.Write(value.LineSpacing);
            output.Write(value.Spacing);
            output.WriteObject(value.Kerning);
            output.Write(value.DefaultCharacter.HasValue);

            if (value.DefaultCharacter.HasValue)
            {
                output.Write(value.DefaultCharacter.Value);
            }
        }

        /// <summary>
        /// ランタイム・リーダーの指定
        /// </summary>
        /// <returns></returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // XNA標準のSpriteFontReaderの型名をフルネームで指定する事で
            // 実行時にはSpriteFontとして使えるようになっている。
            // XNAのバージョンによってPublickKeyTokenの値が変わることに注意
            return  "Microsoft.Xna.Framework.Content.SpriteFontReader, " +
                    "Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, " +
                    "Culture=neutral, PublicKeyToken=842cf8be1de50553";
        }
    }

}
