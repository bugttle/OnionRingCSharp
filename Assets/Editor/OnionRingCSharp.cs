using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

// Original
// https://github.com/kyubuns/onion_ring/blob/master/onion_ring.rb

namespace OnionRingCSharp
{
    /// <summary>
    /// 'oily_png'のインタフェースを模したクラス
    /// </summary>
    namespace ChunkyPng
    {
        // http://www.rubydoc.info/github/wvanbergen/chunky_png/ChunkyPNG/Color
        enum Color
        {
            Transparent,
        }

        // http://www.rubydoc.info/github/wvanbergen/chunky_png/ChunkyPNG/Image
        class Image
        {
            public int height { get; private set; }

            public int width { get; private set; }

            Texture2D texture = null;

            /// <summary>
            /// イメージの読み込み
            /// </summary>
            /// <param name="path">読み込む画像のパス</param>
            /// <returns></returns>
            public static Image FromFile(string path)
            {
                return new Image(path);
            }

            protected Image(string path)
            {
                texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                width = texture.width;
                height = texture.height;
            }

            /// <summary>
            /// イメージの新規作成
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="bgColor">画像の背景色(現在未使用)</param>
            /// <returns></returns>
            public static Image New(int width, int height, ChunkyPng.Color bgColor)
            {
                return new Image(width, height, bgColor);
            }

            protected Image(int width, int height, ChunkyPng.Color bgColor)
            {
                this.width = width;
                this.height = height;
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            }

            /// <summary>
            /// 指定されたx座標の一行を返す
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public IEnumerable<UnityEngine.Color> Column(int x)
            {
                return texture.GetPixels(x, 0, 1, height).Reverse();
            }

            /// <summary>
            /// 指定されたy座標の一列を返す
            /// </summary>
            /// <param name="y"></param>
            /// <returns></returns>
            public IEnumerable<UnityEngine.Color> Row(int y)
            {
                return texture.GetPixels(0, height - 1 - y, width, 1);
            }

            /// <summary>
            /// GetPixelは左下スタートなので、左上スタートの色情報を返す
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public UnityEngine.Color GetPixel(int x, int y)
            {
                return texture.GetPixel(x, height - 1 - y);
            }

            /// <summary>
            /// SetPixelは左下スタートなので、左上スタートの色情報を変更する
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="color"></param>
            public void SetPixel(int x, int y, UnityEngine.Color color)
            {
                texture.SetPixel(x, height - 1 - y, color);
            }

            /// <summary>
            /// x,yの座標で色情報にアクセスさせるためのインデクサ
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public UnityEngine.Color this[int x, int y]
            {
                get
                {
                    return GetPixel(x, y);
                }
                set
                {
                    SetPixel(x, y, value);
                }
            }

            /// <summary>
            /// 指定したファイルへ画像を書き出す
            /// </summary>
            /// <param name="filePath"></param>
            public void Save(string filePath)
            {
                File.WriteAllBytes(filePath, texture.EncodeToPNG());
            }
        }
    }

    namespace Digest
    {
        /// <summary>
        /// 'digest/sha1'のインタフェースを模したクラス
        /// </summary>
        static class SHA1
        {
            static SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();

            public static string HexDigest(string str)
            {
                return ToHashString(ComputeHash(str));
            }

            static byte[] ComputeHash(string str)
            {
                return sha.ComputeHash(Encoding.ASCII.GetBytes(str));
            }

            static string ToHashString(byte[] bytes)
            {
                var sb = new StringBuilder();
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }

    /// <summary>
    /// Rubyの記述と同じように書けるようにするためのLINQ拡張
    /// </summary>
    static partial class LinqExtensions
    {
        public static string Join<T>(this IEnumerable<T> list, string separator)
        {
            return string.Join(separator, list.Select(o => o.ToString()).ToArray());
        }

        public static void Each<T>(this IEnumerable<T> list, System.Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }

        public static void EachWithIndex<T>(this IEnumerable<T> list, System.Action<T, int> action)
        {
            var e = list.GetEnumerator();
            var i = 0;
            while (e.MoveNext())
            {
                action(e.Current, i++);
            }
        }
    }

