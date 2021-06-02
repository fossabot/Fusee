using System;

namespace Fusee.Examples.Integrations.WpfFramebuffer.Core
{
    public class RotationXChangedEventArgs : EventArgs
    {
        public float Rad;

        public RotationXChangedEventArgs(float rad)
        {
            Rad = rad;
        }
    }

    public class RotationYChangedEventArgs : EventArgs
    {
        public float Rad;

        public RotationYChangedEventArgs(float rad)
        {
            Rad = rad;
        }
    }

    public class RotationZChangedEventArgs : EventArgs
    {
        public float Rad;

        public RotationZChangedEventArgs(float rad)
        {
            Rad = rad;
        }
    }
}