using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Utility.存储;

namespace 项目解码接口
{
    [Serializable]
    public class M设备映射
    {
        [DBKey]
        public Int64 Id { get; set; }

        public bool 启用 { get; set; }

        public bool TCP { get; set; }

        public IPAddress IP1 { get; set; }

        /// <summary>
        /// 0表示任意端口
        /// </summary>
        public int 端口1 { get; set; }

        public string 设备类型1 { get; set; }

        public IPAddress IP2 { get; set; }

        /// <summary>
        /// 0表示任意端口
        /// </summary>
        public int 端口2 { get; set; }

        public string 设备类型2 { get; set; }

        public string 协议版本号 { get; set; }

    }
}