    /// <summary>
    /// OnionRingの本体
    /// オリジナルのソースコードと見た目が変わらないことを優先してある (実行速度 < オリジナルとの差分)
    /// </summary>
    static class OnionRing
    {
        public static SpriteBorder Run(string sourceFileName, string outputFileName = null)
        {
            outputFileName = outputFileName ?? sourceFileName;

            var png = ChunkyPng.Image.FromFile(sourceFileName);
            var rangeWidth = CalcTrimRange(Enumerable.Range(0, png.width)
                .Select(x => Digest.SHA1.HexDigest(png.Column(x).Select(c => (c.a != 0) ? c : new Color(0, 0, 0, 0)).Join(","))));
            var rangeHeight = CalcTrimRange(Enumerable.Range(0, png.height)
                .Select(x => Digest.SHA1.HexDigest(png.Row(x).Select(c => (c.a != 0) ? c : new Color(0, 0, 0, 0)).Join(","))));

            var dpix = 2;
            if (rangeWidth == null || rangeWidth[1] - rangeWidth[0] <= dpix * 2)
            {
                rangeWidth = new int[] { 0, -1 };
            }
            else
            {
                rangeWidth[0] += dpix;
                rangeWidth[1] -= dpix;
            }

            if (rangeHeight == null || rangeHeight[1] - rangeHeight[0] <= dpix * 2)
            {
                rangeHeight = new int[] { 0, -1 };
            }
            else
            {
                rangeHeight[0] += dpix;
                rangeHeight[1] -= dpix;
            }

            CreateSlicedImage(png, outputFileName, rangeWidth, rangeHeight);

            if (rangeWidth[0] == 0 && rangeWidth[1] == -1) { rangeWidth = new int[] { 1, png.width - dpix }; }
            if (rangeHeight[0] == 0 && rangeHeight[1] == -1) { rangeHeight = new int[] { 1, png.height - dpix }; }
            var left = rangeWidth[0] - 1;
            var top = rangeHeight[0] - 1;
            var right = png.width - rangeWidth[1] - dpix;
            var bottom = png.height - rangeHeight[1] - dpix;

            return new SpriteBorder(left, top, right, bottom);
        }

        static int[] CalcTrimRange(IEnumerable<string> hashList)
        {
            string tmpHash = null;
            var tmpStartIndex = 0;
            var maxLength = 0;
            int[] maxRange = null;
            var _maxRange = new int[2]; // newが何度も呼び出されるため、事前に確保

            hashList.EachWithIndex((hash, index) =>
            {
                var length = ((index - 1) - tmpStartIndex);
                if (length > maxLength)
                {
                    maxLength = length;
                    // C#用に変更
                    _maxRange[0] = tmpStartIndex;
                    _maxRange[1] = index - 1;
                    maxRange = _maxRange;
                }

                if (tmpHash != hash)
                {
                    tmpHash = hash;
                    tmpStartIndex = index;
                }
            });

            return maxRange;
        }

        static void CreateSlicedImage(ChunkyPng.Image png, string outputFileName, int[] rangeWidth, int[] rangeHeight)
        {
            var outputWidth = png.width - ((rangeWidth[1] - rangeWidth[0]) + 1);
            var outputHeight = png.height - ((rangeHeight[1] - rangeHeight[0]) + 1);

            var output = ChunkyPng.Image.New(outputWidth, outputHeight, ChunkyPng.Color.Transparent);
            Enumerable.Range(0, outputWidth).Each(ax =>
            {
                Enumerable.Range(0, outputHeight).Each(ay =>
                {
                    var bx = ax;
                    var by = ay;
                    if (bx >= rangeWidth[0]) { bx = ax + ((rangeWidth[1] - rangeWidth[0]) + 1); }
                    if (by >= rangeHeight[0]) { by = ay + ((rangeHeight[1] - rangeHeight[0]) + 1); }
                    output[ax, ay] = png.GetPixel(bx, by);
                });
            });
            output.Save(outputFileName);
        }
    }

    /// <summary>
    /// スライス後のボーダーを定義
    /// </summary>
    public struct SpriteBorder
    {
        public int left { get; private set; }

        public int top { get; private set; }

        public int right { get; private set; }

        public int bottom { get; private set; }

        public SpriteBorder(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        /// <summary>
        /// NGUIやSpriteで利用する順序でVector4を返す
        /// </summary>
        /// <returns></returns>
        public Vector4 ToVector4()
        {
            return new Vector4(left, bottom, right, top);
        }

        /// <summary>
        /// OnionRingで返された値をそのままの順序で返す
        /// </summary>
        /// <returns></returns>
        public new string ToString()
        {
            return string.Format("[{0},{1},{2},{3}]", left, top, right, bottom);
        }
    }
}
