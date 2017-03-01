using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestRedis;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var redishelper = new RedisHelper();
            var list = InitialData();
            foreach (var patientInfo in list)
            {
                var result = redishelper.StringSetAsync("testfolderAsync", "testkey" + "_" + patientInfo.VisitNumber + "_" + patientInfo.Age, patientInfo);
            }
            var json = @"{
                          'Id': '006083e2-79b7-4dd9-9f2e-a36b506eafcd',
                          'VisitNumber': '12379534',
                          'PatientId': '23795',
                          'AdmissionNumber': null,
                          'PatientName': 'b0bdebb0-4ecd-418a-9132-aad75a1e5fe0',
                          'Gender': 0,
                          'Age': 25.0,
                          'Nation': '中华任命共和国',
                          'BirthDay': '2017-03-01T10:29:11.9380125+08:00',
                          'ICD': '4564646546156516165156',
                          'FamilyPhone': '4561646516',
                          'ContactPhone': '18615516481',
                          'ContactAddress': '中华人民共和国上海市',
                          'Hkadr': '中华人民共和国上海市',
                          'CurrentAddress': '中华人民共和国上海市',
                          'Profession': 'profession1',
                          'Company': '医利捷信息科技有限公司',
                          'CreatorCode': '0001',
                          'CreatorName': 'admin',
                          'UpdatorCode': '0001',
                          'UpdatorName': 'admin',
                          'CreateTime': '2017-03-01T10:29:11.9380125+08:00',
                          'UpdateTime': '2017-03-01T10:29:11.9380125+08:00',
                          'IsDelete': false
                        }";
            var removePa = JsonConvert.DeserializeObject<PatientInfo>(json);
            //redishelper.HashSet("test", "PatientInfo", removePa.VisitNumber, removePa);
            //var dic = list.ToLookup(n => n.VisitNumber, n => n).ToDictionary(n => n.Key, n => n.First()).ToList();
            //var result = redishelper.StringSet("keyvaluepair", dic);
            //redishelper.ListRemove("testfolder", "testkey", removePa);
            //var listresult = redishelper.ListRangeAsync<PatientInfo>("testfolder", "testkey");
            //var filterpatientlist = listresult.Result.Where(n => n.VisitNumber == "123795");
        }

        public static List<PatientInfo> InitialData()
        {
            var patientInfos = new List<PatientInfo>();
            for (int i = 0; i < 200000; i++)
            {
                patientInfos.Add(new PatientInfo
                {
                    Id=Guid.NewGuid(),
                    VisitNumber = (i+123456).ToString(CultureInfo.InvariantCulture),
                    PatientId = (i + 23456).ToString(CultureInfo.InvariantCulture),
                    PatientName = Guid.NewGuid().ToString(),
                    Gender = i/2==0?1:0,
                    Age=25,
                    Nation = "中华任命共和国",
                    BirthDay = DateTime.Now,
                    ICD = "4564646546156516165156",
                    FamilyPhone = "4561646516",
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
            }
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
        public string Nation { get; set; }
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
