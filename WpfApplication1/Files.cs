using System;
using System.Collections.Generic;
using System.IO;

namespace WpfApplication1
{
	public static class FileLength
	{
		public static int MaxRecordsPerFile { get; set; } = 5;
	}

	public static class Files
	{
		private static void AddToDataFile(string path, int key, string data, bool isDeleted = false)
		{
			string text = $"{isDeleted}, {key}, {data}";
			if (File.Exists(path))
			{
				File.AppendAllText(path, text + Environment.NewLine);
			}
			else
			{
				File.WriteAllText(path, text + Environment.NewLine);
			}
		}

		public static bool AddToIndexArea(string path, int key, string data)
		{
			int maxRecordsPerFile = CountFileMaxLength("length.txt");
			int blockNumber = 0;
			string text = $"{key}, {data}";

			while (true)
			{
				string indexFilePath = $"{path}{blockNumber}.txt";

				if (!File.Exists(indexFilePath))
				{
					File.WriteAllText(indexFilePath, text);
					AddToDataFile("dataFile.txt", key, data);
					return true;
				}

				string[] lines = File.ReadAllLines(indexFilePath);

				if (lines.Length == 0)
				{
					File.WriteAllText(indexFilePath, text);
					AddToDataFile("dataFile.txt", key, data);
					return true;
				}

				int lastRecordKey = int.Parse(lines[lines.Length - 1].Split(',')[0]);

				if (lines.Length < maxRecordsPerFile)
				{
					foreach (string line in lines)
					{
						if (int.TryParse(line.Split(',')[0], out int existingKey) && existingKey == key)
						{
							return false;
						}
					}

					InsertKeyIntoFile(indexFilePath, key, text, lines);
					AddToDataFile("dataFile.txt", key, data);
					return true;
				}

				if (key <= lastRecordKey && lines.Length == maxRecordsPerFile)
				{
					foreach (string line in lines)
					{
						if (int.TryParse(line.Split(',')[0], out int existingKey) && existingKey == key)
						{
							return false;
						}
					}

					FileLength.MaxRecordsPerFile *= 2;
					SetValueLengthFile("length.txt", FileLength.MaxRecordsPerFile);
					InsertKeyIntoFile(indexFilePath, key, text, lines);
					AddToDataFile("dataFile.txt", key, data);
					return true;
				}

				blockNumber++;
			}
		}

		private static void InsertKeyIntoFile(string filePath, int key, string text, string[] lines)
		{
			List<string> records = new List<string>(lines);
			List<int> keys = new List<int>();

			foreach (var line in lines)
			{
				keys.Add(int.Parse(line.Split(',')[0]));
			}

			int indexToInsert = Algorithm.BinarySearch(keys, key);
			if (indexToInsert < 0)
				indexToInsert = ~indexToInsert;

			records.Insert(indexToInsert, text);

			File.WriteAllLines(filePath, records);
		}

		private static int CountFileMaxLength(string lengthFile)
		{
			if (File.Exists(lengthFile))
			{
				return int.TryParse(File.ReadAllText(lengthFile), out int maxRecordsPerFile)
					? maxRecordsPerFile
					: FileLength.MaxRecordsPerFile;
			}
			
			File.WriteAllText(lengthFile, FileLength.MaxRecordsPerFile.ToString());
			return FileLength.MaxRecordsPerFile;
		}

		private static void SetValueLengthFile(string lengthFile, int newLength)
		{
			if (File.Exists(lengthFile))
			{
				File.WriteAllText(lengthFile, newLength.ToString());
			}
		}
	}
}