using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace SML.UWP.WifiConf
{
    class ClientComparer : IEqualityComparer<NetworkOperatorTetheringClient>
    {
        public bool Equals(NetworkOperatorTetheringClient x, NetworkOperatorTetheringClient y) => x.MacAddress == y.MacAddress;

        public int GetHashCode(NetworkOperatorTetheringClient obj) => obj.MacAddress.GetHashCode();
    }
}
