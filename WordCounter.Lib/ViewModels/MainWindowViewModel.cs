﻿namespace WordCounter.Lib.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using WordCounter.Lib.Events;
    using WordCounter.Lib.Processing;

    public sealed class MainWindowViewModel : ViewModelBase
    {
        private bool _startButtonEnabled;
        private string _filePath;
        private FileReader _fileReader;
        private string _status;
        private int _progressPercentage;
        private ObservableCollection<KeyValuePair<string, int>> _sortedList;

        #region Public Properties
        public bool StartButtonEnabled
        {
            get => _startButtonEnabled;
            set
            {
                _startButtonEnabled = value;
                OnPropertyChanged(nameof(StartButtonEnabled));
                OnPropertyChanged(nameof(CancelButtonEnabled));
            }
        }

        public bool CancelButtonEnabled
        {
            get => !_startButtonEnabled;
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public string Status
        {
            get => _status;
            set 
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public int ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                _progressPercentage = value;
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        public ObservableCollection<KeyValuePair<string, int>> SortedList
        {
            get => _sortedList;
            set
            {
                _sortedList = value;
                OnPropertyChanged(nameof(SortedList));
                OnPropertyChanged(nameof(AccessibilityResultText));
            }
        }

        public string AccessibilityResultText 
        {
            get => SortedList != null && SortedList.Any() ? $"Out of {SortedList.Count} words, the most common word is {SortedList[0].Key} which occured in the file {SortedList[0].Value} times" : "";
        }
        #endregion

        public ICommand StartCommand { get; }
        public ICommand CancelCommand { get; }

        public MainWindowViewModel()
        {
            StartButtonEnabled = true;

            StartCommand = new RelayCommand(() =>
            {
                StartButtonEnabled = false;
                _fileReader = new FileReader(_filePath);
                _fileReader._workerProgressChangedEvent.WorkerProcessChanged += ProcessChanged;
                _fileReader._workerFinishedEvent.WorkerFinished += WorkerFinished;
                _fileReader.StartWorker();
            });
            CancelCommand = new RelayCommand(() =>
            {
                StartButtonEnabled = true;
                _fileReader?.CancelWorker();
            });
        }

        private void WorkerFinished(object o, WorkerFinishedEventArgs e)
        {
            Status = e.Message;
            SortedList = e.OrderedList;
            StartButtonEnabled = true;
            UnsubscribeEvents();
            _fileReader.Dispose();
        }

        private void ProcessChanged(object o, WorkerProgressChangedEventArgs e)
        {
            Status = $"{e.Progress}%";
            ProgressPercentage = e.Progress;
        }

        private void UnsubscribeEvents()
        {
            _fileReader._workerProgressChangedEvent.WorkerProcessChanged -= ProcessChanged;
            _fileReader._workerFinishedEvent.WorkerFinished -= WorkerFinished;
        }
    }
}
