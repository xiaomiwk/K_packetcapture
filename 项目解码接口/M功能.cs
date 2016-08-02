using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 项目解码接口
{
    [Serializable]
    public class M功能
    {
        public string 功能码 { get; set; }

        public string 名称 { get; set; }

        public string 业务类型 { get; set; }

        public List<M信息> 结构 { get; set; }

        public Dictionary<string, string> 字典 { get; set; }

        public M功能()
        {
            结构 = new List<M信息>();
            字典 = new Dictionary<string, string>();
        }
    }
}
