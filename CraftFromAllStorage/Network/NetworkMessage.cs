using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thmsn.CraftFromAllStorage.Network
{
    public static class Channel
    {
        public const int DEFAULT = 15007;
    }

    public enum NetworkMessage
    {
        INITIALIZE_STORAGE = 1300,
        OPEN_ANIMATION = 1301,
        CLOSE_ANIMATION = 1302
    }
}
