// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//
// This file is mostly based on the C++ code from once magnificent Moonlight
// https://github.com/mono/moon/blob/master/src/xaml.cpp
// Copyright 2007 Novell, Inc. (http://www.novell.com)
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using System.Collections.Generic;
using Point = System.Drawing.PointF;
using Number = System.Single;

namespace Poupou.SvgPathConverter {

	public class SvgPathParser {

		static int i;

		public ISourceFormatter Formatter { get; set; }

		public void Parse (string svgPath, string name = null)
		{
			if (Formatter == null)
				throw new InvalidOperationException ("Missing formatter");

			if (name == null)
				name = "Unnamed_" + (++i).ToString ();

			Parse (svgPath, name, Formatter);
		}

		static void Advance (string s, ref int pos)
		{
			if (pos >= s.Length)
				return;
			char c = s [pos];
			while (!Char.IsLetterOrDigit (c) && c != '.' && c!= '-' && c != '+') {
				if (++pos == s.Length)
					return;
				c = s [pos];
			}
		}
		
		static int FindNonFloat (string s, int pos)
		{
			char c = s [pos];
            while(Char.IsWhiteSpace(c)) {
                if (++pos == s.Length)
                    return pos;
                c = s[pos];
            }
			while ((Char.IsNumber (c) || c == '.' || c == '-' || c == '+')) {
				if (++pos == s.Length)
					return pos;
				c = s [pos];
			}
			return pos;
		}
		
		static bool MorePointsAvailable (string s, int pos)
		{
			if (pos >= s.Length)
				return false;
			char c = s [pos];
			while (Char.IsWhiteSpace (c) || c == ',')
				c = s [++pos];
			return Char.IsDigit (c) || c == '.' || c == '-' || c == '+';
		}
		
		static Number GetFloat (string svg, ref int pos)
		{
			int end = FindNonFloat (svg, pos);
			string s = svg.Substring (pos, end - pos);
            var f = Number.Parse(s);
			pos = end;
			return f;
		}
		
		static Point GetPoint (string svg, ref int pos)
		{
			while (Char.IsWhiteSpace (svg [pos]))
				pos++;
			float x = GetFloat (svg, ref pos);
			
			while (Char.IsWhiteSpace (svg [pos]))
				pos++;
			if (svg [pos] == ',')
				pos++;
			while (Char.IsWhiteSpace (svg [pos]))
				pos++;
			
			float y = GetFloat (svg, ref pos);
			
			return new Point (x, y);
		}
		
		static Point MakeRelative (Point c, Point m)
		{
			return new Point (m.X + c.X, m.Y + c.Y);
		}

