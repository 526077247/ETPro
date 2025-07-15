﻿
namespace YooAsset
{
    internal class TempFileElement
    {
        public string TempFilePath { private set; get; }
        public string TempFileCRC { private set; get; }
        public long TempFileSize { private set; get; }

        /// <summary>
        /// 注意：原子操作对象
        /// </summary>
        public volatile int Result = 0;

        public TempFileElement(string filePath, string fileCRC, long fileSize)
        {
            TempFilePath = filePath;
            TempFileCRC = fileCRC;
            TempFileSize = fileSize;
        }
    }
}