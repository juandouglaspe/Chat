using Chat.Server.Dal.LocalJson.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Server.Dal.LocalJson.Interfaces
{
    public interface IAsyncJsonFile<TModel> : IAsyncEnumerable<TModel>, IAsyncDisposable, IDisposable
    {
        const int DefaultBufferSize = 2024;
        /// <summary>
        /// Json File Path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Get File Stream for Json File
        /// </summary>
        protected internal FileStream FileStream { get; set; }
        /// <summary>
        /// Get Encoding for 
        /// </summary>
        protected internal Encoding Encoding { get; set; }
        /// <summary>
        /// Get File bytes length
        /// </summary>
        public int FileLength { get; }
        public bool Modified { get; set; }
        public bool IsEmpty
        {
            get
            {
                return Buffer.Count() < 1;
            }
        }
        protected internal IEnumerable<TModel> Buffer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task AddAsync(TModel model)
        {
            string json = JsonConvert.SerializeObject(model);
            json += "]";

            if (!IsEmpty)
            {
                json = "," + json;
            }

            byte[] encBytes = Encoding.GetBytes(json);

            FileStream.Seek(FileLength - 1, SeekOrigin.Begin);
            await FileStream.WriteAsync(encBytes.AsMemory(0, encBytes.Length));

            await FileStream.FlushAsync();
            Modified = true;
        }

        /// <summary>
        /// Zeroes the bytes with the object data.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task RemoveAsync(TModel model)
        {
            model = await GetOneAsync(md =>
            {
                bool contains = false;
                PropertyInfo property = null;
                foreach (var item in typeof(TModel).GetProperties())
                {
                    object[] vs = item.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                    contains = vs.Length > 0;

                    if (contains)
                    {
                        property = item;
                        break;
                    }
                }

                if (!contains)
                    return false;

                return property.GetValue(md) == property.GetValue(model);
            });

            string json = JsonConvert.SerializeObject(model);
            byte[] encBytes = Encoding.GetBytes(json);
            bool modified = false;

            for (int i = 1; i < FileLength; i++)
            {
                bool equal = true;
                byte[] buffer = new byte[DefaultBufferSize];
                FileStream.Seek(i, SeekOrigin.Begin);
                await FileStream.ReadAsync(buffer, 0, buffer.Length);

                for (int j = 0; j < encBytes.Length; j++)
                {
                    if (buffer[j] != encBytes[j])
                    {
                        equal = false;
                        break;
                    }
                }

                if (equal)
                {
                    int length = encBytes.Length;

                    if (Buffer.Count() > 1)
                        length++;

                    buffer = new byte[length];
                    FileStream.Seek(i, SeekOrigin.Begin);
                    await FileStream.WriteAsync(buffer.AsMemory());

                    modified = true;
                    break;
                }
            }

            await FileStream.FlushAsync();
            Modified = modified;
        }
        public async Task RemoveAllAsync(Func<TModel, bool> predicated)
        {

        }
        protected internal async Task<string> GetAllTextAsync()
        {
            byte[] buffer = new byte[DefaultBufferSize];
            int start = 0;

            while ((await FileStream.ReadAsync(buffer.AsMemory(start))) != 0)
            {

            }

            return Encoding.GetString(buffer);
        }
        protected internal async Task<IEnumerable<TModel>> GetAllJsonObjectsAsync()
        {
            if (!Modified)
            {
                if (Buffer != null)
                    return Buffer;
            }

            string allText = await GetAllTextAsync();
            IEnumerable<TModel> models = JsonConvert.DeserializeObject<IEnumerable<TModel>>(allText);

            return models;
        }
        public async Task<IEnumerable<TModel>> GetAllAsync(Func<TModel, bool> predicated)
        {
            IEnumerable<TModel> all = await GetAllJsonObjectsAsync();
            List<TModel> models = all.Where(predicated).ToList();

            return models;
        }
        protected internal virtual bool UpdateBuffer()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual async Task<TModel> GetOneAsync(Func<TModel, bool> predicate)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates a new empty file in the specified directory
        /// </summary>
        /// <param name="path">new file path</param>
        public void InitializeFile(string path);
    }
}
