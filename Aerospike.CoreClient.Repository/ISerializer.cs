using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.CoreClient.Repository
{
    public interface ISerializer
    {
        object Deserialize(Type type, byte[] data);
        byte[] Serialize( object o);
    }
}
