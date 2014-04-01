using System;
using System.Collections.Generic;
using System.IO;

namespace Poupou.SvgPathConverter {

    public class FontAwesomeWriter4 {

        public virtual void PrologueCoreGraphics (TextWriter writer) {
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

        public virtual void PrologueXwtGraphics (TextWriter writer) {
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

        public virtual void Write (TextWriter writer, IEnumerable<string> svgLines, IEnumerable<string> cssLines,
                                   ISourceFormatter code, Action<TextWriter, string, string> writeElement,
                                   Action<TextWriter, string, string> beforeParse) {
            var names = new Dictionary<string, string> ();
            foreach (var line in cssLines) {
                if (!line.StartsWith (".fa-", StringComparison.Ordinal))
                    continue;
                var p = line.IndexOf (':');
                if (p == -1)
                    continue;
                var name = line.Substring (1, p - 1).Replace ('-', '_');
                p = line.IndexOf ("content: \"\\", StringComparison.Ordinal);
                if (p == -1)
                    continue;
                var value = line.Substring (p + 11, 4);

                writeElement (writer, name, value);


                names[value]= name;
            }
            writer.WriteLine ("\t\t// total: {0}", names.Count);
            writer.WriteLine ();

            var parser = new SvgPathParser () {
                                                  Formatter = code
                                              };

            foreach (string line in svgLines) {
                if (!line.StartsWith ("<glyph unicode=\"&#x", StringComparison.Ordinal))
                    continue;
                string id = line.Substring (19, 4);
                string name;
                if (!names.TryGetValue (id, out name))
                    continue;
                int p = line.IndexOf (" d=\"") + 4;
                int e = line.LastIndexOf ('"');
                string data = line.Substring (p, e - p);
                beforeParse (writer, name, id);
                parser.Parse (data, CamelCase (name));
            }
            writer.WriteLine ("\t}");
            writer.WriteLine ("}");

        }
        public string CamelCase (string s) {
            var result = "";
            foreach (var word in s.Split ('_'))
                result += (word.Substring (0, 1).ToUpper () + word.Substring (1));
            return result;
        }
    }
}