		static void Parse (string svg, string name, ISourceFormatter formatter)
		{
			formatter.Prologue (name);

			var start = new Point (0, 0);
			var cp = new Point (0, 0);
			var cp1 = new Point (0, 0);
			var cp2 = new Point (0, 0);
			var cp3 = new Point (0, 0);
			var qbzp = new Point (0, 0); 
			var cbzp = new Point (0, 0);
			int fill_rule = 0;
			int pos = 0;
			bool cbz = false;
			bool qbz = false;
			while (pos < svg.Length) {
				char c = svg [pos++];
				if (Char.IsWhiteSpace (c))
					continue;
				
				bool relative = false;
				switch (c) {
				case 'f':
				case 'F':
					c = svg [pos++];
					if (c == '0')
						fill_rule = 0;
					else if (c == '1')
						fill_rule = 1;
					else
						throw new FormatException ();
					break;
				case 'h':
					relative = true;
					goto case 'H';
				case 'H':
					float x = GetFloat (svg, ref pos);
					if (relative)
						x += cp.X;
					cp = new Point (x, cp.Y);
					formatter.LineTo (cp);
					cbz = qbz = false;
					break;
				case 'm':
					relative = true;
					goto case 'M';
				case 'M':
					cp1 = GetPoint (svg, ref pos);
					if (relative)
						cp1 = MakeRelative (cp, cp1);
					formatter.MoveTo (cp1);
					
					start = cp = cp1;
					
					Advance (svg, ref pos);
					while (MorePointsAvailable (svg, pos)) {
						cp1 = GetPoint (svg, ref pos);
						if (relative)
							cp1 = MakeRelative (cp, cp1);
						formatter.LineTo (cp1);
					}
					cp = cp1;
					cbz = qbz = false;
					break;
				case 'l':
					relative = true;
					goto case 'L';
				case 'L':
					while (MorePointsAvailable (svg, pos)) {
						cp1 = GetPoint (svg, ref pos);
						if (relative)
							cp1 = MakeRelative (cp, cp1);
						Advance (svg, ref pos);
						
						formatter.LineTo (cp1);
						cp = cp1;
					}
					cbz = qbz = false;
					break;
				case 'a':
					relative = true;
					goto case 'A';
				case 'A':
					while (MorePointsAvailable (svg, pos)) {
						cp1 = GetPoint (svg, ref pos);
						// this is a width and height so it's not made relative to cp
						Advance (svg, ref pos);

						float angle = GetFloat (svg, ref pos);
						Advance (svg, ref pos);

						bool is_large = GetFloat (svg, ref pos) != 0.0f;
						Advance (svg, ref pos);

						bool positive_sweep = GetFloat (svg, ref pos) != 0.0f;
						Advance (svg, ref pos);

						cp2 = GetPoint (svg, ref pos);
						if (relative)
							cp2 = MakeRelative (cp, cp2);
						Advance (svg, ref pos);
						
						formatter.ArcTo (cp1, angle, is_large, positive_sweep, cp2, cp);
						
						cp = cp2;
						Advance (svg, ref pos);
					}
					qbz = false;
					cbz = false;
					break;
				case 'q':
					relative = true;
					goto case 'Q';
				case 'Q':
					while (MorePointsAvailable (svg, pos)) {
						cp1 = GetPoint (svg, ref pos);
						if (relative)
							cp1 = MakeRelative (cp, cp1);
						Advance (svg, ref pos);
						
						cp2 = GetPoint (svg, ref pos);
						if (relative)
							cp2 = MakeRelative (cp, cp2);
						Advance (svg, ref pos);
						
						formatter.QuadCurveTo (cp1, cp2);
						
						cp = cp2;
						Advance (svg, ref pos);
					}
					qbz = true;
					qbzp = cp1;
					cbz = false;
					break;
				case 'c':
					relative = true;
					goto case 'C';
				case 'C':
					while (MorePointsAvailable (svg, pos)) {
						cp1 = GetPoint (svg, ref pos);
						if (relative)
							cp1 = MakeRelative (cp, cp1);
						Advance (svg, ref pos);
						
						cp2 = GetPoint (svg, ref pos);
						if (relative)
							cp2 = MakeRelative (cp, cp2);
						Advance (svg, ref pos);
						
						cp3 = GetPoint (svg, ref pos);
						if (relative)
							cp3 = MakeRelative (cp, cp3);
						Advance (svg, ref pos);

						formatter.CurveTo (cp1, cp2, cp3);
						
						cp1 = cp3;
					}
					cp = cp3;
					cbz = true;
					cbzp = cp2;
					qbz = false;
					break;
				case 't':
					relative = true;
					goto case 'T';
				case 'T':
					while (MorePointsAvailable (svg, pos)) {
						cp2 = GetPoint (svg, ref pos);
						if (relative)
							cp2 = MakeRelative (cp, cp2);
						if (qbz) {
							cp1.X = 2 * cp.X - qbzp.X;
							cp1.Y = 2 * cp.Y - qbzp.Y;
						} else {
							cp1 = cp;
						}
						formatter.QuadCurveTo (cp1, cp2);
						qbz = true;
						qbzp = cp1;
						cp = cp2;
						Advance (svg, ref pos);
					}
					cbz = false;
					break;
				case 's':
					relative = true;
					goto case 'S';
				case 'S':
					while (MorePointsAvailable (svg, pos)) {
						cp2 = GetPoint (svg, ref pos);
						if (relative)
							cp2 = MakeRelative (cp, cp2);
						Advance (svg, ref pos);

						cp3 = GetPoint (svg, ref pos);
						if (relative)
							cp3 = MakeRelative (cp, cp3);

						if (cbz) {
							cp1.X = 2 * cp.X - cbzp.X;
							cp1.Y = 2 * cp.Y - cbzp.Y;
						} else {
							cp1 = cp;
						}
						formatter.CurveTo (cp1, cp2, cp3);
						cbz = true;
						cbzp = cp2;
						cp = cp3;
						Advance (svg, ref pos);
					}
					qbz = false;
					break;
				case 'v':
					relative = true;
					goto case 'V';
				case 'V':
					float y = GetFloat (svg, ref pos);
					if (relative)
						y += cp.Y;
					cp = new Point (cp.X, y);
					formatter.LineTo (cp);
					cbz = qbz = false;
					break;
				case 'z':
				case 'Z':
					formatter.ClosePath ();
					formatter.MoveTo (start);
					cp = start;
					cbz = qbz = false;
					break;
				default:
					throw new FormatException (c.ToString ());
				}
			}
			formatter.Epilogue ();
		}
#if false
static Geometry *
geometry_from_str (const char *str)
{
	char *inptr = (char *) str;
	Point cp = Point (0, 0);
	Point cp1, cp2, cp3;
	Point start;
	char *end;
	PathGeometry *pg = NULL;
	FillRule fill_rule = FillRuleEvenOdd;
	bool cbz = false; // last figure is a cubic bezier curve
	bool qbz = false; // last figure is a quadratic bezier curve
	Point cbzp, qbzp; // points needed to create "smooth" beziers

	moon_path *path = moon_path_new (10);

	while (*inptr) {
		if (g_ascii_isspace (*inptr))
			inptr++;

		if (!inptr[0])
			break;

		bool relative = false;

		char c = *inptr;
		inptr = g_utf8_next_char (inptr);

		switch (c) {
		case 'c':
			relative = true;
		case 'C':
		{
			while (more_points_available (&inptr)) {
				if (!get_point (&cp1, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp1);

				advance (&inptr);

				if (!get_point (&cp2, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp2);

				advance (&inptr);

				if (!get_point (&cp3, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp3);

				advance (&inptr);

				moon_curve_to (path, cp1.x, cp1.y, cp2.x, cp2.y, cp3.x, cp3.y);

				cp1.x = cp3.x;
				cp1.y = cp3.y;
			}
			cp.x = cp3.x;
			cp.y = cp3.y;
			cbz = true;
			cbzp.x = cp2.x;
			cbzp.y = cp2.y;
			qbz = false;
			break;
		}
		case 's':
			relative = true;
		case 'S':
		{
			while (more_points_available (&inptr)) {
				if (!get_point (&cp2, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp2);

				advance (&inptr);

				if (!get_point (&cp3, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp3);

				if (cbz) {
					cp1.x = 2 * cp.x - cbzp.x;
					cp1.y = 2 * cp.y - cbzp.y;
				} else
					cp1 = cp;

				moon_curve_to (path, cp1.x, cp1.y, cp2.x, cp2.y, cp3.x, cp3.y);
				cbz = true;
				cbzp.x = cp2.x;
				cbzp.y = cp2.y;

				cp.x = cp3.x;
				cp.y = cp3.y;

				advance (&inptr);
			}
			qbz = false;
			break;
		}
		case 'a':
			relative = true;
		case 'A':
		{
			while (more_points_available (&inptr)) {
				if (!get_point (&cp1, &inptr))
					break;

				advance (&inptr);

				double angle = g_ascii_strtod (inptr, &end);
				if (end == inptr)
					break;

				inptr = end;
				advance (&inptr);

				int is_large = strtol (inptr, &end, 10);
				if (end == inptr)
					break;

				inptr = end;
				advance (&inptr);

				int sweep = strtol (inptr, &end, 10);
				if (end == inptr)
					break;

				inptr = end;
				advance (&inptr);

				if (!get_point (&cp2, &inptr))
					break;

				if (relative)
					make_relative (&cp, &cp2);

				moon_arc_to (path, cp1.x, cp1.y, angle, is_large, sweep, cp2.x, cp2.y);

				cp.x = cp2.x;
				cp.y = cp2.y;

				advance (&inptr);
			}
			cbz = qbz = false;
			break;
		}
	}

	pg = new PathGeometry (path);
	pg->SetFillRule (fill_rule);
	return pg;

bad_pml:
	moon_path_destroy (path);
	return NULL;
}		}
#endif
	}
}