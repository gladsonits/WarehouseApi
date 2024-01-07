using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WareHouseEventTriggerFunction
{
    public class TaskHelper : ITaskHelper
    {
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] GetMessageBytes(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
