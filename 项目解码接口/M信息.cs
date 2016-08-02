using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 项目解码接口
{
    [Serializable]
    public class M信息
    {
        public string 键 { get; set; }

        public string 值 { get; set; }

        public string 描述 { get; set; }

        public M信息 父节点 { get; set; }

        public List<M信息> 子节点 { get; set; }

        public M信息()
        {
            子节点 = new List<M信息>();
        }

        public override string ToString()
        {
            var __详述内容 = new StringBuilder();
            __详述内容.AppendFormat("{0} :  {1}{2}", 键, 值, Environment.NewLine);
            递归详述(子节点, __详述内容, "--------");
            return __详述内容.ToString();
        }

        public static void 递归详述(List<M信息> 子节点, StringBuilder __详述内容, string __缩进)
        {
            子节点.ForEach(q =>
            {
                __详述内容.AppendFormat("{0}{1} :  {2}{3}", __缩进, q.键, q.值, Environment.NewLine);
                if (q.子节点 != null && q.子节点.Count > 0)
                {
                    递归详述(q.子节点, __详述内容, __缩进 + __缩进);
                }
            });
        }
    }
}
