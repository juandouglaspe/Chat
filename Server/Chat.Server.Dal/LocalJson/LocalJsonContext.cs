using Chat.Server.Dal.LocalJson.Interfaces;
using Chat.Server.Dal.MongoDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Server.Dal.LocalJson
{
    public class LocalJsonContext
    {
        public LocalJsonContext(Encoding encoding) : this(encoding, "Data")
        {
            ConnectedsUsers = new AsyncJson<ConnectedUser>(GetFileName<ConnectedUser>(), encoding);
        }
        public LocalJsonContext(Encoding encoding, string local)
        {
            ConnectedsUsers = new AsyncJson<ConnectedUser>(GetFileName<ConnectedUser>(), encoding);
            BaseFolder = local;
        }

        private string BaseFolder;
        private string BasePath => $"{Environment.CurrentDirectory}\\{BaseFolder}\\";

        public IAsyncJsonFile<ConnectedUser> ConnectedsUsers { get; set; }

        public string GetFileName<T>()
        {
            return $"{BasePath}{typeof(T).Name}.json";
        }
    }
}
