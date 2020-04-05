using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRecentAct.Memory
{
    class OsuEnterKey
    {
        //88 c3 a2 ? ? ? ? 03 24

        private static readonly string s_pattern = "\x88\xc3\xa2\x00\x00\x00\x00\x03\x24";

        private static readonly string s_mask = "x????xx";

        private IntPtr m_address;
        public bool TryInit()
        {
            bool success = false;

            return success;
        }
    }
}
