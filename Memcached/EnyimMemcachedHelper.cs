using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Protocol;
using Enyim.Caching.Memcached.Protocol.Text;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached.Results.Extensions;
using Enyim.Caching.Memcached.Results.StatusCodes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abhs.Code.Memcached
{
    public static class EnyimMemcachedHelper
    {
        private static MemcachedClient client;

        static EnyimMemcachedHelper()
        {
            client = GetClient();
            LogManager.AssignFactory(new Log4NetFactory());
        }
        private static MemcachedClient GetClient()
        {
            return new MemcachedClient();
        }
        public static bool Set(string key, object value, StoreMode mode = StoreMode.Set)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = GetUniqueKey();
            }
            return client.Store(mode, key, value);
        }
        public static bool Set(string key, object value, DateTime expiresAt, StoreMode mode = StoreMode.Set)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = GetUniqueKey();
            }
            return client.Store(mode, key, value, expiresAt);
        }
        public static bool Set(string key, object value, TimeSpan validFor, StoreMode mode = StoreMode.Set)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = GetUniqueKey();
            }
            return client.Store(mode, key, value, validFor);
        }

        public static object Get(string key)
        {
            return client.Get(key);
        }
        public static T Get<T>(string key)
        {
            return client.Get<T>(key);
        }

        /// <summary>
        /// 如果该键不存在，则通过使用指定的函数将键/值对添加到 Memcached 中。 如果该键存在，则返回新值或现有值。
        /// </summary>
        /// <param name="key">要添加的元素的键。</param>
        /// <param name="valueFactory">用于为键生成值的函数。</param>
        /// <param name="expiresAt">过期时间，默认永久有效</param>
        /// <returns>键的值。 如果 Memcached 中已存在该键，则为该键的现有值；如果 Memcached 中不存在该键，则为新值。</returns>
        public static T GetOrAdd<T>(string key, Func<string, T> valueFactory, DateTime expiresAt = default(DateTime))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            
            T local = default(T);
            IGetOperationResult<T> result = client.ExecuteGet<T>(key);
            if (result.Success && result.HasValue)
            {
                local = result.Value;
            }
            else
            {
                local = valueFactory(key);
                if (expiresAt == default(DateTime))
                {
                    Set(key, local);
                }
                else
                {
                    Set(key, local, expiresAt);
                }
            }
            return local;
        }

        public static IDictionary<string, object> Get(List<string> keys)
        {
            return client.Get(keys);
        }

        public static CasResult<bool> Cas(string key, object value, StoreMode mode = StoreMode.Set)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = GetUniqueKey();
            }
            return client.Cas(mode, key, value);
        }

        public static CasResult<object> GetWithCas(string key)
        {
            return client.GetWithCas(key);
        }
        public static CasResult<T> GetWithCas<T>(string key)
        {
            return client.GetWithCas<T>(key);
        }
        public static bool Remove(string key)
        {
            return client.Remove(key);
        }
        public static void FlushAll()
        {
            client.FlushAll();
        }

        static string GetUniqueKey(string prefix = null)
        {
            return (string.IsNullOrEmpty(prefix) ? "" : prefix + "_") + "_" + Guid.NewGuid().ToString("N");
        }

        public static List<string> Cachedump(this MemcachedClient client, int slab = 1, int limit = 0)
        {
            List<string> keys = new List<string>();
            
            Type t = client.GetType();
            PropertyInfo prop_Pool = t.GetProperty("Pool", BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo prop_KeyTransformer = t.GetProperty("KeyTransformer", BindingFlags.Instance | BindingFlags.NonPublic);

            IServerPool pool = prop_Pool.GetValue(client) as IServerPool;
            IMemcachedKeyTransformer keyTransformer = prop_KeyTransformer.GetValue(client) as IMemcachedKeyTransformer;

            string command = string.Format("{0} {1}", slab, limit);

            var hashedKey = keyTransformer.Transform(command.Replace(" ", "_"));
            var node = pool.Locate(hashedKey);

            if (node != null)
            {
                ICachedumpOperation op = new CachedumpOperation(command);
                var commandResult = node.Execute(op);

                if (commandResult.Success)
                {
                    keys.AddRange(op.Result.Keys);
                }
            }
            return keys;
        }
        
        public static Dictionary<IPEndPoint, List<int>> Slabs(this MemcachedClient client)
        {
            ServerStats stats = client.Stats("items");

            Type t = stats.GetType();
            FieldInfo field_Results = t.GetField("results", BindingFlags.Instance | BindingFlags.NonPublic);

            Dictionary<IPEndPoint, Dictionary<string, string>> results = field_Results.GetValue(stats) as Dictionary<IPEndPoint, Dictionary<string, string>>;
            Dictionary<IPEndPoint, Dictionary<string, string>>.Enumerator re = results.GetEnumerator();
            Dictionary<IPEndPoint, List<int>> servers = new Dictionary<IPEndPoint, List<int>>();
            while (re.MoveNext())
            {
                List<int> members = new List<int>();
                re.Current.Value.Keys.ToList().ForEach(key =>
                {
                    Match match = Regex.Match(key, @"items:(?<slab>\d+):number");
                    int slab = 0;
                    if (match.Success == true && int.TryParse(match.Groups["slab"].Value, out slab))
                    {
                        members.Add(slab);
                    }
                });

                servers.Add(re.Current.Key, members);
            }

            return servers;
        }

        /// <summary>
        /// 查询 Memcached 中所有的缓存键
        /// </summary>
        /// <returns>Memcached 中所有的缓存键</returns>
        public static List<string> GetAllKeys()
        {
            Dictionary<IPEndPoint, List<int>> slabs = client.Slabs();
            List<string> keys = new List<string>();
            slabs.Values.ToList().ForEach(members =>
            {
                members.ForEach(slab =>
                {
                    keys.AddRange(client.Cachedump(slab, 0));
                });
            });
            return keys;
        }

    }

    internal static class TextSocketHelper
    {
        private const string GenericErrorResponse = "ERROR";
        private const string ClientErrorResponse = "CLIENT_ERROR ";
        private const string ServerErrorResponse = "SERVER_ERROR ";
        private const int ErrorResponseLength = 13;

        public const string CommandTerminator = "\r\n";

        private static readonly Enyim.Caching.ILog log = Enyim.Caching.LogManager.GetLogger(typeof(TextSocketHelper));

        /// <summary>
        /// Reads the response of the server.
        /// </summary>
        /// <returns>The data sent by the memcached server.</returns>
        /// <exception cref="T:System.InvalidOperationException">The server did not sent a response or an empty line was returned.</exception>
        /// <exception cref="T:Enyim.Caching.Memcached.MemcachedException">The server did not specified any reason just returned the string ERROR. - or - The server returned a SERVER_ERROR, in this case the Message of the exception is the message returned by the server.</exception>
        /// <exception cref="T:Enyim.Caching.Memcached.MemcachedClientException">The server did not recognize the request sent by the client. The Message of the exception is the message returned by the server.</exception>
        public static string ReadResponse(PooledSocket socket)
        {
            string response = TextSocketHelper.ReadLine(socket);

            if (log.IsDebugEnabled)
                log.Debug("Received response: " + response);

            if (String.IsNullOrEmpty(response))
                throw new MemcachedClientException("Empty response received.");

            if (String.Compare(response, GenericErrorResponse, StringComparison.Ordinal) == 0)
                throw new NotSupportedException("Operation is not supported by the server or the request was malformed. If the latter please report the bug to the developers.");

            if (response.Length >= ErrorResponseLength)
            {
                if (String.Compare(response, 0, ClientErrorResponse, 0, ErrorResponseLength, StringComparison.Ordinal) == 0)
                {
                    throw new MemcachedClientException(response.Remove(0, ErrorResponseLength));
                }
                else if (String.Compare(response, 0, ServerErrorResponse, 0, ErrorResponseLength, StringComparison.Ordinal) == 0)
                {
                    throw new MemcachedException(response.Remove(0, ErrorResponseLength));
                }
            }

            return response;
        }


        /// <summary>
        /// Reads a line from the socket. A line is terninated by \r\n.
        /// </summary>
        /// <returns></returns>
        private static string ReadLine(PooledSocket socket)
        {
            MemoryStream ms = new MemoryStream(50);

            bool gotR = false;
            //byte[] buffer = new byte[1];

            int data;

            while (true)
            {
                data = socket.ReadByte();

                if (data == 13)
                {
                    gotR = true;
                    continue;
                }

                if (gotR)
                {
                    if (data == 10)
                        break;

                    ms.WriteByte(13);

                    gotR = false;
                }

                ms.WriteByte((byte)data);
            }

            string retval = Encoding.ASCII.GetString(ms.GetBuffer(), 0, (int)ms.Length);

            if (log.IsDebugEnabled)
                log.Debug("ReadLine: " + retval);

            return retval;
        }

        /// <summary>
        /// Gets the bytes representing the specified command. returned buffer can be used to streamline multiple writes into one Write on the Socket
        /// using the <see cref="M:Enyim.Caching.Memcached.PooledSocket.Write(IList&lt;ArraySegment&lt;byte&gt;&gt;)"/>
        /// </summary>
        /// <param name="value">The command to be converted.</param>
        /// <returns>The buffer containing the bytes representing the command. The command must be terminated by \r\n.</returns>
        /// <remarks>The Nagle algorithm is disabled on the socket to speed things up, so it's recommended to convert a command into a buffer
        /// and use the <see cref="M:Enyim.Caching.Memcached.PooledSocket.Write(IList&lt;ArraySegment&lt;byte&gt;&gt;)"/> to send the command and the additional buffers in one transaction.</remarks>
        public static IList<ArraySegment<byte>> GetCommandBuffer(string value)
        {
            var data = new ArraySegment<byte>(Encoding.ASCII.GetBytes(value));

            return new ArraySegment<byte>[] { data };
        }

        public static IList<ArraySegment<byte>> GetCommandBuffer(string value, IList<ArraySegment<byte>> list)
        {
            var data = new ArraySegment<byte>(Encoding.ASCII.GetBytes(value));

            list.Add(data);

            return list;
        }

    }

    public interface ICachedumpOperation : IOperation
    {
        string Paras { get; }
        Dictionary<string, string> Result { get; }
    }

    public class CachedumpOperation : Operation, ICachedumpOperation
    {
        internal CachedumpOperation(string paras)
        {
            this.paras = paras;
        }
        private string paras;
        private Dictionary<string, string> result;

        protected override IList<ArraySegment<byte>> GetBuffer()
        {
            var command = "stats cachedump " + this.Paras + TextSocketHelper.CommandTerminator;

            return TextSocketHelper.GetCommandBuffer(command);
        }

        protected override IOperationResult ReadResponse(PooledSocket socket)
        {
            this.result = new Dictionary<string, string>();

            while (true)
            {
                string response = TextSocketHelper.ReadResponse(socket);
                //response is:
                //ITEM testKey2 [3 b; 1600335168 s]
                //ITEM key_name [value_length b; expire_time | access_time s]
                // 0     1      2             3  4                         5
                //1.2.2- 访问时间(timestamp)
                //1.2.4+ 过期时间(timestamp)
                //如果是永不过期的key，expire_time会显示为服务器启动的时间

                if (string.Compare(response, "END", StringComparison.Ordinal) == 0)
                    break;

                if (response.Length < 5 || string.Compare(response, 0, "ITEM ", 0, 5, StringComparison.Ordinal) != 0)
                    throw new MemcachedClientException("No ITEM response received.\r\n" + response);

                string[] parts = response.Split(' ');
                this.result.Add(parts[1], string.Join(" ", parts, 2, parts.Length - 2));
            };
            var result = new CachedumpOperationResult();
            return result.Pass();
        }

        protected override bool ReadResponseAsync(PooledSocket socket, Action<bool> next)
        {
            throw new System.NotSupportedException();
        }

        public Dictionary<string, string> Result
        {
            get { return this.result; }
        }

        Dictionary<string, string> ICachedumpOperation.Result
        {
            get { return this.result; }
        }

        public string Paras
        {
            get
            {
                return this.paras;
            }
        }
        string ICachedumpOperation.Paras
        {
            get
            {
                return this.paras;
            }
        }
    }

    public class CachedumpOperationResult : OperationResultBase
    {
    }
}
