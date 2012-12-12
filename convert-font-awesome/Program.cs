// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012 Xamarin Inc.
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using System.IO;
using Poupou.SvgPathConverter;

// This sample shows how you can use the library to converts every SVG path inside FontAwesome into a MonoTouch.Dialog
// based application to show them all. Since MonoTouch uses C# and iOS is CoreGraphics based then the parameters (and
// the extra code generation) are hardcoded inside the sample.

class Program {

	static void Usage (string error, params string[] values)
	{
		Console.WriteLine ("Usage: convert-font-awesome <font-directory> [generated-file.cs]");
        if (error != null) {
            Console.WriteLine(error, values);
            Console.ReadLine();
        }
		Environment.Exit (1);
	}

	public static int Main (string[] args)
	{
        if (args.Length < 1) {
            var awesomeDir = @"..\..\Font-Awesome";
            if (!Directory.Exists(awesomeDir))
                Usage("error: Path to FontAwesome directory required");
            else
                args = new[] { awesomeDir, "FontAwesome-Generated.cs" };
        }

		string font_dir = args [0];
		string css_file = Path.Combine (font_dir, "css/font-awesome.css");
		if (!File.Exists (css_file))
			Usage ("error: Missing '{0}' file.", css_file);

		string svg_file = Path.Combine (font_dir, "font/fontawesome-webfont.svg");
		if (!File.Exists (svg_file))
			Usage ("error: Missing '{0}' file.", svg_file);

		TextWriter writer = (args.Length < 2) ? Console.Out : new StreamWriter (args [1]);

        ISourceFormatter code = null;
        
        var fontWriter = new FontAwesomeWriter();
        var fType = FormatterTypes.XwtDrawingContext;
        Action<TextWriter, string> writeElement = null;
        if (fType.HasFlag(FormatterTypes.CoreGraphics)) {
            // MonoTouch uses C# and CoreGraphics
            code = new CSharpCoreGraphicsFormatter(writer);
            fontWriter.PrologueCoreGraphics(writer);
            writeElement = (w,name)=>w.WriteLine("\t\tvar {0}_element = new ImageStringElement (\"{0}\", GetAwesomeIcon ({0}));", name);
        } else if (fType.HasFlag(FormatterTypes.XwtDrawingContext)) {
            // MonoTouch uses C# and CoreGraphics
            code = new XwtContextFormatter(writer);
            fontWriter.PrologueXwtGraphics(writer);
            writeElement = (w, name) => w.WriteLine("\t\tvar {0}_element = new ImageStringElement (\"{0}\", GetAwesomeIcon ({0}));", name);
        }
        fontWriter.Write(writer, File.ReadAllLines(svg_file), File.ReadAllLines(css_file), code,writeElement);
      

        writer.Close ();

		return 0;
	}
}