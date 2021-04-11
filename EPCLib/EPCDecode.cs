using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPCLib
{
    public class EPCDecode
    {
        XDocument xDocCodingTable; //编码表
        XDocument xDocPartitionTable; //分区表
        BitArr code;

        public EPCDecode(string codingTable, string partitionTable)
        {
            xDocCodingTable = XDocument.Load(codingTable);
            xDocPartitionTable = XDocument.Load(partitionTable);
        }

        public string PureIdentityURI { get; private set; }
        public int Filter { get; private set; }
        public string TagScheme { get; private set; }
        public byte[] BinCode
        {
            get
            {
                return code.ToByteArray();
            }
            set
            {
                code = new BitArr(value);
            }
        }

        public string Decode(ref string pureURI)
        {
            if (code == null) return null;
            long header = code.SubBitToInteger(0, 8);
            var queryTagScheme = xDocCodingTable.Descendants("TagScheme")
                .Where(item => item.Element("Header").Value == header.ToString())
                .FirstOrDefault();
            if (queryTagScheme == null)
            {
                return "首部值不存在";
            }
            StringBuilder result = new StringBuilder();
            int index = 8;
            foreach (var segment in queryTagScheme.Elements())
            {
                if (segment.Name.ToString() == "Header")//首部
                {
                    result.Append(queryTagScheme.Parent.Attribute("uri").Value);
                    TagScheme = queryTagScheme.Attribute("name").Value;
                }
                else if (segment.Name.ToString() == "Filter")//过滤值
                {
                    int len = int.Parse(queryTagScheme.Element("Filter")
                        .Attribute("len").Value);
                    long filter = code.SubBitToInteger(index, len);
                    if (filter == -1)
                    {
                        return "过滤值解码错误";
                    }
                    index += len;
                    Filter = (int)filter;
                }
                else if (segment.Name.ToString() == "Partition")
                {   //首先查找分区表名称
                    string partitionName = segment.Attribute("tableName").Value;
                    //找到相应的分区表
                    var queryPartition = xDocPartitionTable.Descendants("Partition")
                        .Where(item => item.Attribute("name").Value == partitionName);
                    //首先获取分区值
                    long partVal = code.SubBitToInteger(index, 3);
                    if (partVal == -1)
                    {
                        return "分区值解码错误";
                    }
                    index += 3;
                    //查找匹配分区表行
                    var row = queryPartition.Descendants("Row")
                        .Where(item => item.Attribute("value").Value == partVal.ToString())
                        .Select(item => new
                        {
                            part1BitLen = item.Attribute("col1").Value,
                            part1Len = item.Attribute("col2").Value,
                            part2BitLen = item.Attribute("col3").Value,
                            part2Len = item.Attribute("col4").Value
                        }).FirstOrDefault();

                    //解码分区表第一部分
                    string part1Name = queryPartition.Attributes("part1Name").FirstOrDefault().Value;
                    string part1Method = queryPartition.Attributes("col2Method").FirstOrDefault().Value;
                    int len = int.Parse(row.part1BitLen);
                    int part1Len = int.Parse(row.part1Len);
                    string part1 = DecodeSegment(index, len, part1Len, part1Method);
                    if (part1 == null) return part1Name + "段解码失败";
                    index += len;
                    result.Append(part1);
                    //解码分区表第二部分
                    string part2Name = queryPartition.Attributes("part2Name").FirstOrDefault().Value;
                    string part2Method = queryPartition.Attributes("col4Method").FirstOrDefault().Value;
                    len = int.Parse(row.part2BitLen);
                    int part2Len = int.Parse(row.part2Len);
                    string part2 = DecodeSegment(index, len, part2Len, part2Method);
                    if (part2 == null) return part2Name + "段解码失败";
                    if (part2Method == "Set39_max")
                    {
                        index += Symbols.CalcActualLength(part2) * 6 + 6;
                    }
                    else
                    {
                        index += len;
                    }
                    result.Append('.');
                    result.Append(part2);
                }
                else //其它逻辑段的解码
                {
                    int len = int.Parse(segment.Attribute("len").Value);
                    string method = segment.Attribute("method").Value;
                    string name = segment.Attribute("name").Value;
                    string other = DecodeSegment(index, len, -1, method);
                    if (other == null) return "逻辑段：" + name + "解码错误";
                    if (method == "Set39_max")
                    {
                        index += Symbols.CalcActualLength(other) * 6 + 6;
                    }
                    else
                    {
                        index += len;
                    }
                    if (other == "") continue;
                    result.Append('.');
                    result.Append(other);
                }
            }




            pureURI = result.ToString();

            return null;
        }

        /// <summary>
        /// 解码逻辑段
        /// </summary>
        /// <param name="begin">逻辑段在位流中的开始位置</param>
        /// <param name="bitLen">逻辑段位长度</param>
        /// <param name="strLen">结果长度</param>
        /// <param name="method">解码方法</param>
        /// <returns>成功返回解码后的字符串，失败返回null</returns>
        private string DecodeSegment(int begin, int bitLen, int strLen, string method)
        {
            if (method == "Integer")
            {
                string result = code.SubBitToInteger(begin, bitLen).ToString();
                if (result.Length < strLen)
                {
                    result = result.PadLeft(strLen, '0');
                }
                return result;
            }
            else if (method == "Set82" || method == "Set82_max")
            {
                StringBuilder sb = new StringBuilder(20);
                int end = begin + bitLen;
                while (begin < end)
                {
                    byte hexValue = code.SubBitToByte(begin, 7);
                    if (hexValue != 0)
                    {
                        sb.Append(Symbols.GetSet82URIForm(hexValue));
                    }
                    begin += 7;
                }
                return sb.ToString();
            }
            else if (method == "Set39_max")
            {
                StringBuilder sb = new StringBuilder(20);
                int end = begin + bitLen;
                while (begin < end)
                {
                    byte hexValue = code.SubBitToByte(begin, 6);
                    if (hexValue == 0)
                    {
                        break;
                    }
                    sb.Append(Symbols.GetSet39URIForm(hexValue));
                    begin += 6;
                }
                return sb.ToString();
            }
            else if (method == "FillZero")
            {
                return "";
            }
            else if (method == "Integer_max")
            {
                string result = code.SubBitToInteger(begin, bitLen).ToString();
                if (result.Length > strLen)
                {
                    return null;
                }
                return result;
            }
            else if (method == "NumericString")
            {
                string result = code.SubBitToInteger(begin, bitLen).ToString();
                if (result[0] != '1')
                {
                    return null;
                }
                return result.Remove(0, 1);
            }
            else if (method == "CAGEorDoDAAC")
            {
                StringBuilder sb = new StringBuilder(6);
                int end = begin + bitLen;
                while (begin < end)
                {
                    byte hexValue = code.SubBitToByte(begin, 6);
                    if (hexValue == 0) break;
                    if (hexValue != 32)
                    {
                        sb.Append(Symbols.GetSet39URIForm(hexValue));
                    }
                    begin += 6;
                }
                return sb.ToString();
            }
            else if (method == "FixedWidthInteger")
            {
                //首先计算字符串长度
                int D = (int)(bitLen * Math.Log(2) / Math.Log(10)); //数字位数
                string result = code.SubBitToInteger(begin, bitLen).ToString();
                if (result.Length < D)
                {
                    result = result.PadLeft(D, '0');
                }
                return result;
            }
            return null;
        }
    }
}
