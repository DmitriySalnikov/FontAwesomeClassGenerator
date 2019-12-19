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

			foreach (var d in data)
			{
				var glyph = new Glyph() { name = d.Key, code = d.Value.code };

				foreach (var s in d.Value.Styles)
				{
					if (!styles.ContainsKey(s))
						styles.Add(s, new HashSet<Glyph>());

					styles[s].Add(glyph);
				}
			}

			Directory.CreateDirectory("Output");
			var fileName = "Output/FontAwesome.cs";
			if (File.Exists(fileName))
				File.Delete(fileName);
			var writer = new StreamWriter(File.OpenWrite(fileName));

			writer.WriteLine("namespace FontAwesome");
			writer.WriteLine("{");

			var varPrefix = "public const string _";
			foreach (var style in styles)
			{
				var name = style.Key.First().ToString().ToUpper() + style.Key.Substring(1);
				writer.WriteLine("\tclass FontAwesome" + name);
				writer.WriteLine("\t{");

				foreach (var glyph in style.Value)
				{
					writer.WriteLine($"\t\t{varPrefix}{glyph.name.Replace("-", "_")} = \"\\u{ glyph.code}\";");
				}

				writer.WriteLine("\t}");
			}

			writer.WriteLine("}");

			writer.Close();
		}
	}
}
