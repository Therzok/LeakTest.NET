using System;
using System.IO;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace LeakTest.NET.ObjectGraph
{
	sealed class DotEngine : IDotEngine
	{
		public static IDotEngine Instance = new DotEngine();

		public string Run(GraphvizImageType imageType, string dot, string outputFileName)
		{
			// Maybe read from stdin?
			File.WriteAllText(outputFileName, dot);

			var imagePath = Path.ChangeExtension(outputFileName, "svg");
			var args = $"{outputFileName} -Tsvg -o\"{imagePath}\"";

			using (var process = System.Diagnostics.Process.Start("dot", args))
			{
				process.WaitForExit();
			}

			return imagePath;
		}
	}
}
