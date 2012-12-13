// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using System.IO;
using Point = System.Drawing.PointF;
using Number = System.Single;

namespace Poupou.SvgPathConverter {

	public class CSharpCoreGraphicsFormatter : ISourceFormatter {

		TextWriter writer;

		public CSharpCoreGraphicsFormatter (TextWriter textWriter)
		{
			writer = textWriter;
		}
		
		public void Prologue (string name)
		{
			writer.WriteLine ("\tstatic void {0} (CGContext c)", name);
			writer.WriteLine ("\t{");
		}
	
		public void Epilogue ()
		{
			writer.WriteLine ("\t\tc.FillPath ();");
			writer.WriteLine ("\t\tc.StrokePath ();");
			writer.WriteLine ("\t}");
			writer.WriteLine ();
		}
	
		public void MoveTo (Point pt)
		{
			writer.WriteLine ("\t\tc.MoveTo ({0}f, {1}f);", pt.X, pt.Y);
		}
	
		public void LineTo (Point pt)
		{
			writer.WriteLine ("\t\tc.AddLineToPoint ({0}f, {1}f);", pt.X, pt.Y);
		}
	
		public void ClosePath ()
		{
			writer.WriteLine ("\t\tc.ClosePath ();");
		}
	
		public void QuadCurveTo (Point endPoint, Point controlPoint)
		{
			writer.WriteLine ("\t\tc.AddQuadCurveToPoint ({0}f, {1}f, {2}f, {3}f);", endPoint.X, endPoint.Y, controlPoint.X, controlPoint.Y);
		}

		public void CurveTo (Point endPoint, Point controlPoint1, Point controlPoint2)
		{
			writer.WriteLine ("\t\tc.AddCurveToPoint ({0}f, {1}f, {2}f, {3}f, {4}f, {5}f);", 
				endPoint.X, endPoint.Y, controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y);
		}

		public void ArcTo (Point size, Number angle, bool isLarge, bool sweep, Point endPoint, Point startPoint)
		{
			this.ArcHelper (size, angle, isLarge, sweep, endPoint, startPoint);
		}
	}
}