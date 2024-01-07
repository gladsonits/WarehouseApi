using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouseEventTriggerFunction
{
    public interface ITaskHelper
    {
        //Use this method to deserialize input HTTP message payload
        T Deserialize<T>(string json);
        //Use this method to get Service Bus message bytes
        byte[] GetMessageBytes(string message);
    }
}
