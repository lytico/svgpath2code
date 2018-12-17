using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Poupou.SvgPathConverter
{

	public class FontAwesomeWriter5
	{

		public virtual void PrologueCoreGraphics (TextWriter writer)
		{
			// MonoTouch uses C# and CoreGraphics
			writer.WriteLine ("// note: Generated file - do not modify - use convert-font-awesome to regenerate");
			writer.WriteLine ();
			writer.WriteLine ("using MonoTouch.CoreGraphics;");
			writer.WriteLine ("using MonoTouch.Dialog;");
			writer.WriteLine ("using MonoTouch.Foundation;");
			writer.WriteLine ("using MonoTouch.UIKit;");
			writer.WriteLine ();
			writer.WriteLine ("namespace Poupou.Awesome.Demo {");
			writer.WriteLine ();
			writer.WriteLine ("\t[Preserve]");
			writer.WriteLine ("\tpublic partial class Elements {");
		}

		public virtual void PrologueXwtGraphics (TextWriter writer)
		{
			writer.WriteLine ("// note: Generated file - do not modify - use convert-font-awesome to regenerate");
			writer.WriteLine ();
			writer.WriteLine ("using Xwt;");
			writer.WriteLine ("using Xwt.Drawing;");
			writer.WriteLine ();
			writer.WriteLine ("namespace Limada.XwtAwesome {");
			writer.WriteLine ();
			writer.WriteLine ("\t[Preserve]");
			writer.WriteLine ("\tpublic partial class FontIcons {");
		}

		public virtual void Write (TextWriter writer, Stream svg,
					   ISourceFormatter code, Action<TextWriter, string, string> writeElement,
					   Action<TextWriter, string, string> beforeParse)
		{
			var names = new Dictionary<string, string> ();


			var parser = new SvgPathParser () {
				Formatter = code
			};

			var doc = XDocument.Load (svg);

			var root = doc.Root;
			foreach (var font in root.Elements().First().Elements()) {
				foreach (var glyph in font.Elements ()) {
					if (glyph.Name.LocalName != "glyph")
						continue;
					var name = glyph.Attribute ("glyph-name")?.Value;
					var unicode = glyph.Attribute ("unicode")?.Value;
					var data = glyph.Attribute ("d")?.Value;
					beforeParse (writer, name,  ((int)unicode.First ()).ToString ("X4"));
					parser.Parse (data, $"Fa{CamelCase (name.Replace ('-', '_'))}");

				}
			}

			writer.WriteLine ("\t}");
			writer.WriteLine ("}");

		}

		public string CamelCase (string s)
		{
			var result = "";
			foreach (var word in s.Split ('_'))
				result += (word.Substring (0, 1).ToUpper () + word.Substring (1));
			return result;
		}
	}
}