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
        private const string SaveComplement = ".save";

        public FileUpdater(string filePath, SaveMode saveMode, string selectedMonitors)
        {
            FilePath = filePath;
            SaveMode = saveMode;
            SelectedMonitors = selectedMonitors;
        }

        public string UpdateRdpFile()
        {
            if(!File.Exists(FilePath))
            {
                throw new FileNotFoundException("File does not exist");
            }

            if(SaveMode != SaveMode.None)
            {
                return SaveRdpFile();
            }

            return null;
        }

        private string SaveRdpFile()
        {
            var newFileName = GetSaveFile();
            var newFileContent = GetNewFileContent();
            saveFile(newFileName, newFileContent);

            return newFileName;
        }

        private void saveFile(string newFileName, string[] newFileContent)
        {
            if (File.Exists(newFileName))
            {
                File.Delete(newFileName);
            }

            File.WriteAllLines(newFileName, newFileContent);
        }

        private string[] GetNewFileContent()
        {
            var currentFileContent = File.ReadAllLines(FilePath);
            var newFileContent = new string[currentFileContent.Length];

            for (int i=0; i<currentFileContent.Length; i++)
            {
                var currentLine = currentFileContent[i];
                var newLine = string.Empty;

                if(currentLine.Contains("selectedmonitors"))
                {
                    var splitedLine = currentLine.Split(':');
                    splitedLine[splitedLine.Length - 1] = SelectedMonitors;
                    newLine = string.Join(':', splitedLine);
                }
                else
                {
                    newLine = currentLine;
                }

                newFileContent[i] = newLine;
            }

            return newFileContent;
        }

        private string GetSaveFile()
        {
            var currentFileName = Path.GetFileNameWithoutExtension(FilePath);
            var currentFilePath = Path.GetDirectoryName(FilePath);
            var currentFileExt = Path.GetExtension(FilePath);

            if(SaveMode == SaveMode.One)
            {
                return $"{currentFilePath}{Path.DirectorySeparatorChar}{currentFileName}{SaveComplement}{currentFileExt}";
            }
            else
            {
                var saveIncrementRegexp = new Regex($@".*{currentFileName}{SaveComplement}\.(\d+)\..*");

                var maxSaveIndex = Directory.GetFiles(currentFilePath)
                    .Select(f => saveIncrementRegexp.Match(Path.GetFileName(f)))
                    .Where(m => m.Success)
                    .Select(m => int.TryParse(m.Groups[1].Value, out var parsedIndex) ? parsedIndex : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                return $"{currentFilePath}{Path.DirectorySeparatorChar}{currentFileName}{SaveComplement}.{++maxSaveIndex}{currentFileExt}";
            }
        }
    }
}
