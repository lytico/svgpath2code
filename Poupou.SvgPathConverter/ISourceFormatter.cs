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
        /// <summary>
        /// adds a bezier curve from current (startPoint) to endPoint with controlPoint
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="controlPoint"></param>
		void QuadCurveTo (Point endPoint, Point controlPoint);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
		void CurveTo (Point endPoint, Point controlPoint1, Point controlPoint2);
        void ArcTo(Point size, Number angle, bool isLarge, bool sweep, Point ep, Point sp);
		void ClosePath ();
	}
}