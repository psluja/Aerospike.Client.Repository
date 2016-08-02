using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aerospike.Client.Repository
{
    public interface IBinaryPresenter
    {
        string MakeString(byte[] data);
        byte[] MakeBinary(string str);
    }

    public class StringBinaryPresenter: IBinaryPresenter
    {
        public string MakeString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public byte[] MakeBinary(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
