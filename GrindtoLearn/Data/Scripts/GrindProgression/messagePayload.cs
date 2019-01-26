using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace Pixel
{
    public class MessagePayload
    {
        public MyDefinitionId definitionId { get; set; }
        public bool success { get; set; }

        public MessagePayload(MyDefinitionId _definitionId, bool _success)
        {
            definitionId = _definitionId;
            success = _success;
        }
    }
}
