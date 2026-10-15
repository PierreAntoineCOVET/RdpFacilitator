using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RdpFacilitator
{
    public class FileUpdater
    {
        private readonly string FilePath;
        private readonly SaveMode SaveMode;
        private readonly string SelectedMonitors;
        private readonly string? Address;
        private const string SaveComplement = ".save";
        private const string FullAddressKey = "full address:s:";
        private const string MultiMonitorKey = "use multimon:i:";
        private const string SelectedMonitorsKey = "selectedmonitors:s:";

        public FileUpdater(string filePath, SaveMode saveMode, string selectedMonitors, string? address)
        {
            FilePath = Path.GetFullPath(filePath);
            SaveMode = saveMode;
            SelectedMonitors = selectedMonitors;
            Address = address;
        }

        public string UpdateRdpFile()
        {
            if(!File.Exists(FilePath))
            {
                CreateRdpFile();
                return FilePath;
            }

            UpdateExistingRdpFile();

            return FilePath;
        }

        private void CreateRdpFile()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new FileNotFoundException($"File {FilePath} does not exist. Use -a to give the remote computer address and create it.");
            }

            File.WriteAllLines(FilePath, new[]
            {
                FullAddressKey + Address,
                MultiMonitorKey + "1",
                SelectedMonitorsKey + SelectedMonitors
            }, Encoding.Unicode);
        }

        private void UpdateExistingRdpFile()
        {
            Encoding encoding;
            List<string> fileContent;

            using (var reader = new StreamReader(FilePath, new UTF8Encoding(false), true))
            {
                fileContent = reader.ReadToEnd()
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .ToList();
                encoding = reader.CurrentEncoding;
            }

            if (fileContent.Count > 0 && fileContent[^1].Length == 0)
            {
                fileContent.RemoveAt(fileContent.Count - 1);
            }

            var hasChanged = false;

            if (!string.IsNullOrWhiteSpace(Address))
            {
                hasChanged |= SetValue(fileContent, FullAddressKey, Address);
            }

            hasChanged |= SetValue(fileContent, MultiMonitorKey, "1");
            hasChanged |= SetValue(fileContent, SelectedMonitorsKey, SelectedMonitors);

            if (!hasChanged)
            {
                return;
            }

            if(SaveMode != SaveMode.None)
            {
                File.Copy(FilePath, GetSaveFile(), true);
            }

            File.WriteAllLines(FilePath, fileContent, encoding);
        }

        private static bool SetValue(List<string> fileContent, string key, string value)
        {
            var index = fileContent.FindIndex(l => l.StartsWith(key, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                fileContent.Add(key + value);
                return true;
            }

            if (fileContent[index] == key + value)
            {
                return false;
            }

            fileContent[index] = key + value;
            return true;
        }

        private string GetSaveFile()
        {
            var currentFileName = Path.GetFileNameWithoutExtension(FilePath);
            var currentFilePath = Path.GetDirectoryName(FilePath)!;
            var currentFileExt = Path.GetExtension(FilePath);

            if(SaveMode == SaveMode.One)
            {
                return Path.Combine(currentFilePath, $"{currentFileName}{SaveComplement}{currentFileExt}");
            }
            else
            {
                var saveIncrementRegexp = new Regex($@"^{Regex.Escape(currentFileName + SaveComplement)}\.(\d+){Regex.Escape(currentFileExt)}$", RegexOptions.IgnoreCase);

                var maxSaveIndex = Directory.GetFiles(currentFilePath)
                    .Select(f => saveIncrementRegexp.Match(Path.GetFileName(f)))
                    .Where(m => m.Success)
                    .Select(m => int.TryParse(m.Groups[1].Value, out var parsedIndex) ? parsedIndex : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                return Path.Combine(currentFilePath, $"{currentFileName}{SaveComplement}.{++maxSaveIndex}{currentFileExt}");
            }
        }
    }
}
