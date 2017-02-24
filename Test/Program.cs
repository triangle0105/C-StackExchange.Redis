using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRedis;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            RedisHelper redis = new RedisHelper();
            var list1 = new List<Demo>
            {
                new Demo
                {
                    Id = 1,
                    Name = "a"
                },
                new Demo
                {
                    Id = 2,
                    Name = "b"
                },
            };
            var task=redis.ListRightPushAsync<Demo>("folertest1", "keytest", list1);
            //redis.ListRightPush<Demo>("folertest", "keytest", list1[1]);
            //redis.ListLeftPush<Demo>("folertest", "keytest", list1[1]);
            //var task=redis.ListRangeAsync<Demo>("folertest", "keytest");
            var list = task.Result;
        }

        public class Demo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
