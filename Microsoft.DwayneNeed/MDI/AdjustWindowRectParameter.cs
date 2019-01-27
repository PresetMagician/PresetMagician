using System.Windows;

namespace Microsoft.DwayneNeed.MDI
{
    public class AdjustWindowRectParameter
    {
        public AdjustWindowRectParameter()
        {
        }

        public AdjustWindowRectParameter(Vector delta, MdiWindowEdge interactiveEdges)
        {
            Delta = delta;
            InteractiveEdges = interactiveEdges;
        }

        /// <summary>
        ///     The amount to adjust the window rect by.
        /// </summary>
        public Vector Delta { get; set; }

        /// <summary>
        ///     The edges of the window rect to adust.
        /// </summary>
        /// <remarks>
        ///     Specifying None means to move the window rect by the Delta
        ///     amount.
        /// </remarks>
        public MdiWindowEdge InteractiveEdges { get; set; }
    }
}