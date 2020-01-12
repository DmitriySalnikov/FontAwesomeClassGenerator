using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace FontAwesomeClassGenerator
{
	class Program
	{
		struct Glyph
		{
			public string name;
			public string code;
			public char _char;
		}

		class IconsData
		{
			[JsonProperty("styles")]
			public string[] Styles;
			[JsonProperty("unicode")]
			public string code;
		}

		static void Main(string[] args)
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, IconsData>>(File.ReadAllText("icons.json"));
			var styles = new Dictionary<string, HashSet<Glyph>>();
			var ranges = new Dictionary<string, List<ushort>>();

			foreach (var d in data)
			{
				var glyph = new Glyph() { name = d.Key, code = d.Value.code, _char = (char)ushort.Parse(d.Value.code, System.Globalization.NumberStyles.HexNumber) };

				foreach (var s in d.Value.Styles)
				{
					if (!styles.ContainsKey(s))
						styles.Add(s, new HashSet<Glyph>());
					if (!ranges.ContainsKey(s))
						ranges.Add(s, new List<ushort>());

					styles[s].Add(glyph);
					ranges[s].Add(glyph._char);
				}
			}

			Directory.CreateDirectory("Output");
			var fileName = "Output/FontAwesome.cs";
			if (File.Exists(fileName))
				File.Delete(fileName);
			var writer = new StreamWriter(File.OpenWrite(fileName), Encoding.UTF8);

			writer.WriteLine("// Use for search with visuals https://fontawesome.com/icons \n");
			writer.WriteLine("namespace FontAwesome");
			writer.WriteLine("{");

			var varPrefix = "public const string _";
			foreach (var style in styles)
			{
				var name = style.Key.First().ToString().ToUpper() + style.Key.Substring(1);
				writer.WriteLine("\tclass FontAwesome" + name);
				writer.WriteLine("\t{");

				StringBuilder sb = new StringBuilder();
				foreach (var glyph in style.Value)
				{
					writer.WriteLine($"\t\t{varPrefix}{glyph.name.Replace("-", "_")} = \"\\u{ glyph.code}\";//{ glyph._char }");
					sb.Append(glyph._char);
				}

				ranges[style.Key].Sort();
				var res = BuildRanges(ranges[style.Key]);
				writer.Write("\t\tpublic static ushort[] Ranges = new ushort[] { ");
				foreach (var r in res)
				{
					writer.Write($"{r}, ");
				}
				writer.Write(" };\n");
				writer.WriteLine($"\t\tpublic const string AllIconsString = \"{sb.ToString()}\";");

				writer.WriteLine("\t}");
			}

			writer.WriteLine("}");

			writer.Close();
		}

		static ushort[] BuildRanges(List<ushort> rawRanges)
		{
			List<ushort> res = new List<ushort>();
			res.Add(rawRanges.First());
			var prevVal = rawRanges.First();
			for (int i = 1; i < rawRanges.Count; i++)
			{
				var curChar = rawRanges[i];
				if (curChar - res.Last() > 1)
				{
					res.Add(prevVal);
					res.Add(curChar);
				}
				prevVal = curChar;
			}
			res.Add(rawRanges.Last());

			return res.ToArray();
		}
	}
}
