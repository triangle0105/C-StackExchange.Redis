using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestRedis
{
    public class RedisService
    {
        public readonly RedisHelper RedisHelper=new RedisHelper();
        public bool RedisCreate<T>(string folder,List<T> value,List<RedisSearchField> redisSearchFields)
        {
            if (redisSearchFields == null || redisSearchFields.Count<=0) 
                throw new Exception("查询属性不能为空");
            List<RedisSearchField> fields=null;
            var fieldList=RedisHelper.GetKeysContains(folder, "FieldsAttributeFolder");
            if (fieldList == null || fieldList.Count == 0)
            {
                fields = RedisHelper.SetSearchFields(folder, redisSearchFields);
            }
            if (fields != null)
            {
                RedisHelper.SetRedisData(folder, value, fields);
            }
            return true;
        }

        public bool RedisCreate(string folder, DataTable value, List<RedisSearchField> redisSearchFields)
        {
            if (redisSearchFields == null || redisSearchFields.Count <= 0)
                throw new Exception("查询属性不能为空");
            List<RedisSearchField> fields = null;
            var fieldList = RedisHelper.GetKeysContains(folder, "FieldsAttributeFolder");
            if (fieldList == null || fieldList.Count == 0)
            {
                fields = RedisHelper.SetSearchFields(folder, redisSearchFields);
            }
            if (fields != null)
            {
                RedisHelper.SetRedisData(folder, value, fields);
            }
            return true;
        }

        public bool RedisUpdate()
        {
            return false;
        }

        public long DeleteRedisData(string folder)
        {
            return RedisHelper.KeyDelete(folder);
        }

        public bool IfExisstFolder(string folder)
        {
            var keylist= RedisHelper.GetKeysContains(folder, "");
            return keylist.Count>0;
        }

        public List<T> GetList<T>(string folder, string searchText)
        {
            var returnList = RedisDynamicQueryable.Where<T>(folder, searchText, RedisHelper);
            return returnList;
        }
        public DataTable GetDataTable(string folder, string searchText)
        {
            var returnList = RedisDynamicQueryable.Where(folder, searchText, RedisHelper);
            return returnList;
        }
    }
}
