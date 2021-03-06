// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TranslationslistViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    using System.Windows.Input;

    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// Define the TranslationslistViewModel type.
    /// </summary>
    public class TranslationsListViewModel : BaseViewModel
    {
        public TranslationsListViewModel()
        {
            this.IsDataLoaded = false;
            this.AvailableTranslations = new ObservableCollection<ObservableTranslationItem>();
            this.AvailableTranslations.CollectionChanged += AvailableTranslationsCollectionChanged;
        }

        #region Properties
        public ObservableCollection<ObservableTranslationItem> AvailableTranslations { get; private set; }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set
            {
                if (value == isDataLoaded)
                    return;

                isDataLoaded = value;

                base.RaisePropertyChanged(() => IsDataLoaded);
            }
        }

        private bool anyTranslationsDownloaded;
        public bool AnyTranslationsDownloaded
        {
            get { return anyTranslationsDownloaded; }
            set
            {
                if (value == anyTranslationsDownloaded)
                    return;

                anyTranslationsDownloaded = value;

                base.RaisePropertyChanged(() => AnyTranslationsDownloaded);
            }
        }
        #endregion Properties

        #region Public methods
        public async void LoadData()
        {
            var list = await TranslationListTask.DownloadTranslations(true, "tag");
            if (list == null)
                return;

            foreach (var item in list)
            {
                this.AvailableTranslations.Add(new ObservableTranslationItem(item));
            }

            this.IsDataLoaded = true;
        }
        #endregion Public methods

        #region Event handlers
        void AvailableTranslationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableTranslationItem item in e.NewItems)
                {
                    item.DownloadComplete += TranslationDownloadComplete;
                    item.DeleteComplete += TranslationDeleteComplete;
                    item.NavigateRequested += TranslationNavigateRequested;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableTranslationItem item in e.OldItems)
                {
                    item.DownloadComplete -= TranslationDownloadComplete;
                    item.DeleteComplete -= TranslationDeleteComplete;
                    item.NavigateRequested -= TranslationNavigateRequested;
                }
            }            
        }

        private void TranslationDownloadComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;
            translation.Exists = true;

            // Hack to update list after download / delete completed
            AvailableTranslations.Remove(translation);
            AvailableTranslations.Add(translation);
        }

        private void TranslationDeleteComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;
            translation.Exists = false;

            // Hack to update list after download / delete completed
            AvailableTranslations.Remove(translation);
            AvailableTranslations.Add(translation);
        }

        private void TranslationNavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;
            if (NavigateRequested != null)
                NavigateRequested(sender, e);
        }
        #endregion

        public event EventHandler NavigateRequested;
    }
}
