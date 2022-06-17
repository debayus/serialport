using System;
using System.IO;

namespace serialport.Mahas.Helpers
{
	public static class MahasLog
	{
		public static string currentDirectory = Path.GetDirectoryName(Directory.GetCurrentDirectory());

		private static string GetDir(string category)
		{
			var fileName = $"{DateTime.Today:yyyyMMdd}.log";
			var dir = Path.Join(currentDirectory, "logs", category);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			var path = Path.Join(dir, fileName);
			if (!File.Exists(path))
			{
				File.Create(path);
			}
			return dir;
		}

		public static void Error(Exception ex)
		{
			var dir = GetDir("error");
			File.AppendAllText(dir, $"{DateTime.Now} {ex.Message}");
		}

		public static void Log(string log)
		{
			var dir = GetDir("logs");
			File.AppendAllText(dir, $"{DateTime.Now} {log}");
		}
	}
}

