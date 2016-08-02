using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using 项目解码接口;
using SharpPcap;

namespace UI
{
    public class M抓包配置
    {
        public ICaptureDevice 网卡 { get; set; }

        public M项目 项目 { get; set; }

        public bool 录像 { get; set; }

        public M抓包配置()
        {

        }
    }
}
