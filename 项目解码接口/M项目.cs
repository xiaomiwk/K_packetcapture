using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utility.存储;

namespace 项目解码接口
{
    [Serializable]
    public class M项目
    {
        [DBKey]
        public Int64 Id { get; set; }

        public string 名称 { get; set; }

        public List<M设备映射> 设备通信模板 { get; set; }

        public List<M设备映射> 当前通信设备 { get; set; }

        public List<string> 业务类型列表 { get; set; }

        public List<string> 设备类型列表 { get; set; }

        public Func<M解码前报文, M解码后报文> 解码器 { get; set; }

        /// <summary>
        /// 参数依次为: 发送方设备类型, 接收方设备类型, 功能码
        /// </summary>
        public Func<string, string, string, M功能> 功能码查询 { get; set; }

        public List<string> 默认内容过滤 { get; set; }

        public List<string> 默认业务过滤 { get; set; }

        public M项目()
        {
            this.默认内容过滤 = new List<string>();
            this.默认业务过滤 = new List<string>();
        }

        public override string ToString()
        {
            return 名称;
        }
    }
}
