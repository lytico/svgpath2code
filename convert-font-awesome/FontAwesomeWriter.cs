// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//  Lytico (www.limada.org)
//
// Copyright 2012 Xamarin Inc.
//
// Licensed under the GNU LGPL 2 license only (no "later versions")

using System;
using System.Collections.Generic;
using System.IO;

namespace Poupou.SvgPathConverter {
    
    public class FontAwesomeWriter {

        public virtual void PrologueCoreGraphics(TextWriter writer) {
            // MonoTouch uses C# and CoreGraphics
            writer.WriteLine("// note: Generated file - do not modify - use convert-font-awesome to regenerate");
            writer.WriteLine();
            writer.WriteLine("using MonoTouch.CoreGraphics;");
            writer.WriteLine("using MonoTouch.Dialog;");
            writer.WriteLine("using MonoTouch.Foundation;");
            writer.WriteLine("using MonoTouch.UIKit;");
            writer.WriteLine();
            writer.WriteLine("namespace Poupou.Awesome.Demo {");
            writer.WriteLine();
        }

        public virtual void PrologueXwtGraphics(TextWriter writer) {
            writer.WriteLine("// note: Generated file - do not modify - use convert-font-awesome to regenerate");
            writer.WriteLine();
            writer.WriteLine("using Xwt;");
            writer.WriteLine("using Xwt.Drawing;");
            writer.WriteLine();
            writer.WriteLine("namespace Limada.XwtAwesome {");
            writer.WriteLine();
        }

        public virtual void Write(TextWriter writer, IEnumerable<string> svgLines, IEnumerable<string> cssLines, ISourceFormatter code, Action<TextWriter, string> writeElement) {

            writer.WriteLine("\t[Preserve]");
            writer.WriteLine("\tpublic partial class Elements {");
            
            var names = new Dictionary<string, string>();
            foreach (var line in cssLines) {
                if (!line.StartsWith(".icon-", StringComparison.Ordinal))
                    continue;
                var p = line.IndexOf(':');
                var name = line.Substring(1, p - 1).Replace('-', '_');
                p = line.IndexOf("content: \"\\", StringComparison.Ordinal);
                if (p == -1)
                    continue;
                var value = line.Substring(p + 11, 4);
                writer.WriteLine("\t\t// {0} : {1}", name, value);
                writeElement(writer, name);

                writer.WriteLine();
                names.Add(value, name);
            }
            writer.WriteLine("\t\t// total: {0}", names.Count);
            writer.WriteLine();

            var parser = new SvgPathParser() {
                Formatter = code
            };

            foreach (string line in svgLines) {
                if (!line.StartsWith("<glyph unicode=\"&#x", StringComparison.Ordinal))
                    continue;
                string id = line.Substring(19, 4);
                string name;
                if (!names.TryGetValue(id, out name))
                    continue;
                int p = line.IndexOf(" d=\"") + 4;
                int e = line.LastIndexOf('"');
                string data = line.Substring(p, e - p);
                parser.Parse(data, name);
            }
            writer.WriteLine("\t}");
            writer.WriteLine("}");

        }
    }
}