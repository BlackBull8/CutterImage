using System.Windows;
using System;

namespace CutterLogical.EventArgsDefinition
{
    public class VerticalAlignmentEventArgs:EventArgs
    {
        public VerticalAlignment VerticalType { get; set; }
        public double Dist { get; set; }
    }
}