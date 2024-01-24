using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using TextCopy;

using static SirSqlValetCommands.Data.SVCGlobal;
using static SirSqlValetCommands.Data.GCSS;
using static SirSqlValetCommands.Data.Extensions;

namespace SirSqlValetCommands.Data
{
    public static class GCSS
    {
        public  static  string  troisTiPoints       => "white-space: nowrap; overflow: hidden; text-overflow:ellipsis"; 
        
        public  static  string  beauBackground      => $"background-image: linear-gradient(180deg, {@GCSS.Background_Top} 0%, {@GCSS.Background_Bottom} 100%); background-color:{@GCSS.SteelBlue_Dark.BlackOrWhiteHexFader(0.5)}";

        public  static  string  SteelBlue           => "#4682B4";
        public  static  string  SteelBlue_Light     => SteelBlue.BlackOrWhiteHexFader(.5);
        public  static  string  SteelBlue_Dark      => SteelBlue.BlackOrWhiteHexFader(-.5);

        public  static  string  Background_Top      => SteelBlue.BlackOrWhiteHexFader(-.80); // rgb(5, 39, 103)
        public  static  string  Background_Bottom   => "#3A0647";

        public  static  string  BlackOrWhiteHexFader    (this string hexColor, double p)    => "#" + string.Join("", hexColor.ToIntArray().Select(_ => BlackOrWhiteIntFader(_, p).ToString("X2")));
        private static  int[]   ToIntArray              (this string hex)                   => Enumerable.Range(0, hex.Replace("#", "").Length / 2).Select(_ => Convert.ToInt32($"0x{hex.Replace("#", "").Substring(_ * 2, 2)}", 16)).ToArray();
        private static  int     BlackOrWhiteIntFader    (this int i, double p)              => Math.Max(0, Math.Min(255, i + (int)Math.Round(p * Math.Abs((p > 0 ? 255 : 0) - i), 0, MidpointRounding.AwayFromZero)));

        public  static  Color   ToColor                 (this string hexColor)
        {
            int[] rgb = hexColor.ToIntArray();
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }

    }

    public static class Extensions
    {
        public static   IEnumerable<int>    RangeFromTo(this IEnumerable<int> ie, int from, int to) => Enumerable.Range(from, to - from + 1);

        public static   bool isnws          (this string s)                                         => string.IsNullOrWhiteSpace(s);
        public static   bool notisnws       (this string s)                                         => !s.isnws();
        public static   bool isne           (this string s)                                         => string.IsNullOrEmpty(s);
        public static   bool notisne        (this string s)                                         => !s.isne();

        public static   IEnumerable<string> SplitTextOnLines(this string textToSplit)
        {
            return textToSplit.Split(new[] { $"\r\n", $"\r", $"\n" }, StringSplitOptions.None);
        }

        public static string Join(this IEnumerable<string>  list, char      separator,          bool before = false, bool after = false) => JoinBeforeAfter(separator.ToString(),               list, before, after);
        public static string Join(this IEnumerable<string>  list, string    separator = null,   bool before = false, bool after = false) => JoinBeforeAfter(separator ?? Environment.NewLine,   list, before, after);
        public static string Join(this IEnumerable<char>    list, char      separator,          bool before = false, bool after = false) => JoinBeforeAfter(separator.ToString(),               list, before, after);
        public static string Join(this IEnumerable<char>    list, string    separator = null,   bool before = false, bool after = false) => JoinBeforeAfter(separator,                          list, before, after);

        private static string JoinBeforeAfter<T>(string separator, IEnumerable<T> list, bool before, bool after) => (before ? separator : "") + string.Join(separator, list) + (after ? separator : "");

        public static IEnumerable<(T _, int i)> WithIndex<T>(this IEnumerable<T> list)
        {
            return list.Select((T _, int i) => (_, i)).OrderBy(_ => _.i);
        }
        public static IEnumerable<T> FromTo<T>(this IEnumerable<T> list, int from, int? to = null)
        {
            to = to ?? (list.Count() - 1);
            return list.FromToIdx(from, to).Select(_ => _._);
        }
        public static IEnumerable<(T _, int i)> FromToIdx<T>(this IEnumerable<T> list, int from, int? to = null)
        {
            to = to ?? (list.Count() - 1);
            return list.WithIndex().Skip(from).Take((int)to - from + 1);
        }
        public static IEnumerable<int> RangeFromTo(int from, int to)
        {
            var returnValue = Enumerable.Range(from, Math.Abs(to - from + 1));
            if (from > to)
                returnValue = returnValue.Reverse();

            return returnValue;
        }
    }
}