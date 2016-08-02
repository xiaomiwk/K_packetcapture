using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Utility.存储;
using 项目解码接口;

namespace UI.IBLL
{
    public class B项目
    {
        private const string _路径 = "项目列表.xml";

        private readonly XDocument _XML文档;

        private readonly XElement _根节点;

        private readonly Dictionary<string, I项目> _解码缓存 = new Dictionary<string, I项目>();

        public B项目()
        {
            var __绝对路径 = H路径.获取绝对路径(_路径);
            _XML文档 = XDocument.Load(__绝对路径);
            _根节点 = _XML文档.Root;

        }

        public List<M项目> 获取所有()
        {
            var __结果 = new List<M项目>();
            var __当前目录 = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var __节点列表 = _根节点.Elements("项目");
            foreach (var __节点 in __节点列表)
            {
                var __名称 = __节点.Element("名称").Value;
                var __路径 = __节点.Element("路径").Value;
                var __程序集名称 = __节点.Element("程序集名称").Value;
                var __类型全称 = __节点.Element("类型全称").Value;

                Assembly __程序集 = Assembly.LoadFrom(Path.Combine(__当前目录, __路径 + "\\" + __程序集名称));
                var __类型 = __程序集.GetType(__类型全称);
                if (__类型 == null)
                {
                    continue;
                }
                var __I解码 = (I项目)Activator.CreateInstance(__类型);
                var __项目 = __I解码.获取项目();
                __项目.名称 = __名称;
                __结果.Add(__项目);
                _解码缓存[__名称] = __I解码;
            }
            __结果.Add(构造空项目());
            __结果.Add(构造HTTP项目());
            return __结果;
        }

        public void 保存项目映射(string 项目, List<M设备映射> 当前通信)
        {
            if (_解码缓存.ContainsKey(项目))
            {
                _解码缓存[项目].保存当前映射(当前通信);
            }
        }

        private M项目 构造空项目()
        {
            var __项目 = new M项目
            {
                当前通信设备 = new List<M设备映射>(),
                功能码查询 = (e1, e2, e3) => null,
                解码器 = ((M解码前报文 e1) => new M解码后报文
                {
                    TCP = e1.TCP,
                    发送方 = e1.发送方IP.ToString() + ":" + e1.发送方端口号,
                    发送方设备类型 = "未指定",
                    概述 = "",
                    功能码 = "",
                    接收方 = e1.接收方IP.ToString() + ":" + e1.接收方端口号,
                    接收方设备类型 = "未指定",
                    码流 = e1.码流,
                    时间 = e1.时间,
                    详述 = null,
                    业务 = "未指定",
                }),
                名称 = "空",
                设备类型列表 = new List<string> { "未指定" },
                设备通信模板 = new List<M设备映射>
                {
                    new M设备映射{ IP1 = IPAddress.Any, IP2 = IPAddress.Any, TCP = true, 端口1 = 0, 端口2 = 0, 启用 = true, 设备类型1 = "未指定", 设备类型2 = "未指定"},
                    new M设备映射{ IP1 = IPAddress.Any, IP2 = IPAddress.Any, TCP = false, 端口1 = 0, 端口2 = 0, 启用 = true, 设备类型1 = "未指定", 设备类型2 = "未指定"}
                },
                业务类型列表 = new List<string> { "未指定" }
            };
            __项目.当前通信设备 = new List<M设备映射>(__项目.设备通信模板);
            return __项目;
        }

        private M项目 构造HTTP项目()
        {
            var __项目 = new M项目
            {
                当前通信设备 = new List<M设备映射>(),
                功能码查询 = (e1, e2, e3) => new M功能(),
                名称 = "访问网站",
                设备类型列表 = new List<string> { "服务器", "客户端" },
                设备通信模板 = new List<M设备映射>
                {
                    new M设备映射{ IP1 = IPAddress.Any, IP2 = IPAddress.Any, TCP = true, 端口1 = 80, 端口2 = 0, 启用 = true, 设备类型1 = "服务器", 设备类型2 = "客户端"},
                    new M设备映射{ IP1 = IPAddress.Any, IP2 = IPAddress.Any, TCP = true, 端口1 = 8080, 端口2 = 0, 启用 = true, 设备类型1 = "服务器", 设备类型2 = "客户端"},
                    new M设备映射{ IP1 = IPAddress.Any, IP2 = IPAddress.Any, TCP = true, 端口1 = 8070, 端口2 = 0, 启用 = true, 设备类型1 = "服务器", 设备类型2 = "客户端"},
                },
                业务类型列表 = new List<string> { "" }
            };
            __项目.当前通信设备 = new List<M设备映射>(__项目.设备通信模板);

            __项目.解码器 = ((M解码前报文 e1) =>
            {
                if (__项目.当前通信设备.Count == 0)
                {
                    return null;
                }
                return new M解码后报文
                {
                    TCP = e1.TCP,
                    发送方 = e1.发送方IP + ":" + e1.发送方端口号,
                    发送方设备类型 = "",
                    概述 = "",
                    功能码 = "",
                    接收方 = e1.接收方IP + ":" + e1.接收方端口号,
                    接收方设备类型 = "",
                    码流 = e1.码流,
                    时间 = e1.时间,
                    详述 = new List<M信息> { new M信息 { 键 = "解码", 值 = Encoding.UTF8.GetString(e1.码流) } },
                    业务 = "",
                };
            });
            return __项目;
        }

    }
}
