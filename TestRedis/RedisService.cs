using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestRedis
{
    public class RedisService
    {
        public readonly RedisHelper RedisHelper=new RedisHelper();
        public bool RedisSet<T>(string folder,List<T> value,List<RedisSearchField> redisSearchFields)
        {
            if (redisSearchFields == null) return true;
            var fields=RedisHelper.SetSearchFields(folder, redisSearchFields);
            if (fields != null)
            {
                RedisHelper.SetRedisData(folder, value, fields);
            }
            return true;
        }
        
        public string UpdateRedisSearchFields(string folder, List<RedisSearchField> redisSearchFields)
        {
            return null;
        }

        public List<T> GetList<T>(string folder, string searchText)
        {
            var returnList = RedisDynamicQueryable<T>.Where<T>(folder, searchText, RedisHelper);
            return returnList;
        }
    }
}
