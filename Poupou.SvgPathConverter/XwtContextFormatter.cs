/*
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 Lytico (http://www.limada.org)
 * 
 */
using System;
using System.IO;
using Point = System.Drawing.PointF;
using Number = System.Single;
using System.Globalization;

namespace Poupou.SvgPathConverter
{
	public class XwtContextFormatter : ISourceFormatter
	{

		TextWriter writer;

		public XwtContextFormatter (TextWriter textWriter)
		{
			writer = textWriter;
		    
		}
		
		public virtual void Prologue (string name)
		{
			writer.WriteLine ("\tpublic virtual void {0} (Context c)", name);
			writer.WriteLine ("\t{");
		}
	
		public virtual void Epilogue ()
		{
			//writer.WriteLine ("\t\tc.FillPreserve ();");
			//writer.WriteLine ("\t\tc.Stroke ();");
			writer.WriteLine ("\t}");
			writer.WriteLine ();
		}
	
        public string Write(double value) {
            return value.ToString("#.#####",CultureInfo.InvariantCulture);
        }
		public void MoveTo (Point pt)
		{
            writer.WriteLine("\t\tc.MoveTo ({0}d, {1}d);", Write(pt.X), Write(pt.Y));
		}
	
		public void LineTo (Point pt)
		{
			writer.WriteLine ("\t\tc.LineTo ({0}d, {1}d);", Write(pt.X), Write(pt.Y));
		}
	
		public void ClosePath ()
		{
			writer.WriteLine ("\t\tc.ClosePath ();");
		}
	
		public void QuadCurveTo (Point controlPoint, Point endPoint)
		{
			writer.WriteLine ("\t\tc.CurveTo ({0}d, {1}d, {2}d, {3}d, {4}d, {5}d);",
                Write(controlPoint.X), Write(controlPoint.Y), 
                Write(controlPoint.X), Write(controlPoint.Y), 
                Write(endPoint.X), Write(endPoint.Y));
		}

		public void CurveTo (Point endPoint, Point controlPoint1, Point controlPoint2)
		{
			writer.WriteLine ("\t\tc.CurveTo ({0}d, {1}d, {2}d, {3}d, {4}d, {5}d);", 
				Write(endPoint.X), Write(endPoint.Y), 
                Write(controlPoint1.X), Write(controlPoint1.Y), 
                Write(controlPoint2.X), Write(controlPoint2.Y));
		}

		public void ArcTo (Point size, Number angle, bool isLarge, bool sweep, Point endPoint, Point startPoint)
		{
			this.ArcHelper (size, angle, isLarge, sweep, endPoint, startPoint);
		}
	}
}