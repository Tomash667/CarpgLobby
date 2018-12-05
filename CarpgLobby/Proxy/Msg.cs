using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarpgLobby.Proxy
{
    public class Msg
    {

        public delegate bool Callback(Msg msg);
    }
}
