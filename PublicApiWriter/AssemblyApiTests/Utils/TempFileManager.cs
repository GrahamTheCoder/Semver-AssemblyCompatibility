using System;
using System.Collections.Generic;
using System.IO;

namespace Gtc.AssemblyApiTests.Utils
{
    public class TempFileManager : IDisposable
    {
        private readonly List<FileInfo> m_Files = new List<FileInfo>();
        private readonly DirectoryInfo m_TempDirectory;

        public TempFileManager()
        {
            var randomDirectoryName = Path.GetTempFileName().Substring(0,8);
            var newTempDirectoryPath = Path.Combine(Path.GetTempPath(), randomDirectoryName);
            m_TempDirectory = new DirectoryInfo(newTempDirectoryPath);
            m_TempDirectory.Create();
        }


        public FileInfo GetNew()
        {
            var newTempFilePath = Path.Combine(m_TempDirectory.FullName, Path.GetTempFileName());
            var fileInfo = new FileInfo(newTempFilePath);
            m_Files.Add(fileInfo);
            return fileInfo;
        }

        public void Dispose()
        {
            foreach (var fileInfo in m_Files)
            {
                fileInfo.Delete();
            }
        }
    }
}