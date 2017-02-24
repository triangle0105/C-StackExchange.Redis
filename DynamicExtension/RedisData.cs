using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicExtension
{
    public class RedisData<T>
    {
        //Redis文件夹 上级目录与子目录用:分隔
        public string Folder { get; set; }
        public string Key { get; set; }
        public T Data { get; set; }
        

    }
}
