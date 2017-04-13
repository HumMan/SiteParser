using System;
using System.Threading;

namespace Viewer.Assistants
{
    public class TypeAssistant
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }
        Timer waitingTimer;

        public TypeAssistant(int waitingMilliSeconds = 600)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            waitingTimer = new Timer(p =>
            {
                Idled(this, EventArgs.Empty);
            });
        }
        public void TextChanged()
        {
            waitingTimer.Change(WaitingMilliSeconds, Timeout.Infinite);
        }
    }
}
