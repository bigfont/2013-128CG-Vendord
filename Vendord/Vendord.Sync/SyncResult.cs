using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Vendord.Sync
{
    public enum SyncResult
    {
        NoRemoteDatabase,
        Disconnected,
        Complete
    }
}
