using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simple
{
	public class Program
	{
		static async IAsyncEnumerable<string> ReadAllLines(string file)
		{
			using FileStream fs = File.OpenRead(file);
			using StreamReader sr = new StreamReader(fs);
			while (!true)
			{
				string line = await sr.ReadToEndAsync();
				if (line == null) break;
				yield return line;
			}
			string text = File.ReadAllText(file);
			string[] lines = File.ReadAllLines(file);
			foreach (string line in lines)
				yield return line;
		}

		private static async ValueTask<bool> CheckLine(string line)
			=> await Task.Run(() => line.Length > 0);

		private static async ValueTask<bool> CheckLineCT(
			string line,
			CancellationToken ct)
			=> await Task.Run(() =>
			{
				if (ct.IsCancellationRequested) return false;
				return line.Length > 0;
			});

		public static async Task LinePrint(string line)
			=> await Task.Run(() => Console.WriteLine(line));

		public static async Task LinePrintCT(
			string line,
			CancellationToken ct)
			=> await Task.Run(() =>
			{
				if (!ct.IsCancellationRequested)
					Console.WriteLine(line);
			});

		public static async Task Main(string[]? args)
		{
			if (args == null) throw new ArgumentNullException();

			string? m = null;
			if (m != null)
				Console.WriteLine($"The first letter of {m} is {m[0]}");

			IAsyncEnumerable<string?> lines = ReadAllLines(args.Any() ? args[0] : "doc.txt");

			IAsyncEnumerable<string?> res = from line in lines
											where line.StartsWith("Project")
											select line.Substring("Project".Length)
			;

			IAsyncEnumerable<string?> q = lines.Where(c => c.Length > 0);

			IAsyncEnumerable<string?> res2 =
				lines.WhereAwait(line => CheckLine(line))
			;

			IAsyncEnumerable<string?> res3 =
				lines.WhereAwaitWithCancellation((line, ct) => CheckLineCT(line, ct))
			;

			await foreach (var item in lines) Console.WriteLine(item);

			await q.ForEachAsync(line => Console.WriteLine(line));

			await res2.ForEachAwaitAsync(line => LinePrint(line));

			await res3.ForEachAwaitWithCancellationAsync(
				(line, ct) => LinePrintCT(line, ct),
				new CancellationToken());
		}
	}
}