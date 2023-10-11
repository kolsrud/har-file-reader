using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace HarFileReader
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("fileName\t" + Printer.Print(Entry.Headers.ToArray()));
			foreach (var arg in args)
			{
				var baseFileName = Path.GetFileName(arg);
				var dirName = Path.GetDirectoryName(arg);
				dirName = string.IsNullOrWhiteSpace(dirName) ? "." : dirName;
				var allFiles = Directory.GetFiles(dirName, baseFileName);
				foreach (var file in allFiles)
				{
					var fileName = Path.GetFileName(file);
					var txt = new StreamReader(file);
					var fullFile = JObject.Parse(txt.ReadToEnd());
					var entries = fullFile["log"]["entries"].ToObject<List<Entry>>();
					foreach (var entry in entries)
					{
						Console.WriteLine(Printer.Print(fileName, entry));
					}
				}
			}
		}
	}

	internal class Entry : Printer
	{
		public DateTime startedDateTime;
		public double time;
		public Request request;
		public Response response;
		public Timings timings;
		public string _fromCache;
        public string _resourceType;

        public override string ToString()
		{
			return Print(startedDateTime.ToString("O"), time, request, response, timings, _fromCache ?? "No", _resourceType);
		}

		public static IEnumerable<string> Headers => new[] {nameof(startedDateTime), nameof(time)}.Concat(Request.Headers).Concat(Response.Headers).Concat(Timings.Headers).Append(nameof(_fromCache)).Append(nameof(_resourceType));
	}

	internal abstract class Printer
	{
		public static string Print(params object[] objs)
		{
			return string.Join("\t", objs.Select(o => o.ToString()));
		}
	}

	internal class Request : Printer
	{
		public string method;
		public string url;
		public int bodySize;

		public override string ToString()
		{
			return Print(method,url,bodySize);
		}

		public static IEnumerable<string> Headers => new[] { nameof(method), nameof(url), "req." + nameof(bodySize) };
	}

	internal class Response : Printer
	{
		public int status;
		public string statusText;
		public Content content;
		public int bodySize;
		public int _transferSize;

		public override string ToString()
		{
			return Print(status, statusText, bodySize, _transferSize, content);
		}

		public static IEnumerable<string> Headers => new[] { nameof(status), nameof(statusText), "rsp." + nameof(bodySize), nameof(_transferSize) }.Concat(Content.Headers);
	}

	internal class Content : Printer
	{
		public int size;
		public override string ToString()
		{
			return Print(size);
		}

		public static IEnumerable<string> Headers => new[] { nameof(size) };
	}

	internal class Timings : Printer
	{
		public double blocked;
		public double dns;
		public double ssl;
		public double connect;
		public double send;
		public double wait;
		public double receive;
		public double _blocked_queueing;

		public override string ToString()
		{
			return Print(blocked, connect, wait, receive, _blocked_queueing);
		}

		public static IEnumerable<string> Headers => new[] { nameof(blocked), nameof(connect), nameof(wait), nameof(receive), nameof(_blocked_queueing) };
	}
}
