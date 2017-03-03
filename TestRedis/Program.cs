
using System.Configuration;
using System.Globalization;
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
        private static void Main(string[] args)
        {
            var redisservice = new RedisService();
            var paitients = InitialPatientInfos();
            var redisSearchField = new List<RedisSearchField>
            {
                new RedisSearchField{Name = "VisitNumber",TypeCode = TypeCode.String},
                new RedisSearchField{Name = "Age",TypeCode = TypeCode.Int32},
                new RedisSearchField{Name = "PatientName",TypeCode = TypeCode.String},
                new RedisSearchField{Name = "Company",TypeCode = TypeCode.String},
                new RedisSearchField{Name = "IsMale",TypeCode = TypeCode.Boolean},
                //new RedisSearchField{Name = "BirthDay",TypeCode = TypeCode.DateTime},
            };

            //redisservice.RedisSet("PatientInfo", paitients, redisSearchField);

            //var test1 = redisservice.GetList<PatientInfo>("PatientInfo", "Age > 20 && Company like '医利捷'");
            //var test2 = redisservice.GetList<PatientInfo>("PatientInfo", "(Age >= 20 && Company like '医利捷')||(Age == 18)");
            //var test3 = redisservice.GetList<PatientInfo>("PatientInfo", "Age =='12345678'");
            //var test4 = redisservice.GetList<PatientInfo>("PatientInfo", "Age ==20");
            //var test5 = redisservice.GetList<PatientInfo>("PatientInfo", "Company like '医利捷'&&PatientName=='male1'||Age>18");
            //var test2 = redisservice.GetList<PatientInfo>("PatientInfo", "((Age <= 20 && Company like '医利捷')||PatientName=='male2') && PatientName like 'male'");

            var test3 = redisservice.GetList<PatientInfo>("PatientInfo", "IsMale == true");
        }
            
        private static List<PatientInfo> InitialPatientInfos()
        {
            var patientInfos = new List<PatientInfo>();
                patientInfos.Add(new PatientInfo
                {
                    Id = Guid.NewGuid(),
                    VisitNumber = (12345678).ToString(CultureInfo.InvariantCulture),
                    PatientId = (234567).ToString(CultureInfo.InvariantCulture),
                    PatientName = "male1",
                    Gender = 1,
                    Age = 25,
                    IsMale = false,
                    BirthDay = DateTime.Now.AddDays(-2),
                    ICD = "4564646546156516165156",
                    FamilyPhone = "1111111111",
                    ContactPhone = "18615516481",
                    ContactAddress = "中华人民共和国上海市",
                    Hkadr = "中华人民共和国上海市",
                    CurrentAddress = "中华人民共和国上海市",
                    Profession = "profession1",
                    Company = "医利捷信息科技有限公司",
                    CreatorCode = "0001",
                    CreatorName = "admin",
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDelete = false,
                    UpdatorCode = "0001",
                    UpdatorName = "admin",
                }); 
                patientInfos.Add(new PatientInfo
                {
                    Id = Guid.NewGuid(),
                    VisitNumber = (12345699).ToString(CultureInfo.InvariantCulture),
                    PatientId = (234566).ToString(CultureInfo.InvariantCulture),
                    PatientName = "male2",
                    Gender = 1,
                    Age = 20,
                    IsMale = true,
                    BirthDay = DateTime.Now.AddDays(-5),
                    ICD = "456462664651615555",
                    FamilyPhone = "2222222222",
                    ContactPhone = "18615516561",
                    ContactAddress = "中华人民共和国上海市",
                    Hkadr = "中华人民共和国上海市",
                    CurrentAddress = "中华人民共和国上海市",
                    Profession = "profession1",
                    Company = "斯迈康信息科技有限公司",
                    CreatorCode = "0001",
                    CreatorName = "admin",
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDelete = false,
                    UpdatorCode = "0001",
                    UpdatorName = "admin",
                });
                patientInfos.Add(new PatientInfo
                {
                    Id = Guid.NewGuid(),
                    VisitNumber = (12421699).ToString(CultureInfo.InvariantCulture),
                    PatientId = (267866).ToString(CultureInfo.InvariantCulture),
                    PatientName = "female1",
                    Gender = 0,
                    Age = 18,
                    IsMale = true,
                    BirthDay = DateTime.Now,
                    ICD = "45648948951615555",
                    FamilyPhone = "333333333333",
                    ContactPhone = "18615432561",
                    ContactAddress = "中华人民共和国上海市",
                    Hkadr = "中华人民共和国上海市",
                    CurrentAddress = "中华人民共和国上海市",
                    Profession = "profession1",
                    Company = "医利捷信息科技有限公司",
                    CreatorCode = "0001",
                    CreatorName = "admin",
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDelete = false,
                    UpdatorCode = "0001",
                    UpdatorName = "admin",
                });
            //var list = patientInfos.Where(n => n.Age == 1);
            return patientInfos;
        }

        //static void Main(string[] args)
        //{

        //    RedisHelper redisdbHelper = new RedisHelper();
        //    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        //    IDatabase db = redis.GetDatabase();

        //    string value1 = "abcdefg";
        //    db.StringSet("mykey", value1);

        //    var info = db.StringGet("mykey");
        //    Console.WriteLine(info); // writes: "abcdefg"

            //ISubscriber sub = redis.GetSubscriber();
            //sub.Subscribe("messages", (channel, message) =>
            //{
            //    Console.WriteLine((string)message);
            //});
            //sub.Publish("messages", "hello");

            //// sliding expiration
            //db.KeyExpire("mykey", TimeSpan.FromSeconds(0), flags: CommandFlags.FireAndForget);
            //var keylist = new List<string>
            //{
            //    "folertest1:keytest",
            //    "folertest:keytest"
            //};
            //var list = redisdbHelper.StringGetAsync("", keylist);
            //var value = (string)db.StringGet("mykey");
            //foreach (var key in redisdbHelper.GetServer().Keys(pattern: "*ke*"))
            //{
            //    Console.WriteLine(key);
            //}
            //Console.WriteLine(value);
            //Console.ReadKey();


            //RedisHelper redis = new RedisHelper();

            //#region String

            //string str = "123";
            //Demo demo = new Demo()
            //{
            //    Id = 1,
            //    Name = "123"
            //};
            //var resukt = redis.StringSet("Folder1","redis_string_test", str);
            //var str1 = redis.StringGet("Folder1", "redis_string_test");
            //redis.StringSet("Folder1", "redis_string_model", demo);
            //var model = redis.StringGet<Demo>("Folder1", "redis_string_model");

            //for (int i = 0; i < 10; i++)
            //{
            //    redis.StringIncrement("Folder1", "StringIncrement", 2);
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    redis.StringDecrement("Folder1", "StringIncrement");
            //}
            //redis.StringSet("Folder1", "redis_string_model1", demo, TimeSpan.FromSeconds(10));

            //#endregion String

            //#region List

            //for (int i = 0; i < 10; i++)
            //{
            //    redis.ListRightPush("Folder1", "list", i);
            //}

            //for (int i = 10; i < 20; i++)
            //{
            //    redis.ListLeftPush("Folder1", "list", i);
            //}
            //var length = redis.ListLength("Folder1", "list");

            //var leftpop = redis.ListLeftPop<string>("Folder1", "list");
            //var rightPop = redis.ListRightPop<string>("Folder1", "list");

            //var list = redis.ListRange<int>("Folder1", "list");

            //#endregion List

            //#region Hash

            //redis.HashSet("Folder1", "user", "u1", "123");
            //redis.HashSet("Folder1", "user", "u2", "1234");
            //redis.HashSet("Folder1", "user", "u3", "1235");
            //var news = redis.HashGet<string>("Folder1", "user", "u2");

            //#endregion Hash

            //#region 发布订阅

            //redis.Subscribe("Channel1");
            //for (int i = 0; i < 10; i++)
            //{
            //    redis.Publish("Channel1", "msg" + i);
            //    if (i == 2)
            //    {
            //        redis.Unsubscribe("Channel1");
            //    }
            //}

            //#endregion 发布订阅

            //#region 事务

            //var tran = redis.CreateTransaction();

            //tran.StringSetAsync("tran_string", "test1");
            //tran.StringSetAsync("tran_string1", "test2");
            //bool committed = tran.Execute();

            //#endregion 事务

            //#region Lock

            //var db = redis.GetDatabase();
            //RedisValue token = Environment.MachineName;
            //if (db.LockTake("lock_test", token, TimeSpan.FromSeconds(10)))
            //{
            //    try
            //    {
            //        //TODO:开始做你需要的事情
            //        Thread.Sleep(5000);
            //    }
            //    finally
            //    {
            //        db.LockRelease("lock_test", token);
            //    }
            //}

            //#endregion Lock
        //}
    }
    public class Demo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PatientInfo
    {
        public Guid Id { get; set; }
        public string VisitNumber { get; set; }
        public string PatientId { get; set; }
        public string AdmissionNumber { get; set; }
        public string PatientName { get; set; }
        public int Gender { get; set; }
        public float? Age { get; set; }
        public bool IsMale { get; set; }
        public DateTime? BirthDay { get; set; }
        public string ICD { get; set; }
        public string FamilyPhone { get; set; }
        public string ContactPhone { get; set; }
        public string ContactAddress { get; set; }
        public string Hkadr { get; set; }
        public string CurrentAddress { get; set; }
        public string Profession { get; set; }
        public string Company { get; set; }
        public string CreatorCode { get; set; }
        public string CreatorName { get; set; }
        public string UpdatorCode { get; set; }
        public string UpdatorName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public bool IsDelete { get; set; }
    }
}
