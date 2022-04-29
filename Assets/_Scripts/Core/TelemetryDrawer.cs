using UnityEngine;

namespace FGear
{
    public class TelemetryDrawer
    {
        //telemetry window
        protected int mWinID;

        protected Rect mWindowRect;

        protected virtual void initTelemetryWindow()
        {
        }

        public virtual void drawTelemetry()
        {
        }
    }
}