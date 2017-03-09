
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
            //var test4 = redisservice.GetList<PatientInfo>("PatientInfo", "Age ==20");
            //var test5 = redisservice.GetList<PatientInfo>("PatientInfo", "Company like '医利捷'&&PatientName=='male1'||Age>18");
            //var test6 = redisservice.GetList<PatientInfo>("PatientInfo", "((Age <= 20 && Company like '医利捷')||PatientName=='male2') && PatientName like 'male'");
            //var test3 = redisservice.GetList<PatientInfo>("PatientInfo", "Age == 30");
            var test7 = redisservice.GetList<PatientInfo>("PatientInfo", "Age in (18,25)");

            //var tran = redisservice.RedisHelper.CreateTransaction();
            //tran.StringSetAsync("tran_string", "test1");
            //tran.StringSetAsync("tran_string1", "test2");
            //bool committed = tran.Execute();
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
