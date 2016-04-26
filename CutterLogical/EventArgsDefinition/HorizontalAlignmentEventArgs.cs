using System;
using System.Windows;

namespace CutterLogical.EventArgsDefinition
{
    public class HorizontalAlignmentEventArgs :EventArgs
    {
        public HorizontalAlignment HorizontalType { get; set; }
        public double Dist { get; set; }
    }
}