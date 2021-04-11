using System;
using System.Text;

namespace EPCLib
{
    class BitArr
    {
        private byte[] items;
        private int index = 0;//从最高位开始计数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="len">指定BitArr的位长度</param>
        public BitArr(int len)
        {
            items = new byte[len / 8 + ((len % 8 == 0) ? 0 : 1)];
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="binCode">指定BitArr所包含的位流，使用字节数组的形式存放</param>
        public BitArr(byte[] binCode)
        {
            items = binCode;
        }
        /// <summary>
        /// 追加由字符串转化而来的字节数组，如果len小于数组的总位数，则从右边
        /// 字节开始裁减多出的位。
        /// </summary>
        /// <param name="vals">要追加的字节数组</param>
        /// <param name="len">占用的比特数</param>
        public void AppendTrimRight(byte[] vals, int len)
        {

            if ((len / 8 + ((len % 8 == 0) ? 0 : 1)) > vals.Length)
            {
                throw new ArgumentException("Append方法的len参数太大，超出了数组范围！");
            }
            for (int i = 0; i < len / 8; i++)
            {
                Append(vals[i], 8);
            }
            //获取放置在最后一个位在字节的剩余比特数，从最高位开始计数
            int count = len % 8;
            if (count != 0)
            {   //需要将这些位移到最右边才能使用下面的Append方法添加
                byte lastEle = vals[vals.Length - 1];
                lastEle >>= 8 - count;
                Append(lastEle, count);
            }
        }

        /// <summary>
        /// 追加由整数转化而来的字节数组，如果len小于数组的总位数，则从左边
        /// 字节开始裁减多出的位。
        /// </summary>
        /// <param name="vals">要追加的字节数组</param>
        /// <param name="len">占用的比特数</param>
        public void AppendTrimLeft(byte[] vals, int len)
        {

            if ((len / 8 + ((len % 8 == 0) ? 0 : 1)) > vals.Length)
            {
                throw new ArgumentException("Append方法的len参数太大，" +
                    "超出了数组范围！");
            }
            int index = vals.Length - len / 8;
            if ((len % 8) != 0)
            {
                Append(vals[index - 1], len % 8);
            }
            for (; index < vals.Length; index++)
            {
                Append(vals[index], 8);
            }
        }

        /// <summary>
        /// 追加指定字节，如果添加的比特数不足8位，请将数值集中在右侧放置
        /// </summary>
        /// <param name="val">追加的值</param>
        /// <param name="len">占用的比特数，从一个字节的最低位开始计数，
        /// 范围0~8</param>
        public void Append(byte val, int len)
        {
            if (len < 1 || len > 8)
            {
                throw new ArgumentException("Append方法的len参数取值范围" +
                    "应在1~8之间！");
            }
            //首先通过左移将val超出len的左边部分全部清零
            val <<= (8 - len);

            int i = index / 8; //获取当前位置指针所在字节的索引
            int loc = index % 8; //获取位置指针在字节中的位置，从最高位开始计数
            items[i] |= (byte)(val >> loc);
            //如果右移过程中有效位被移出，则放到下一个字节中
            if (len > 8 - loc)
            {
                items[i + 1] |= (byte)(val << (8 - loc));
            }
            index += len;
        }

        /// <summary>
        /// 获取位流中指定范围的子位流，如果长度不是8位对齐，则靠左存放。
        /// </summary>
        /// <param name="begin">子位流在位流中的开始位</param>
        /// <param name="length">子位流长度</param>
        /// <returns></returns>
        public byte[] SubBitToLeft(int begin, int length)
        {
            if (begin + length > items.Length * 8)
            {   //如果拷贝内容超出容器范围，则返回null
                return null;
            }
            byte[] bytes = new byte[length / 8 + ((length % 8 == 0) ? 0 : 1)];
            int sIndex = begin; //指向原数组的位指针
            int dIndex = 0; //指向目的数组的位指针
            while (dIndex < length)
            {   //此时Index必将指向源数组中某一字节的最高位
                //拷贝右半部
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] << (sIndex % 8));
                int len = 8 - (sIndex % 8);
                sIndex += len;
                dIndex += len;
                if (dIndex >= length) break;
                //拷贝左半部
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] >> (dIndex % 8));
                len = 8 - (dIndex % 8);
                sIndex += len;
                dIndex += len;
            }
            if (dIndex >= length)
            {
                int shift = 8 - (length % 8);
                if ((shift % 8) != 0)
                {   //消掉多出来的位
                    bytes[length / 8] = (byte)((bytes[length / 8] >> shift) << shift);
                }
            }
            return bytes;
        }

        /// <summary>
        /// 获取位流中指定范围的子位流，如果长度不是8位对齐，则靠右存放。
        /// </summary>
        /// <param name="begin">子位流在位流中的开始位</param>
        /// <param name="length">子位流长度</param>
        /// <returns></returns>
        public byte[] SubBitToRight(int begin, int length)
        {
            if (begin + length > items.Length * 8)
            {   //如果拷贝内容超出容器范围，返回null
                return null;
            }
            byte[] bytes = new byte[length / 8 + ((length % 8 == 0) ? 0 : 1)];
            int sIndex = begin + length - 1; //指向原数组的位指针
            int dIndex = (length / 8) * 8 + 7; //指向目的数组的位指针
            //从右向左拷贝
            while (sIndex > begin)
            {   //拷贝左半部分
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] >> (7 - (sIndex % 8)));
                int len = sIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
                if (sIndex < begin) break;
                //拷贝右半部分
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] << (7 - (dIndex % 8)));
                len = dIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
            }
            if ((length / 8) * 8 + 7 - dIndex > length)
            {
                int shift = 8 - (length % 8);
                if ((shift % 8) != 0)
                {   //消掉多出来的位
                    bytes[0] = (byte)((byte)(bytes[0] << shift) >> shift);
                }
            }
            return bytes;
        }

        /// <summary>
        /// 获取位流中指定范围的子字节，如果长度不是8位对齐，则靠右存放。
        /// </summary>
        /// <param name="begin">子位流在位流中的开始位</param>
        /// <param name="length">子位流长度，值为1~8之间</param>
        /// <returns></returns>
        public byte SubBitToByte(int begin, int length)
        {
            if (begin + length > items.Length * 8)
            {   //如果拷贝内容超出容器范围，返回null
                throw new ArgumentOutOfRangeException("拷贝内容超出窗口范围。");
            }
            byte b = 0;
            int sIndex = begin + length - 1; //指向原数组的位指针
            int dIndex = 7; //指向目的数组的位指针
            //从右向左拷贝
            while (sIndex > begin)
            {   //拷贝左半部分
                b |= (byte)(items[sIndex / 8] >> (7 - (sIndex % 8)));
                int len = sIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
                if (sIndex < begin) break;
                //拷贝右半部分
                b |= (byte)(items[sIndex / 8] << (7 - (dIndex % 8)));
                len = dIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
            }
            if (length < 8)
            {
                int shift = 8 - (length % 8);
                if ((shift % 8) != 0)
                {   //消掉多出来的位
                    b = (byte)((byte)(b << shift) >> shift);
                }
            }
            return b;
        }

        /// <summary>
        /// 将字节流中的指定位转换为整数
        /// </summary>
        /// <param name="binCode">字节流</param>
        /// <param name="begin">开始位</param>
        /// <param name="length">位长度</param>
        /// <returns>成功则返回转换后的整数，失败返回-1</returns>
        public long SubBitToInteger(int begin, int length)
        {
            if (begin + length > items.Length * 8 || length > 64)
            {   //如果拷贝内容超出容器范围，或拷贝长度超过64，则返回null
                return -1;
            }
            byte[] bytes = new byte[8];
            int sIndex = begin + length - 1; //指向原数组的位指针
            int dIndex = 63; //指向目的数组的位指针
            //从右向左拷贝
            while (sIndex >= begin)
            {   //拷贝左半部分
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] >> (7 - (sIndex % 8)));
                int len = sIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
                if (sIndex < begin) break;
                //拷贝右半部分
                bytes[dIndex / 8] |= (byte)(items[sIndex / 8] << (7 - (dIndex % 8)));
                len = dIndex % 8 + 1;
                sIndex -= len;
                dIndex -= len;
            }
            if (63 - dIndex > length)
            {
                int shift = 8 - (length % 8);
                if ((shift % 8) != 0)
                {   //消掉多出来的位
                    int pos = 7 - length / 8;
                    bytes[pos] = (byte)((byte)(bytes[pos] << shift) >> shift);
                }
            }
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public void Trim()
        {
            int len = index / 8 + ((index % 8 == 0) ? 0 : 1);
            byte[] dest = new byte[len];
            for (int i = 0; i < len; i++)
            {
                dest[i] = items[i];
            }
            items = dest;
        }

        /// <summary>
        /// 获取位流的字节数组表现形式
        /// </summary>
        /// <returns>字节数组</returns>
        public byte[] ToByteArray()
        {
            return items;
        }

        /// <summary>
        /// 打印二进制编码
        /// </summary>
        /// <returns>二进制编码的字符串形式</returns>
        public string ToBinString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in items)
            {
                for (int i = 0x80; i > 0; i >>= 1)
                {
                    if ((b & i) == 0)
                    {
                        sb.Append('0');
                    }
                    else
                    {
                        sb.Append('1');
                    }
                }
                sb.Append(' ');
            }
            return sb.ToString();
        }
        /// <summary>
        /// 打印十六进制编码
        /// </summary>
        /// <returns>十六进制编码的字符串形式</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in items)
            {
                sb.Append(b.ToString("X").PadLeft(2, '0'));
                sb.Append(' ');
            }
            return sb.ToString();
        }
    }
}
