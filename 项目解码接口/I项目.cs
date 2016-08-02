using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 项目解码接口
{
    public interface I项目
    {
        M项目 获取项目();

        void 保存当前映射(List<M设备映射> 当前通信);
    }
}
