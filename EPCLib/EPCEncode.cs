using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace EPCLib
{
    public class EPCEncode
    {
        XDocument xDocCodingTable; //编码表
        XDocument xDocPartitionTable; //分区表
        string pureURI = "";
        int schemeIndex = 0;
        int index;

        public EPCEncode(string codingTable, string partitionTable)
        {   //载入XML文件
            xDocCodingTable = XDocument.Load(codingTable);
            xDocPartitionTable = XDocument.Load(partitionTable);
        }

        /// <summary>
        /// 设置Pure Identity URI，并获取可使用的二进制编码方案
        /// </summary>
        /// <param name="uri">Pure Identity URI</param>
        /// <param name="tagSchemes">获取的二进制编码方案，返回值为0时可用，最好初始化为null</param>
        /// <returns>返回null表示成功执行；否则返回错误信息。</returns>
        public string SetURI(string uri, ref string[] tagSchemes)
        {
            //必须以 urn:epc:id: 开始
            if (uri.Substring(0, 11) != "urn:epc:id:")
            {
                return "uri前缀不合法";
            }
            //通过URI中的scheme查找相应的二进制编码方案
            StringBuilder sb = new StringBuilder(10);
            int i;
            for (i = 11; i < uri.Length; i++)
            {
                if (uri[i] == ':')
                {
                    break;
                }
                sb.Append(uri[i]);
            }
            schemeIndex = i; //记录scheme结尾冒号字符的索引值
            string scheme = sb.ToString(); //提取出的前缀
            //查找此前缀所包含的二进制编码方案
            var query = xDocCodingTable.Descendants("TagScheme")
                .Where(item => item.Parent.Attribute("name").Value == scheme);
            if (query.Count() == 0)
            {   //如果没有查找到相应的二进制编码方案
                return "错误的scheme：“" + scheme + "”";
            }
            pureURI = uri;
            tagSchemes = new string[query.Count()];
            int j = 0;
            foreach (var item in query)
            {
                tagSchemes[j++] = item.Attribute("name").Value;
            }
            return null;
        }

        /// <summary>
        /// 获取Pure Identity URI的Tag URI以及二进制编码。在调用此方法前，必须首先
        /// 调用SetURI方法以设置Pure Identity URI，并确保SetURI方法调用成功。
        /// </summary>
        /// <param name="filter">将使用的过滤值</param>
        /// <param name="tagScheme">将使用的二进制编码方案</param>
        /// <param name="tagURI">得到的Tag URI</param>
        /// <param name="binEncoding">得到的二进制编码</param>
        /// <returns>成功返回null，错误则返回出错信息</returns>
        public string GetTagURIAndBinEncoding(byte filter, string tagScheme, ref string tagURI, ref string binEncoding)
        {
            bool lenVariable = false;
            //查找编码表的所有段
            var queryTagScheme = xDocCodingTable.Descendants("TagScheme")
                .Where(item => item.Attribute("name").Value == tagScheme)
                .FirstOrDefault();
            var segmentList = queryTagScheme.Elements(); //包含每个段结点信息的列表
            int totalBit = int.Parse(queryTagScheme.Attribute("len").Value);
            BitArr result = new BitArr(totalBit); //按总长度创建字节数组
            //遍历每一个段，将其转化为二进制编码
            index = schemeIndex;
            foreach (var segment in segmentList)
            {
                //首部、过滤值和分区表单独处理
                if (segment.Name == "Header")//处理首部
                {
                    result.Append(byte.Parse(segment.Value),
                        int.Parse(segment.Attributes("len").FirstOrDefault().Value));
                }
                else if (segment.Name == "Filter") //处理过滤值
                {
                    result.Append(filter,
                        int.Parse(segment.Attributes("len").FirstOrDefault().Value));
                }
                else if (segment.Name == "Partition")//处理分区表
                {
                    //首先确定分区表名称
                    string partitionName = segment.Attributes("tableName")
                        .FirstOrDefault().Value;
                    //找到相应分区表
                    var queryPartition = xDocPartitionTable.Descendants("Partition")
                        .Where(item => item.Attribute("name").Value == partitionName);
                    string part1Name = queryPartition.Attributes("part1Name")
                        .FirstOrDefault().Value;
                    string part1Method = queryPartition.Attributes("col2Method")
                        .FirstOrDefault().Value;
                    //获取URI中的属性分区表Part1的部分
                    string part1 = GetNextSegmentInUri(part1Method);
                    if (part1 == null) return part1Name + "使用了不合法的字符";
                    int part1Len = part1.Length;
                    //根据Part1的编码字符个数查找分区表中的相应行
                    var row = queryPartition.Elements("Row")
                        .Where(item => item.Attribute("col2").Value == part1Len.ToString())
                        .Select(item => new
                        {
                            val = item.Attribute("value").Value,
                            part1BitLen = item.Attribute("col1").Value,
                            part2BitLen = item.Attribute("col3").Value,
                            part2Len = item.Attribute("col4").Value
                        }).FirstOrDefault();
                    if (row == null)
                    {
                        return part1Name + "使用的字符个数不在分区表指定范围内";
                    }
                    //加入分区值
                    result.Append(byte.Parse(row.val), 3);
                    //加入分区表第一部分编码
                    int part1BitLen = int.Parse(row.part1BitLen);
                    byte[] part1Raw = Encode(part1, part1BitLen, part1Method);
                    if (part1Raw == null)
                    {
                        return "Part1编码失败";
                    }
                    result.AppendTrimLeft(part1Raw, part1BitLen);

                    //获取分区表第二部分指定字符个数
                    int part2Len = int.Parse(row.part2Len);
                    //查找分区表的Part2信息
                    string part2Name = queryPartition.Attributes("part2Name")
                        .FirstOrDefault().Value;
                    string part2Method = queryPartition.Attributes("col4Method")
                        .FirstOrDefault().Value;
                    //获取URI中的属性分区表Part2的部分
                    string part2 = GetNextSegmentInUri(part2Method);
                    if (part2 == null) return part2Name + "使用了不合法的字符";
                    //固定长度分区表列（列名不带“Max”）的合法性判断
                    if (!VerifyLength(part2, part2Len, part2Method))
                    {
                        return part2Name + "使用的字符个数不在分区表指定范围内";
                    }
                    //加入分区表第二部分编码
                    int part2BitLen = int.Parse(row.part2BitLen);
                    byte[] part2Raw = Encode(part2, part2BitLen, part2Method);
                    if (part2Raw == null)
                    {
                        return "Part2编码失败";
                    }
                    if (part2Method == "Set82_max")
                    {
                        result.AppendTrimRight(part2Raw, part2BitLen);
                    }
                    else if (part2Method == "Set39_max")
                    {
                        lenVariable = true;
                        result.AppendTrimRight(part2Raw, Symbols.CalcActualLength(part2) * 6 + 6);
                    }
                    else
                    {
                        result.AppendTrimLeft(part2Raw, part2BitLen);
                    }
                }
                else //其他段共用以下处理方法
                {
                    int len = int.Parse(segment.Attributes("len")
                        .FirstOrDefault().Value);
                    string method = segment.Attributes("method").FirstOrDefault().Value;
                    string name = segment.Attributes("name").FirstOrDefault().Value;
                    string segmentInUri = GetNextSegmentInUri(method);
                    if (segmentInUri == null)
                    {
                        return name + "使用了不合法的字符";
                    }
                    else
                    {
                        byte[] raw = Encode(segmentInUri, len, method);
                        if (method == "Integer" || method == "NumericString" || method == "FixedWidthInteger")
                        {
                            result.AppendTrimLeft(raw, len);
                        }
                        else if (method == "Set82" || method == "CAGEorDoDAAC")
                        {
                            result.AppendTrimRight(raw, len);
                        }
                        else if (method == "Set39_max")
                        {
                            lenVariable = true;
                            result.AppendTrimRight(raw, Symbols.CalcActualLength(segmentInUri) * 6 + 6);
                        }
                    }
                }
            }
            if (lenVariable) result.Trim();
            binEncoding = result.ToString();
            return null;
        }

        /// <summary>
        /// 根据"."符号获取URI中的下一个逻辑段
        /// </summary>
        /// <param name="method">逻辑段的编码方法，从编码表中获取</param>
        /// <returns>执行成功，返回逻辑段字符串；失败则返回null</returns>
        private string GetNextSegmentInUri(string method)
        {
            StringBuilder sb = new StringBuilder(10);
            for (index++; index < pureURI.Length; index++)
            {
                char c = pureURI[index];
                if (c == '.')
                {
                    break;
                }
                //判断字符合法性
                if (!VerifyChar(c, method))
                {
                    return null;
                }
                sb.Append(c);
            }
            return sb.ToString(); //获取分区表第一部分编码
        }

        /// <summary>
        /// 校验URI中的字符是否合法，此处仅编写了整数方法部分，其余省略
        /// </summary>
        /// <param name="c">字符</param>
        /// <param name="method">方法，从编码表或分区表中获取</param>
        /// <returns>合法返回true，不合法返回false</returns>
        private bool VerifyChar(char c, string method)
        {
            if (method == "Integer" || method == "Integer_max" | method == "NumericString"
                || method == "FixedWidthInteger")
            {
                if (Char.IsDigit(c))
                {
                    return true;
                }
            }
            else if (method == "Set82" || method == "Set82_max")
            {
                if (Symbols.GetSet82HexValue(c) != 0)
                {
                    return true;
                }
            }
            else if (method == "Set39_max")
            {
                if (Symbols.GetSet39HexValue(c) != 0)
                {
                    return true;
                }
            }
            else if (method == "CAGEorDoDAAC")
            {
                if (c == '#' || c == '-' || c == '/') return false;
                if (Symbols.GetSet39HexValue(c) != 0)
                {
                    return true;
                }
            }
            return false;
        }


        private bool VerifyLength(string segment, int len, string method)
        {
            if (method == "Integer" || method == "Set82")
            {
                if (len != Symbols.CalcActualLength(segment))
                {
                    return false;
                }
            } //最大长度分区表列（列名带“Max”）的合法性判断
            else if (method.Substring(method.Length - 4, 4) == "_max")
            {
                if (segment.Length > len)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 将URI段编码为二进制编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <param name="bitLen">编码位长度，对于Set39_max和CAGEorDoDAAC，此参数无关，可设置为0</param>
        /// <param name="method"></param>
        /// <returns></returns>
        private byte[] Encode(string segment, int bitLen, string method)
        {
            if (method == "Integer" || method == "Integer_max") //整数编码
            {
                return EncodeInteger(segment, bitLen);
            }
            else if (method == "Set82" || method == "Set82_max") // 字符串编码
            {
                return EncodeSet82(segment, bitLen);
            }
            else if (method == "Set39_max") //6-bit可变串编码
            {
                return EncodeSet39(segment);
            }
            else if (method == "NumericString") //数字串编码
            {
                return EncodeNumericString(segment, bitLen);
            }
            else if (method == "FixedWidthInteger") //定长整数编码
            {
                return EncodeFixedWidthInteger(segment, bitLen);
            }
            else if (method == "CAGEorDoDAAC") //6-bit CAGE/DODAAC
            {
                return EncodeCAGEorDoDAAC(segment);
            }
            return null;
        }

        /// <summary>
        /// 对整数进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <param name="bitLen">编码长度</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private static byte[] EncodeInteger(string segment, int bitLen)
        {
            long num;
            if (!long.TryParse(segment, out num))
            {
                return null;
            }
            if (num > (Math.Pow(2, bitLen)))
            {
                return null;
            }
            byte[] result = BitConverter.GetBytes(num);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// 对字符串进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <param name="bitLen">编码长度</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private static byte[] EncodeSet82(string segment, int bitLen)
        {
            int i = 0;
            BitArr bitArr = new BitArr(bitLen);
            while (i < segment.Length)
            {
                if (segment[i] != '%')
                {
                    byte hv = Symbols.GetSet82HexValue(segment[i]);
                    if (hv != 0)
                    {
                        bitArr.Append(hv, 7);
                        i++;
                    }
                    else
                    {  //此字符并不在Set82中，编码失败
                        return null;
                    }
                }
                else //转义三元组的情况
                {
                    if (i < segment.Length - 2)
                    {
                        string triplet = segment.Substring(i, 3);
                        byte hv = Symbols.GetSet82HexValue(triplet);
                        if (hv != 0)
                        {
                            bitArr.Append(hv, 7);
                            i += 3;
                        }
                        else
                        {  //此字符并不在Set82中，编码失败
                            return null;
                        }
                    }
                    else //%后面不足两个字符，凑不齐转义三元组，语法错误
                    {
                        return null;
                    }
                }
            }
            return bitArr.ToByteArray();
        }
        /// <summary>
        /// 对6-bit可变串进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private byte[] EncodeSet39(string segment)
        {
            //计算6-bit可变串长度
            int i = 0;
            int len = Symbols.CalcActualLength(segment) * 6 + 6; //每字符占用6-bit，加上长度为6-bit的数字0
            BitArr bitArr = new BitArr(len);
            while (i < segment.Length)
            {
                if (segment[i] != '%')
                {
                    byte hv = Symbols.GetSet39HexValue(segment[i]);
                    if (hv != 0)
                    {
                        bitArr.Append(hv, 6);
                        i++;
                    }
                    else
                    {  //此字符并不在Set39中，编码失败
                        return null;
                    }
                }
                else //转义三元组的情况
                {
                    if (i < segment.Length - 2)
                    {
                        string triplet = segment.Substring(i, 3);
                        byte hv = Symbols.GetSet39HexValue(triplet);
                        if (hv != 0)
                        {
                            bitArr.Append(hv, 6);
                            i += 3;
                        }
                        else
                        {  //此字符并不在Set39中，编码失败
                            return null;
                        }
                    }
                    else //%后面不足两个字符，凑不齐转义三元组，语法错误
                    {
                        return null;
                    }
                }
            }
            bitArr.Append(0, 6);
            return bitArr.ToByteArray();
        }

        /// <summary>
        /// 对数字串进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <param name="bitLen">编码长度</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private byte[] EncodeNumericString(string segment, int bitLen)
        {
            long num;
            if (!long.TryParse(segment, out num))
            {
                return null;
            }
            if (num > (Math.Pow(2, bitLen)))
            {
                return null;
            }
            segment = "1" + segment;
            long.TryParse(segment, out num);
            byte[] result = BitConverter.GetBytes(num);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// 对定长整数进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <param name="bitLen">编码长度</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private byte[] EncodeFixedWidthInteger(string segment, int bitLen)
        {
            //首先计算字符串长度
            int D = (int)(bitLen * Math.Log(2) / Math.Log(10)); //数字位数
            long num;
            if (!long.TryParse(segment, out num))
            {
                return null;
            }
            if (num > (long)Math.Pow(10, D) - 1)
            {
                return null;
            }
            byte[] result = BitConverter.GetBytes(num);
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// 对6-bit CAGE/DODAAC进行编码
        /// </summary>
        /// <param name="segment">URI中的逻辑段</param>
        /// <returns>成功返回存放编码结果的字节数组，否则返回null</returns>
        private byte[] EncodeCAGEorDoDAAC(string segment)
        {
            //计算6-bit可变串长度
            BitArr bitArr = new BitArr(36);
            if (segment.Length == 5)
            {
                bitArr.Append(32, 6);
            }
            else if (segment.Length == 6) { }
            else
            {
                return null;
            }
            for (int i = 0; i < segment.Length; i++)
            {
                byte hv = Symbols.GetSet39HexValue(segment[i]);
                if (hv != 0)
                {
                    bitArr.Append(hv, 6);
                }
                else
                {  //此字符并不在Set39中，编码失败
                    return null;
                }
            }
            return bitArr.ToByteArray();
        }
    }
}
