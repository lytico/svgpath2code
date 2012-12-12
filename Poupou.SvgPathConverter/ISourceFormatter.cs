// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using Point = System.Drawing.PointF;
using Number = System.Single;

namespace Poupou.SvgPathConverter {
	
	public interface ISourceFormatter {
	
		void Prologue (string name);
		void Epilogue ();
	
		void MoveTo (Point pt);
		void LineTo (Point pt);
		void QuadCurveTo (Point pt1, Point pt2);
		void CurveTo (Point pt1, Point pt2, Point pt3);
        void ArcTo(Point size, Number angle, bool isLarge, bool sweep, Point ep, Point sp);
		void ClosePath ();
	}
}