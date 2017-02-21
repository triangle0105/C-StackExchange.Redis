﻿
using System.Threading;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRedis
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            //IDatabase db = redis.GetDatabase();

            //string value = "abcdefg";
            //db.StringSet("mykey", value);

            //var info = db.StringGet("mykey");
            //Console.WriteLine(info); // writes: "abcdefg"

            //Console.ReadKey();


            RedisHelper redis = new RedisHelper(1);

            #region String

            string str = "123";
            Demo demo = new Demo()
            {
                Id = 1,
                Name = "123"
            };
            var resukt = redis.StringSet("redis_string_test", str);
            var str1 = redis.StringGet("redis_string_test");
            redis.StringSet("redis_string_model", demo);
            var model = redis.StringGet<Demo>("redis_string_model");

            for (int i = 0; i < 10; i++)
            {
                redis.StringIncrement("StringIncrement", 2);
            }
            for (int i = 0; i < 10; i++)
            {
                redis.StringDecrement("StringIncrement");
            }
            redis.StringSet("redis_string_model1", demo, TimeSpan.FromSeconds(10));

            #endregion String

            #region List

            for (int i = 0; i < 10; i++)
            {
                redis.ListRightPush("list", i);
            }

            for (int i = 10; i < 20; i++)
            {
                redis.ListLeftPush("list", i);
            }
            var length = redis.ListLength("list");

            var leftpop = redis.ListLeftPop<string>("list");
            var rightPop = redis.ListRightPop<string>("list");

            var list = redis.ListRange<int>("list");

            #endregion List

            #region Hash

            redis.HashSet("user", "u1", "123");
            redis.HashSet("user", "u2", "1234");
            redis.HashSet("user", "u3", "1235");
            var news = redis.HashGet<string>("user", "u2");

            #endregion Hash

            #region 发布订阅

            redis.Subscribe("Channel1");
            for (int i = 0; i < 10; i++)
            {
                redis.Publish("Channel1", "msg" + i);
                if (i == 2)
                {
                    redis.Unsubscribe("Channel1");
                }
            }

            #endregion 发布订阅

            #region 事务

            var tran = redis.CreateTransaction();

            tran.StringSetAsync("tran_string", "test1");
            tran.StringSetAsync("tran_string1", "test2");
            bool committed = tran.Execute();

            #endregion 事务

            #region Lock

            var db = redis.GetDatabase();
            RedisValue token = Environment.MachineName;
            if (db.LockTake("lock_test", token, TimeSpan.FromSeconds(10)))
            {
                try
                {
                    //TODO:开始做你需要的事情
                    Thread.Sleep(5000);
                }
                finally
                {
                    db.LockRelease("lock_test", token);
                }
            }

            #endregion Lock
        }
    }
    public class Demo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
