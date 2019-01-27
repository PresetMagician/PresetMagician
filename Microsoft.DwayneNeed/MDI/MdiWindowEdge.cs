using System;

namespace Microsoft.DwayneNeed.MDI
{
    [Flags]
    public enum MdiWindowEdge
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8
    }
}