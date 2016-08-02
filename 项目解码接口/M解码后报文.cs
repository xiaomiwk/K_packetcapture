using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 项目解码接口
{
    [Serializable]
    public class M解码后报文
    {
        public DateTime 时间 { get; set; }

        public string 业务 { get; set; }

        public string 发送方 { get; set; }

        public string 发送方设备类型 { get; set; }

        public string 接收方 { get; set; }

        public string 接收方设备类型 { get; set; }

        public string 概述 { get; set; }

        public List<M信息> 详述 { get; set; }

        /// <summary>
        /// 传输层
        /// </summary>
        public byte[] 码流 { get; set; }

        public bool TCP { get; set; }

        public string 功能码 { get; set; }
    }
}
