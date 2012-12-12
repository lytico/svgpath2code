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

	public class XwtContextFormatter : ISourceFormatter {

		TextWriter writer;

        public XwtContextFormatter(TextWriter textWriter)
		{
			writer = textWriter;
		}
		
		public void Prologue (string name)
		{
			writer.WriteLine ("\tstatic void {0} (Context c)", name);
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
	
		public void QuadCurveTo (Point pt1, Point pt2)
		{
			writer.WriteLine ("\t\tc.AddQuadCurveToPoint ({0}f, {1}f, {2}f, {3}f);", pt1.X, pt1.Y, pt2.X, pt2.Y);
		}

		public void CurveTo (Point pt1, Point pt2, Point pt3)
		{
			writer.WriteLine ("\t\tc.AddCurveToPoint ({0}f, {1}f, {2}f, {3}f, {4}f, {5}f);", 
				pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y);
		}

		public void ArcTo (Point size, Number angle, bool isLarge, bool sweep, Point endPoint, Point startPoint)
		{
			this.ArcHelper (size, angle, isLarge, sweep, endPoint, startPoint);
		}
	}
}