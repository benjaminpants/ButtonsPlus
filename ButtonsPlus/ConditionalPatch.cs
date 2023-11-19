using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ButtonsPlus
{
    public class ConditionalPatchLockdownLevers : ConditionalPatch
    {
        public override bool ShouldPatch()
        {
            return ButtonsPlusPlugin.Instance.configLockdownLevers.Value;
        }
    }
}
