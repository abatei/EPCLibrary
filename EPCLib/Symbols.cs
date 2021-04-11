/****************************************
 * 作者：陈广 2021-3-18
 * 用于存放Set82、Set39以及错误码的编码表。
 * 并未使用常用的字典类，原因是三个字段都要作为关键字进行搜索，在条目不多的情况下
 * 使用字典这么复杂的类作为容器反而拖慢速度，于是使用了最原始的数组作为容器
*****************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPCLib
{
    class Symbols
    {
        class Code
        {
            public char GraphicSymbol { get; set; }
            public byte HexValue { get; set; }
            public string URIForm { get; set; }

            public Code(char graphicSymbol, byte hexValue, string uriForm)
            {
                GraphicSymbol = graphicSymbol;
                HexValue = hexValue;
                URIForm = uriForm;
            }
        }

        static Code[] Set82 = new Code[]
        {
            new Code('!', 0x21, "!"),
            new Code('"', 0x22, "%22"),
            new Code('%', 0x21, "%25"),
            new Code('&', 0x21, "%26"),
            new Code('\'', 0x27, "'"),
            new Code('(', 0x28, "("),
            new Code(')', 0x29, ")"),
            new Code('*', 0x2A, "*"),
            new Code('+', 0x2B, "+"),
            new Code(',', 0x2C, ","),
            new Code('-', 0x2D, "-"),
            new Code('.', 0x2E, "."),
            new Code('/', 0x2F, "%2F"),
            new Code('0', 0x30, "0"),
            new Code('1', 0x31, "1"),
            new Code('2', 0x32, "2"),
            new Code('3', 0x33, "3"),
            new Code('4', 0x34, "4"),
            new Code('5', 0x35, "5"),
            new Code('6', 0x36, "6"),
            new Code('7', 0x37, "7"),
            new Code('8', 0x38, "8"),
            new Code('9', 0x39, "9"),
            new Code(':', 0x3A, ":"),
            new Code(';', 0x3B, ";"),
            new Code('<', 0x3C, "%3C"),
            new Code('=', 0x3D, "="),
            new Code('>', 0x3E, "%3E"),
            new Code('?', 0x3F, "%3F"),
            new Code('A', 0x41, "A"),
            new Code('B', 0x42, "B"),
            new Code('C', 0x43, "C"),
            new Code('D', 0x44, "D"),
            new Code('E', 0x45, "E"),
            new Code('F', 0x46, "F"),
            new Code('G', 0x47, "G"),
            new Code('H', 0x48, "H"),
            new Code('I', 0x49, "I"),
            new Code('J', 0x4A, "J"),
            new Code('K', 0x4B, "K"),
            new Code('L', 0x4C, "L"),
            new Code('M', 0x4D, "M"),
            new Code('N', 0x4E, "N"),
            new Code('O', 0x4F, "O"),
            new Code('P', 0x50, "P"),
            new Code('Q', 0x51, "Q"),
            new Code('R', 0x52, "R"),
            new Code('S', 0x53, "S"),
            new Code('T', 0x54, "T"),
            new Code('U', 0x55, "U"),
            new Code('V', 0x56, "V"),
            new Code('W', 0x57, "W"),
            new Code('X', 0x58, "X"),
            new Code('Y', 0x59, "Y"),
            new Code('Z', 0x5A, "Z"),
            new Code('_', 0x5F, "_"),
            new Code('a', 0x61, "a"),
            new Code('b', 0x62, "b"),
            new Code('c', 0x63, "c"),
            new Code('d', 0x64, "d"),
            new Code('e', 0x65, "e"),
            new Code('f', 0x66, "f"),
            new Code('g', 0x67, "g"),
            new Code('h', 0x68, "h"),
            new Code('i', 0x69, "i"),
            new Code('j', 0x6A, "j"),
            new Code('k', 0x6B, "k"),
            new Code('l', 0x6C, "l"),
            new Code('m', 0x6D, "m"),
            new Code('n', 0x6E, "n"),
            new Code('o', 0x6F, "o"),
            new Code('p', 0x70, "p"),
            new Code('q', 0x71, "q"),
            new Code('r', 0x72, "r"),
            new Code('s', 0x73, "s"),
            new Code('t', 0x74, "t"),
            new Code('u', 0x75, "u"),
            new Code('v', 0x76, "v"),
            new Code('w', 0x77, "w"),
            new Code('x', 0x78, "x"),
            new Code('y', 0x79, "y"),
            new Code('z', 0x7A, "z"),
        };

        static Code[] Set39 = new Code[]
        {
            new Code('#', 0x23, "%23"),
            new Code('-', 0x2D, "-"),
            new Code('/', 0x2F, "%2F"),
            new Code('0', 0x30, "0"),
            new Code('1', 0x31, "1"),
            new Code('2', 0x32, "2"),
            new Code('3', 0x33, "3"),
            new Code('4', 0x34, "4"),
            new Code('5', 0x35, "5"),
            new Code('6', 0x36, "6"),
            new Code('7', 0x37, "7"),
            new Code('8', 0x38, "8"),
            new Code('9', 0x39, "9"),
            new Code('A', 0x01, "A"),
            new Code('B', 0x02, "B"),
            new Code('C', 0x03, "C"),
            new Code('D', 0x04, "D"),
            new Code('E', 0x05, "E"),
            new Code('F', 0x06, "F"),
            new Code('G', 0x07, "G"),
            new Code('H', 0x08, "H"),
            new Code('I', 0x09, "I"),
            new Code('J', 0x0A, "J"),
            new Code('K', 0x0B, "K"),
            new Code('L', 0x0C, "L"),
            new Code('M', 0x0D, "M"),
            new Code('N', 0x0E, "N"),
            new Code('O', 0x0F, "O"),
            new Code('P', 0x10, "P"),
            new Code('Q', 0x11, "Q"),
            new Code('R', 0x12, "R"),
            new Code('S', 0x13, "S"),
            new Code('T', 0x14, "T"),
            new Code('U', 0x15, "U"),
            new Code('V', 0x16, "V"),
            new Code('W', 0x17, "W"),
            new Code('X', 0x18, "X"),
            new Code('Y', 0x19, "Y"),
            new Code('Z', 0x1A, "Z"),
         };

        /// <summary>
        /// 获取指定字符的URI形式，之所以不直接使用字符的ASCII，是因为部分特殊字符使用三元组转义字符表示
        /// </summary>
        /// <param name="graphicSymbol">字符的图形符号形式</param>
        /// <returns>返回字符的URI形式，如果字符不在标准规定的82个字符之中，则返回""</returns>
        public static string GetSet82URIForm(char graphicSymbol)
        {
            foreach (Code code in Set82)
            {
                if (code.GraphicSymbol == graphicSymbol)
                {
                    return code.URIForm;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取指定字符的URI形式
        /// </summary>
        /// <param name="hexValue">字符的编码</param>
        /// <returns>返回字符的URI形式，如果字符不在标准规定的82个字符之中，则返回""</returns>
        public static string GetSet82URIForm(byte hexValue)
        {
            foreach (Code code in Set82)
            {
                if (code.HexValue == hexValue)
                {
                    return code.URIForm;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取指定字符的编码值
        /// </summary>
        /// <param name="graphicSymbol">字符的图形符号形式</param>
        /// <returns>返回字符的编码，如果字符不在标准规定的82个字符之中，则返回0</returns>
        public static byte GetSet82HexValue(char graphicSymbol)
        {
            foreach (Code code in Set82)
            {
                if (code.GraphicSymbol == graphicSymbol)
                {
                    return code.HexValue;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取单个字符或三元组转义字符的编码值
        /// </summary>
        /// <param name="uriForm">单个字符组成的字符串或三元组转义字符组成的字符串</param>
        /// <returns>返回字符的编码，如果字符不在标准规定的82个字符之中，则返回0</returns>
        public static byte GetSet82HexValue(string uriForm)
        {
            foreach (Code code in Set82)
            {
                if (code.URIForm == uriForm.ToUpper())
                {
                    return code.HexValue;
                }
            }
            return 0;
        }

        /// <summary>
        /// 在39编码集中获取指定字符的URI形式，之所以不直接使用字符的ASCII，是因为部分特殊字符使用三元组转义字符表示
        /// </summary>
        /// <param name="graphicSymbol">字符的图形符号形式</param>
        /// <returns>返回字符的URI形式，如果字符不在标准规定的39个字符之中，则返回""</returns>
        public static string GetSet39URIForm(char graphicSymbol)
        {
            foreach (Code code in Set39)
            {
                if (code.GraphicSymbol == graphicSymbol)
                {
                    return code.URIForm;
                }
            }
            return "";
        }

        /// <summary>
        /// 在39编码集中获取指定字符的URI形式
        /// </summary>
        /// <param name="hexValue">字符编码/param>
        /// <returns>返回字符的URI形式，如果字符不在标准规定的39个字符之中，则返回""</returns>
        public static string GetSet39URIForm(byte hexValue)
        {
            foreach (Code code in Set39)
            {
                if (code.HexValue == hexValue)
                {
                    return code.URIForm;
                }
            }
            return "";
        }

        /// <summary>
        /// 获取39编码集中指定字符的编码值
        /// </summary>
        /// <param name="graphicSymbol">字符的图形符号形式</param>
        /// <returns>返回字符的编码，如果字符不在标准规定的82个字符之中，则返回0</returns>
        public static byte GetSet39HexValue(char graphicSymbol)
        {
            foreach (Code code in Set82)
            {
                if (code.GraphicSymbol == graphicSymbol)
                {
                    return code.HexValue;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取39编码集单个字符或三元组转义字符的编码值
        /// </summary>
        /// <param name="uriForm">单个字符组成的字符串或三元组转义字符组成的字符串</param>
        /// <returns>返回字符的编码，如果字符不在标准规定的82个字符之中，则返回0</returns>
        public static byte GetSet39HexValue(string uriForm)
        {
            foreach (Code code in Set82)
            {
                if (code.URIForm == uriForm.ToUpper())
                {
                    return code.HexValue;
                }
            }
            return 0;
        }

        /// <summary>
        /// 计算字符串的实际长度，转义三元组的三个字符应当成一个字符
        /// </summary>
        /// <param name="segment">给定的URI段</param>
        /// <returns>实际的字符数</returns>
        public static int CalcActualLength(string segment)
        {
            //查找字符%在段中的出现次数
            int count = 0;
            foreach (char c in segment)
            {
                if (c == '%')
                {
                    count++;
                }
            }
            return segment.Length - count * 2;
        }
    }
}
