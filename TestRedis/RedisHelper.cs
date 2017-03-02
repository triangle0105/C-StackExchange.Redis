using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace TestRedis
{
    public class RedisHelper
    {
        private int DbNum { get; set; }
        private readonly ConnectionMultiplexer _conn;

        #region 构造函数

        public RedisHelper(int dbNum = 0)
            : this(dbNum, null)
        {
        }

        public RedisHelper(int dbNum, string readWriteHosts)
        {
            DbNum = dbNum;
            _conn =
                string.IsNullOrWhiteSpace(readWriteHosts) ?
                RedisConnection.Instance :
                RedisConnection.GetConnectionMultiplexer(readWriteHosts);
        }

        

        #endregion 构造函数

        #region String

        #region 同步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet(string folder ,string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            return Do(db => db.StringSet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="keyValues">键值对</param>
        /// <returns></returns>
        public bool StringSet(string folder, List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
                keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>((string.IsNullOrEmpty(folder) ? p.Key : (RedisKey) (folder + ":" + p.Key)), p.Value)).ToList();
            return Do(db => db.StringSet(newkeyValues.ToArray()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T>(string folder, string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            string json = ConvertJson(obj);
            return Do(db => db.StringSet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public string StringGet(string folder, string key)
        {
            return Do(db => db.StringGet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public RedisValue[] StringGet(string folder, List<string> listKey)
        {
            List<string> newKeys = listKey.Select(n=>string.IsNullOrEmpty(folder) ? n :folder+":"+n).ToList();
            return Do(db => db.StringGet(ConvertRedisKeys(newKeys)));
        }

        public List<T> StringGetToObj<T>(string folder, List<string> listKey)
        {
            var result = new List<T>();
            var keyValues = _conn.GetDatabase().StringGet(ConvertRedisKeys(listKey));
            foreach (var redisValue in keyValues)
            {
                if (redisValue.HasValue && !redisValue.IsNullOrEmpty)
                {
                    var item = JsonConvert.DeserializeObject<T>(redisValue);
                    result.Add(item);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string folder, string key)
        {
            return Do(db => ConvertObj<T>(db.StringGet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key)));
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement(string folder, string key, double val = 1)
        {
            return Do(db => db.StringIncrement(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string folder, string key, double val = 1)
        {
            return Do(db => db.StringDecrement(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, val));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync(string folder, string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            return await Do(db => db.StringSetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="keyValues">键值对</param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync(string folder, List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
                keyValues.Select(p => new KeyValuePair<RedisKey, RedisValue>(string.IsNullOrEmpty(folder) ? p.Key : folder + ":" + p.Key, p.Value)).ToList();
            return await Do(db => db.StringSetAsync(newkeyValues.ToArray()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> StringSetAsync<T>(string folder, string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            string json = ConvertJson(obj);
            return await Do(db => db.StringSetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public async Task<string> StringGetAsync(string folder, string key)
        {
            return await Do(db => db.StringGetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public async Task<RedisValue[]> StringGetAsync(string folder, List<string> listKey)
        {
            List<string> newKeys = listKey.Select(n => string.IsNullOrEmpty(folder) ? n : folder + ":" + n).ToList();
            return await Do(db => db.StringGetAsync(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> StringGetAsync<T>(string folder, string key)
        {
            string result = await Do(db => db.StringGetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvertObj<T>(result);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="folder">redis文件夹</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public async Task<double> StringIncrementAsync(string folder, string key, double val = 1)
        {
            return await Do(db => db.StringIncrementAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> StringDecrementAsync(string folder, string key, double val = 1)
        {
            return await Do(db => db.StringDecrementAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, val));
        }

        #endregion 异步方法

        #endregion String

        #region List

        #region 同步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRemove<T>(string folder, string key, T value)
        {
            Do(db => db.ListRemove(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string folder, string key)
        {
            return Do(redis =>
            {
                var values = redis.ListRange(string.IsNullOrEmpty(folder) ? key : folder + ":" + key);
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush<T>(string folder, string key, T value)
        {
            Do(db => db.ListRightPush(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string folder, string key)
        {
            return Do(db =>
            {
                var value = db.ListRightPop(string.IsNullOrEmpty(folder) ? key : folder + ":" + key);
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush<T>(string folder, string key, T value)
        {
            Do(db => db.ListLeftPush(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string folder, string key)
        {
            return Do(db =>
            {
                var value = db.ListLeftPop(string.IsNullOrEmpty(folder) ? key : folder + ":" + key);
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string folder, string key)
        {
            return Do(redis => redis.ListLength(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRemoveAsync<T>(string folder, string key, T value)
        {
            return await Do(db => db.ListRemoveAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> ListRangeAsync<T>(string folder, string key)
        {
            var values = await Do(redis => redis.ListRangeAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRightPushAsync<T>(string folder, string key, T value)
        {
            return await Do(db => db.ListRightPushAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T>(string folder, string key)
        {
            var value = await Do(db => db.ListRightPopAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListLeftPushAsync<T>(string folder, string key, T value)
        {
            return await Do(db => db.ListLeftPushAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T>(string folder, string key)
        {
            var value = await Do(db => db.ListLeftPopAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync(string folder, string key)
        {
            return await Do(redis => redis.ListLengthAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        #endregion 异步方法

        #endregion List

        #region Hash

        #region 同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists(string folder, string key, string dataKey)
        {
            return Do(db => db.HashExists(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string folder, string key, string dataKey, T t)
        {
            return Do(db =>
            {
                string json = ConvertJson(t);
                return db.HashSet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, json);
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string folder, string key, string dataKey)
        {
            return Do(db => db.HashDelete(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete(string folder, string key, List<RedisValue> dataKeys)
        {
            //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
            return Do(db => db.HashDelete(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string folder, string key, string dataKey)
        {
            return Do(db =>
            {
                string value = db.HashGet(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey);
                return ConvertObj<T>(value);
            });
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double HashIncrement(string folder, string key, string dataKey, double val = 1)
        {
            return Do(db => db.HashIncrement(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double HashDecrement(string folder, string key, string dataKey, double val = 1)
        {
            return Do(db => db.HashDecrement(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashKeys<T>(string folder, string key)
        {
            return Do(db =>
            {
                RedisValue[] values = db.HashKeys(string.IsNullOrEmpty(folder) ? key : folder + ":" + key);
                return ConvetList<T>(values);
            });
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync(string folder, string key, string dataKey)
        {
            return await Do(db => db.HashExistsAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T>(string folder, string key, string dataKey, T t)
        {
            return await Do(db =>
            {
                string json = ConvertJson(t);
                return db.HashSetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, json);
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync(string folder, string key, string dataKey)
        {
            return await Do(db => db.HashDeleteAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public async Task<long> HashDeleteAsync(string folder, string key, List<RedisValue> dataKeys)
        {
            //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
            return await Do(db => db.HashDeleteAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKeys.ToArray()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<T> HashGeAsync<T>(string folder, string key, string dataKey)
        {
            string value = await Do(db => db.HashGetAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey));
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public async Task<double> HashIncrementAsync(string folder, string key, string dataKey, double val = 1)
        {
            return await Do(db => db.HashIncrementAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> HashDecrementAsync(string folder, string key, string dataKey, double val = 1)
        {
            return await Do(db => db.HashDecrementAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashKeysAsync<T>(string folder, string key)
        {
            RedisValue[] values = await Do(db => db.HashKeysAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvetList<T>(values);
        }

        #endregion 异步方法

        #endregion Hash

        #region SortedSet 有序集合

        #region 同步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public bool SortedSetAdd<T>(string folder, string key, T value, double score)
        {
            return Do(redis => redis.SortedSetAdd(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson<T>(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SortedSetRemove<T>(string folder, string key, T value)
        {
            return Do(redis => redis.SortedSetRemove(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T>(string folder, string key)
        {
            return Do(redis =>
            {
                var values = redis.SortedSetRangeByRank(string.IsNullOrEmpty(folder) ? key : folder + ":" + key);
                return ConvetList<T>(values);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string folder, string key)
        {
            return Do(redis => redis.SortedSetLength(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public async Task<bool> SortedSetAddAsync<T>(string folder, string key, T value, double score)
        {
            return await Do(redis => redis.SortedSetAddAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson<T>(value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<bool> SortedSetRemoveAsync<T>(string folder, string key, T value)
        {
            return await Do(redis => redis.SortedSetRemoveAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, ConvertJson(value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string folder, string key)
        {
            var values = await Do(redis => redis.SortedSetRangeByRankAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
            return ConvetList<T>(values);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync(string folder, string key)
        {
            return await Do(redis => redis.SortedSetLengthAsync(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        #endregion 异步方法

        #endregion SortedSet 有序集合

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string folder, string key)
        {
            return Do(db => db.KeyDelete(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(string folder, List<string> keys)
        {
            List<string> newKeys = keys.Select(n => string.IsNullOrEmpty(folder) ? n : folder + ":" + n).ToList();
            return Do(db => db.KeyDelete(ConvertRedisKeys(newKeys)));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists(string folder, string key)
        {
            return Do(db => db.KeyExists(string.IsNullOrEmpty(folder) ? key : folder + ":" + key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename(string folder, string key, string newKey)
        {
            return Do(db => db.KeyRename(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string folder, string key, TimeSpan? expiry = default(TimeSpan?))
        {
            return Do(db => db.KeyExpire(string.IsNullOrEmpty(folder) ? key : folder + ":" + key, expiry));
        }

        /// <summary>
        /// 获取模糊查询的keylist
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="keyvalue">redis key</param>
        /// <returns></returns>
        public List<string> GetKeysContains(string folder, string keyvalue)
        {
            var keyList = new List<string>();
            var server = _conn.GetServer(RedisConnection.HostAndPort);
            keyList.AddRange(server.Keys(pattern: string.Format("{0}*{1}*",folder, keyvalue)).Select(key => (string)key));
            return keyList;
        }


        /// <summary>
        /// 根据folder，字段名查找对应条件的keys
        /// </summary>
        /// <param name="folder">redis文件夹</param>
        /// <param name="expressionLeft">redis key</param>
        /// <param name="expressionRight"></param>
        /// <param name="relationSymbol"></param>
        /// <returns></returns>
        public List<string> GetKeysSearch(string folder, string expressionLeft, string expressionRight, RelationOperator relationSymbol)
        {
            var fieldFolder = folder + ":FieldsAttributeFolder";
            var fieldlist = GetKeysContains(fieldFolder, expressionLeft).ToList();
            var searchFieldlist = StringGetToObj<RedisSearchField>(folder, fieldlist);
            var resultList = new List<string>();
            //获取当前查询字段属性
            var currentProp=searchFieldlist.FirstOrDefault(n => n.Name == expressionLeft);
            if (relationSymbol != RelationOperator.Empty)
            {
                switch (relationSymbol)
                {
                    case RelationOperator.Like:
                        resultList = GetKeysContains(folder, expressionRight);
                        break;
                    case RelationOperator.Eq:
                        if (currentProp != null)
                            resultList = ValueCompareList(currentProp, GetKeysContains(folder, expressionLeft), relationSymbol, expressionRight);
                        break;
                    case RelationOperator.NotEq:
                        if (currentProp != null)
                            resultList = ValueCompareList(currentProp, GetKeysContains(folder, expressionLeft), relationSymbol, expressionRight);
                        break;
                    default:
                        if (currentProp != null)
                            resultList = ValueCompareList(currentProp, GetKeysContains(folder, expressionLeft), relationSymbol, expressionRight);
                        break;
                }
            }
            return resultList;
        }

        private List<string> LikeCompareList(RedisSearchField field, IEnumerable<string> keyList, RelationOperator relationOperator, string rightValue)
        {
            var result = new List<string>();
            foreach (var key in keyList)
            {
                var lastOrDefault = key.Split(':').LastOrDefault();
                if (lastOrDefault != null)
                {
                    var currValue = lastOrDefault.Split('_')[field.Index - 1];
                    switch (field.TypeCode)
                    {
                        case TypeCode.String:
                            break;
                    }
                }
            }
            return null;
        }

        private List<string> ValueCompareList(RedisSearchField field, IEnumerable<string> keyList, RelationOperator relationOperator, string rightValue)
        {
            var result = new List<string>();
            foreach (var key in keyList)
            {
                var lastOrDefault = key.Split(':').LastOrDefault();
                if (lastOrDefault != null)
                {
                    var currValue = lastOrDefault.Split('_')[field.Index - 1];
                    #region 判断属性类型进行比较
                    switch (field.TypeCode)
                    {
                        case TypeCode.Byte:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Byte.Parse(currValue) > Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Byte.Parse(currValue) >= Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Byte.Parse(currValue) < Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Byte.Parse(currValue) <= Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Byte.Parse(currValue) == Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Byte.Parse(currValue) != Byte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Boolean:
                            switch (relationOperator)
                            {
                                case RelationOperator.Eq:
                                    if (Boolean.Parse(currValue) == Boolean.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Boolean.Parse(currValue) != Boolean.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Char:
                            switch (relationOperator)
                            {
                                case RelationOperator.Eq:
                                    if (Char.Parse(currValue) == Char.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Char.Parse(currValue) != Char.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.String:
                            switch (relationOperator)
                            {
                                case RelationOperator.Eq:
                                    if (currValue == rightValue)
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (currValue != rightValue)
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.DBNull:
                            break;
                        case TypeCode.DateTime:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (DateTime.Parse(currValue) > DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (DateTime.Parse(currValue) >= DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (DateTime.Parse(currValue) < DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (DateTime.Parse(currValue) <= DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (DateTime.Parse(currValue) == DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (DateTime.Parse(currValue) != DateTime.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Decimal:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Decimal.Parse(currValue) > Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Decimal.Parse(currValue) >= Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Decimal.Parse(currValue) < Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Decimal.Parse(currValue) <= Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Decimal.Parse(currValue) == Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Decimal.Parse(currValue) != Decimal.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Double:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Double.Parse(currValue) > Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Double.Parse(currValue) >= Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Double.Parse(currValue) < Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Double.Parse(currValue) <= Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Double.Parse(currValue) == Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Double.Parse(currValue) != Double.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Empty:
                            break;
                        case TypeCode.Int16:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Int16.Parse(currValue) > Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Int16.Parse(currValue) >= Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Int16.Parse(currValue) < Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Int16.Parse(currValue) <= Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Int16.Parse(currValue) == Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Int16.Parse(currValue) != Int16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Int32:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Int32.Parse(currValue) > Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Int32.Parse(currValue) >= Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Int32.Parse(currValue) < Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Int32.Parse(currValue) <= Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Int32.Parse(currValue) == Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Int32.Parse(currValue) != Int32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Int64:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Int64.Parse(currValue) > Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Int64.Parse(currValue) >= Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Int64.Parse(currValue) < Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Int64.Parse(currValue) <= Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Int64.Parse(currValue) == Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Int64.Parse(currValue) != Int64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Object:
                            break;
                        case TypeCode.SByte:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (SByte.Parse(currValue) > SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (SByte.Parse(currValue) >= SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (SByte.Parse(currValue) < SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (SByte.Parse(currValue) <= SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (SByte.Parse(currValue) == SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (SByte.Parse(currValue) != SByte.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.Single:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (Single.Parse(currValue) > Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (Single.Parse(currValue) >= Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (Single.Parse(currValue) < Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (Single.Parse(currValue) <= Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (Single.Parse(currValue) == Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (Single.Parse(currValue) != Single.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.UInt16:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (UInt16.Parse(currValue) > UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (UInt16.Parse(currValue) >= UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (UInt16.Parse(currValue) < UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (UInt16.Parse(currValue) <= UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (UInt16.Parse(currValue) == UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (UInt16.Parse(currValue) != UInt16.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.UInt32:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (UInt32.Parse(currValue) > UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (UInt32.Parse(currValue) >= UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (UInt32.Parse(currValue) < UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (UInt32.Parse(currValue) <= UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (UInt32.Parse(currValue) == UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (UInt32.Parse(currValue) != UInt32.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                        case TypeCode.UInt64:
                            switch (relationOperator)
                            {
                                case RelationOperator.Ge:
                                    if (UInt64.Parse(currValue) > UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Gt:
                                    if (UInt64.Parse(currValue) >= UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Le:
                                    if (UInt64.Parse(currValue) < UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Lt:
                                    if (UInt64.Parse(currValue) <= UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.Eq:
                                    if (UInt64.Parse(currValue) == UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                                case RelationOperator.NotEq:
                                    if (UInt64.Parse(currValue) != UInt64.Parse(rightValue))
                                        result.Add(key);
                                    break;
                            }
                            break;
                    }
                    #endregion
                }
            }
            return result;
        }

        #endregion key

        #region 发布订阅

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long Publish<T>(string channel, T msg)
        {
            ISubscriber sub = _conn.GetSubscriber();
            return sub.Publish(channel, ConvertJson(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        /// <summary>
        /// Redis发布订阅  取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion 发布订阅

        #region 其他

        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        public IDatabase GetDatabase()
        {
            return _conn.GetDatabase(DbNum);
        }

        public IServer GetServer()
        {
            return _conn.GetServer(RedisConnection.HostAndPort);
        }

        #endregion 其他

        #region 辅助方法
        
        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(DbNum);
            return func(database);
        }

        private string ConvertJson<T>(T value)
        {
            string result = value is string ? value.ToString() : JsonConvert.SerializeObject(value);
            return result;
        }

        private T ConvertObj<T>(RedisValue value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        private List<T> ConvetList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = ConvertObj<T>(item);
                result.Add(model);
            }
            return result;
        }

        private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }

        public List<RedisSearchField> SetSearchFields(string folder, List<RedisSearchField> redisSearchFields)
        {
            try
            {
                if (redisSearchFields != null)
                {
                    for (int i = 1; i <= redisSearchFields.Count; i++)
                    {
                        redisSearchFields[i - 1].Index = i;
                        var currentKey = string.Format("FieldsAttributeFolder:{0}", redisSearchFields[i - 1].Name);
                        // ReSharper disable once CSharpWarnings::CS4014
                        StringSetAsync(folder, currentKey, redisSearchFields[i - 1]);
                    }
                    return redisSearchFields.OrderBy(n=>n.Index).ToList();
                }
                else
                {
                    throw new Exception("查询字段配置数据不能为空");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool SetRedisData<T>(string folder, List<T> value, List<RedisSearchField> redisSearchFields)
        {
            try
            {
                foreach (var item in value)
                {
                    if (redisSearchFields != null)
                    {
                        var currentKey = "";
                        var attrList = redisSearchFields.OrderBy(n => n.Index).ToList();
                        foreach (var source in attrList)
                        {
                            currentKey = attrList.IndexOf(source) == 0 ? GetObjPropValue(item, source.Name) : currentKey + "_" + GetObjPropValue(item, source.Name);
                        }
                        // ReSharper disable once CSharpWarnings::CS4014
                        StringSetAsync(folder, currentKey, item);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string GetObjPropValue<T>(T t, string propName)
        {
            foreach (PropertyInfo p in t.GetType().GetProperties())
            {
                if (p.Name.ToLower() == propName.ToLower())
                {
                    return p.GetValue(t).ToString();
                }
            }
            return null;
        }

        #endregion 辅助方法


    }
}
