using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qf4net
{
    /// <summary>
    /// Provide durable state-machine support, via an online/offline capability.
    /// </summary>
    public enum MachineState
    {
        Online,
        Exiting,
        Offline
    }

}
