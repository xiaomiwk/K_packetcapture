using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using 项目解码接口;

namespace UI
{
    static class H公共
    {
        public static string 获取过滤表达式(List<M设备映射> __映射列表)
        {
            //(tcp and dst port 80 and dst 202.195.114.89) or (src 202.195.114.117)
            var __过滤条件字符串 = new StringBuilder();
            __映射列表.ForEach(q =>
            {
                if (!q.启用)
                {
                    return;
                }
                if (__过滤条件字符串.Length != 0)
                {
                    __过滤条件字符串.Append(" or ");
                }
                __过滤条件字符串.Append(" ( ");
                __过滤条件字符串.Append(q.TCP ? " tcp " : " udp ");
                if (q.端口1 != 0)
                {
                    __过滤条件字符串.Append(" and src port " + q.端口1.ToString());
                }
                if (!q.IP1.Equals(IPAddress.Any))
                {
                    __过滤条件字符串.Append(" and src " + q.IP1.ToString());
                }
                if (q.端口2 != 0)
                {
                    __过滤条件字符串.Append(" and dst port " + q.端口2.ToString());
                }
                if (!q.IP2.Equals(IPAddress.Any))
                {
                    __过滤条件字符串.Append(" and dst " + q.IP2.ToString());
                }
                __过滤条件字符串.Append(" ) ");
                __过滤条件字符串.Append(" or ");
                __过滤条件字符串.Append(" ( ");
                __过滤条件字符串.Append(q.TCP ? " tcp " : " udp ");
                if (q.端口1 != 0)
                {
                    __过滤条件字符串.Append(" and dst port " + q.端口1.ToString());
                }
                if (!q.IP1.Equals(IPAddress.Any))
                {
                    __过滤条件字符串.Append(" and dst " + q.IP1.ToString());
                }
                if (q.端口2 != 0)
                {
                    __过滤条件字符串.Append(" and src port " + q.端口2.ToString());
                }
                if (!q.IP2.Equals(IPAddress.Any))
                {
                    __过滤条件字符串.Append(" and src " + q.IP2.ToString());
                }
                __过滤条件字符串.Append(" ) ");

            });
            return __过滤条件字符串.ToString();
        }
    }
}
