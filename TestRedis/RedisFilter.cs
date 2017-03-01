using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRedis
{
    public class RedisFilter
    {
        public string Folder { get; set; }
        //public string Value{get;set;}
        public List<Keyword> Keywords { get; set; }
    }

    public class Keyword
    {
        public string Name { get; set; }
        public Type KeyType { get; set; }
        public Operation Operation { get; set; }
        public string Value { get; set; }
    }

    public enum Operation
    {
        And,
        Or,
        le,
        lt,
        ge,
        gt,
        eq,
    }


}
