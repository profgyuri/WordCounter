﻿namespace WordCounter.Lib.Processing
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    internal class FileReader
    {
        private readonly string _path;
        private readonly StringBuilder _stringBuilder;
        private readonly Dictionary<string, int> _dictionary;
        private readonly BackgroundWorker _backgroundWorker;

        public FileReader(string path)
        {
            _path = path;
            _stringBuilder = new StringBuilder();
            _dictionary = new Dictionary<string, int>();
            _backgroundWorker = new BackgroundWorker();

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += ReadByBytes;
            _backgroundWorker.RunWorkerCompleted += WorkerCompleted;
            _backgroundWorker.ProgressChanged += ProgressChanged;
        }

        public void StartWorker()
        {
            if (_backgroundWorker != null && !_backgroundWorker.IsBusy)
            {
                _backgroundWorker?.RunWorkerAsync();
            }            
        }

        public void CancelWorker()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var progress = e.ProgressPercentage;
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var message = "Done!";
        }

        private void ReadByBytes(object sender, DoWorkEventArgs e)
        {
            using var reader = new StreamReader(_path);
            var fileSize = reader.BaseStream.Length;
            var readBytes = 0;

            var byteValue = reader.Peek();

            while (byteValue >= 0)
            {
                HandleCharacterFromByteValue(byteValue);

                byteValue = reader.Read();
                readBytes++;

                var progressPercentage = (int)((double)readBytes / fileSize * 100);
                _backgroundWorker.ReportProgress(progressPercentage);
            }
        }

        private void HandleCharacterFromByteValue(int byteValue)
        {
            var character = (char)byteValue;

            if (char.IsWhiteSpace(character))
            {
                AddNewWordIfNotEmpty();
            }
            else
            {
                _stringBuilder.Append(character);
            }
        }

        private void AddNewWordIfNotEmpty()
        {
            if (_stringBuilder.Length > 0)
            {
                var word = _stringBuilder.ToString();

                AddOrIncreaseWordOccurance(word);

                _stringBuilder.Clear();
            }
        }

        private void AddOrIncreaseWordOccurance(string word)
        {
            if (!_dictionary.ContainsKey(word))
            {
                _dictionary[word] = 0;
            }

            _dictionary[word]++;
        }
    }
}