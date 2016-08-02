using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace 项目解码接口
{
    /// <summary>
    /// 传输层解码
    /// </summary>
    [Serializable]
    public class M解码前报文
    {
        public bool TCP { get; set; }

        public string 发送方设备类型 { get; set; }

        public IPAddress 发送方IP { get; set; }

        public int 发送方端口号 { get; set; }

        public string 接收方设备类型 { get; set; }

        public IPAddress 接收方IP { get; set; }

        public int 接收方端口号 { get; set; }

        /// <summary>
        /// 传输层
        /// </summary>
        public byte[] 码流 { get; set; }

        public DateTime 时间 { get; set; }

    }
}
