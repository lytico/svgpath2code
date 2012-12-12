// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//
// This file is mostly based on the C++ code from once magnificent Moonlight
// https://github.com/mono/moon/blob/master/src/moon-path.cpp
// Copyright 2007-2008 Novell, Inc. (http://www.novell.com)
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using System.IO;
using Point = System.Drawing.PointF;
using Number = System.Single;

namespace Poupou.SvgPathConverter {
	
	public static class FormatterRocks {

        static bool IsNearZero(Number value)
		{
			return Math.Abs (value) < 0.000019;
		}

		// The SVG Arc is a bit more complex than others - and also quite different than many existing API
		// This implementation will use a ISourceFormatter's CurveTo method to draw the arc
		public static void ArcHelper (this ISourceFormatter formatter, Point size, float anglef, 
			bool isLarge, bool sweep, Point endPoint, Point startPoint)
		{
			if (IsNearZero (endPoint.X - startPoint.X) && IsNearZero (endPoint.Y - startPoint.Y))
				return;

			// Correction of out-of-range radii, see F6.6 (step 1)
			if (IsNearZero (size.X) || IsNearZero (size.Y)) {
				// treat this as a straight line (to end point)
				formatter.LineTo (endPoint);
				return;
			}

			// Correction of out-of-range radii, see F6.6.1 (step 2)
			var rx = Math.Abs (size.X);
			var ry = Math.Abs (size.Y);
			
			// convert angle into radians
			var angle = anglef * Math.PI / 180.0d;
			
			// variables required for F6.3.1
			var cos_phi = Math.Cos (angle);
			var sin_phi = Math.Sin (angle);
			var dx2 = (startPoint.X - endPoint.X) / 2.0;
			var dy2 = (startPoint.Y - endPoint.Y) / 2.0;
			var x1p = cos_phi * dx2 + sin_phi * dy2;
			var y1p = cos_phi * dy2 - sin_phi * dx2;
			var x1p2 = x1p * x1p;
			var y1p2 = y1p * y1p;
			var rx2 = rx * rx;
			var ry2 = ry * ry;
			
			// Correction of out-of-range radii, see F6.6.2 (step 4)
			var lambda = (x1p2 / rx2) + (y1p2 / ry2);
			if (lambda > 1.0) {
				// see F6.6.3
				var lambda_root = (Number) Math.Sqrt (lambda);
				rx *= lambda_root;
				ry *= lambda_root;
				// update rx2 and ry2
				rx2 = rx * rx;
				ry2 = ry * ry;
			}
			
			double cxp, cyp, cx, cy;
			double c = (rx2 * ry2) - (rx2 * y1p2) - (ry2 * x1p2);
			
			// check if there is no possible solution (i.e. we can't do a square root of a negative value)
			if (c < 0.0) {
				// scale uniformly until we have a single solution (see F6.2) i.e. when c == 0.0
                var scale = (Number)Math.Sqrt(1.0 - c / (rx2 * ry2));
				rx *= scale;
				ry *= scale;
				// update rx2 and ry2
				rx2 = rx * rx;
				ry2 = ry * ry;
				
				// step 2 (F6.5.2) - simplified since c == 0.0
				cxp = 0.0;
				cyp = 0.0;
				
				// step 3 (F6.5.3 first part) - simplified since cxp and cyp == 0.0
				cx = 0.0;
				cy = 0.0;
			} else {
				// complete c calculation
				c = Math.Sqrt (c / ((rx2 * y1p2) + (ry2 * x1p2)));
				
				// inverse sign if Fa == Fs
				if (isLarge == sweep)
					c = -c;
				
				// step 2 (F6.5.2)
				cxp = c * ( rx * y1p / ry);
				cyp = c * (-ry * x1p / rx);
				
				// step 3 (F6.5.3 first part)
				cx = cos_phi * cxp - sin_phi * cyp;
				cy = sin_phi * cxp + cos_phi * cyp;
			}
			
			// step 3 (F6.5.3 second part) we now have the center point of the ellipse
			cx += (startPoint.X + endPoint.X) / 2.0;
			cy += (startPoint.Y + endPoint.Y) / 2.0;
			
			// step 4 (F6.5.4)
			// we dont' use arccos (as per w3c doc), see http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/index.htm
			// note: atan2 (0.0, 1.0) == 0.0
			var at = Math.Atan2 (((y1p - cyp) / ry), ((x1p - cxp) / rx));
			var theta1 = (at < 0.0) ? 2.0 * Math.PI + at : at;
			
			var nat = Math.Atan2 (((-y1p - cyp) / ry), ((-x1p - cxp) / rx));
			var delta_theta = (nat < at) ? 2.0 * Math.PI - at + nat : nat - at;
			
			if (sweep) {
				// ensure delta theta < 0 or else add 360 degrees
				if (delta_theta < 0.0)
					delta_theta += 2.0 * Math.PI;
			} else {
				// ensure delta theta > 0 or else substract 360 degrees
				if (delta_theta > 0.0)
					delta_theta -= 2.0 * Math.PI;
			}
			
			// add several cubic bezier to approximate the arc (smaller than 90 degrees)
			// we add one extra segment because we want something smaller than 90deg (i.e. not 90 itself)
			var segments = (int) (Math.Abs (delta_theta / Math.PI)) + 1;
			var delta = delta_theta / segments;
			
			// http://www.stillhq.com/ctpfaq/2001/comp.text.pdf-faq-2001-04.txt (section 2.13)
			var bcp = (Number) (4.0 / 3 * (1 - Math.Cos (delta / 2)) / Math.Sin (delta / 2));
			
			var cos_phi_rx = cos_phi * rx;
			var cos_phi_ry = cos_phi * ry;
			var sin_phi_rx = sin_phi * rx;
			var sin_phi_ry = sin_phi * ry;
			
			var cos_theta1 = Math.Cos (theta1);
			var sin_theta1 = Math.Sin (theta1);

			var c1 =  new Point (0, 0);
                var c2 = new Point (0, 0);
			
			int i;
			for (i = 0; i < segments; ++i) {
				// end angle (for this segment) = current + delta
				var theta2 = theta1 + delta;
				var cos_theta2 = Math.Cos (theta2);
				var sin_theta2 = Math.Sin (theta2);
				
				// first control point (based on start point sx,sy)
				c1.X = startPoint.X - bcp * (Number) (cos_phi_rx * sin_theta1 + sin_phi_ry * cos_theta1);
                c1.Y = startPoint.Y + bcp * (Number)(cos_phi_ry * cos_theta1 - sin_phi_rx * sin_theta1);
				
				// end point (for this segment)
                endPoint.X = (Number)(cx + (cos_phi_rx * cos_theta2 - sin_phi_ry * sin_theta2));
                endPoint.Y = (Number)(cy + (sin_phi_rx * cos_theta2 + cos_phi_ry * sin_theta2));
				
				// second control point (based on end point ex,ey)
                c2.X = endPoint.X + bcp * (Number)(cos_phi_rx * sin_theta2 + sin_phi_ry * cos_theta2);
                c2.Y = endPoint.Y + bcp * (Number)(sin_phi_rx * sin_theta2 - cos_phi_ry * cos_theta2);
				
				formatter.CurveTo (c1, c2, endPoint);
				
				// next start point is the current end point (same for angle)
				startPoint = endPoint;
				theta1 = theta2;
				// avoid recomputations
				cos_theta1 = cos_theta2;
				sin_theta1 = sin_theta2;
			}
		}
	}
}