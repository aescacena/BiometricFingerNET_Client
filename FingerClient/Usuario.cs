using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FingerClient
{
    [Serializable]
    public class Usuario
    {
        public int id { get; set; }
        public string username { get; set; }
        public byte[] finger { get; set; }
    }
}
