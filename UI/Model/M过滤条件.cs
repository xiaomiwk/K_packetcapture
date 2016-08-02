using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace UI
{
    public class M过滤条件
    {
        public bool TCP { get; set; }

        public IPAddress 发送方IP { get; set; }

        public int? 发送方端口号 { get; set; }

        public IPAddress 接收方IP { get; set; }

        public int? 接收方端口号 { get; set; }
    }
}
