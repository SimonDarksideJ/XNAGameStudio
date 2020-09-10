#region Using ステートメント

using System.Collections.Generic;
using System.Text;

#endregion

namespace WpfFontPipeline
{
    /// <summary>
    /// JISコードで定義されている文字を取得する為のヘルパークラス
    /// ここではJIS基本漢字(JIS X 0208)で定義されている文字を返す
    /// http://www.asahi-net.or.jp/~ax2s-kmtn/ref/jisx0208.html
    /// </summary>
    static class JisCode
    {
        #region パブリックメソッド

        /// <summary>
        /// 記号、特殊記号、148字の取得
        /// </summary>
        public static IEnumerable<char> GetSpecialCharacters()
        {
            var list = new List<char>(148);
            AddRow(1, list);
            AddJisCodes(0x2220, 0x222e, list);
            AddJisCodes(0x223a, 0x2241, list);
            AddJisCodes(0x224a, 0x2250, list);
            AddJisCodes(0x225c, 0x226a, list);
            AddJisCodes(0x2272, 0x2279, list);
            AddJisCodes(0x227e, 0x227e, list);
            return list;
        }

        /// <summary>
        /// 全角英数字、62字の取得
        /// </summary>
        public static IEnumerable<char> GetLatinLetters()
        {
            var list = new List<char>(62);
            AddJisCodes(0x2330, 0x2339, list);
            AddJisCodes(0x2341, 0x235a, list);
            AddJisCodes(0x2361, 0x237a, list);
            return list;
        }

        /// <summary>
        /// ひらがな、83字の取得
        /// </summary>
        public static IEnumerable<char> GetHiragana()
        {
            var list = new List<char>(83);
            AddJisCodes(0x2421, 0x2473, list);
            return list;
        }

        /// <summary>
        /// カタカナ、86字の取得
        /// </summary>
        public static IEnumerable<char> GetKatakana()
        {
            var list = new List<char>(86);
            AddJisCodes(0x2521, 0x2576, list);
            return list;
        }

        /// <summary>
        /// ギリシャ文字、48字の取得
        /// </summary>
        public static IEnumerable<char> GetGreekLetters()
        {
            var list = new List<char>(48);
            AddJisCodes(0x2621, 0x2638, list);
            AddJisCodes(0x2641, 0x2658, list);
            return list;
        }

        /// <summary>
        /// キリル文字、66字の取得
        /// </summary>
        public static IEnumerable<char> GetCyrillicLetters()
        {
            var list = new List<char>(66);
            AddJisCodes(0x2721, 0x2741, list);
            AddJisCodes(0x2751, 0x2771, list);
            return list;

        }

        /// <summary>
        /// 罫線文字、32字の取得
        /// </summary>
        public static IEnumerable<char> GetBoxDrawingCharacters()
        {
            var list = new List<char>(32);
            AddJisCodes(0x2821, 0x2840, list);
            return list;
        }

        /// <summary>
        /// 第１水準漢字、2,965字の取得
        /// </summary>
        public static IEnumerable<char> GetKanjiLevel1()
        {
            var list = new List<char>(2965);
            for (int row = 16; row <= 46; ++row)
            {
                AddRow(row, list);
            }
            AddJisCodes(0x4f21, 0x4f53, list);
            return list;
        }

        /// <summary>
        /// 第２水準漢字、3,990字の取得
        /// </summary>
        public static IEnumerable<char> GetKanjiLevel2()
        {
            var list = new List<char>(3390);
            for (int row = 48; row <= 83; ++row)
            {
                AddRow(row, list);
            }
            AddJisCodes(0x7421, 0x7426, list);
            return list;
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 指定した区の文字取得
        /// </summary>
        /// <param name="row">区番号</param>
        /// <param name="list">出力先</param>
        private static void AddRow(int row, List<char> list)
        {
            int offset = 0x2000 + row * 0x100;
            AddJisCodes(offset + 0x21, offset + 0x7e, list);
        }

        /// <summary>
        /// 指定されたJISコード領域の文字取得
        /// </summary>
        /// <param name="start">開始JISコード</param>
        /// <param name="end">終了JISコード</param>
        /// <param name="list">出力先</param>
        private static void AddJisCodes(int start, int end, List<char> list)
        {
            int idx = 0;
            buffer[idx++] = 0x1b;   // 2バイト文字コードへのエスケープシーケンス
            buffer[idx++] = 0x24;
            buffer[idx++] = 0x42;
            for (int jisCode = start; jisCode <= end; ++jisCode)
            {
                buffer[idx++] = (byte)((jisCode >> 8) & 0xff);
                buffer[idx++] = (byte)(jisCode & 0xff);
            }

            list.AddRange(encoding.GetChars(buffer, 0, idx));
        }

        #endregion

        #region プライベートフィールド

        // JISコードからUnicode変換用のEncoding
        static Encoding encoding = Encoding.GetEncoding("iso-2022-jp");

        // JISコード格納用バッファ、
        // ひとつの区内の96文字+エスケープシーケンス分のサイズ
        static byte[] buffer = new byte[16 * 6 * 2 + 3];

        #endregion

    }
}
