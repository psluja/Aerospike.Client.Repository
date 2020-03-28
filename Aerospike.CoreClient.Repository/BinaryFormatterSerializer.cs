using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.CoreClient.Repository
{
    public class BinaryFormatterSerializer:ISerializer
    {
        public object Deserialize(Type type, byte[] data)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                return bf.Deserialize(ms);
            }
        }

        public byte[] Serialize(object o)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                ms.Flush();
                return ms.ToArray();
            }
        }
    }
}
