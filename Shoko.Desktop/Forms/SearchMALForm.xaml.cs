using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Shoko.Commons.Extensions;
using Shoko.Desktop.UserControls.Settings;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for SearchMALForm.xaml
    /// </summary>
    public partial class SearchMALForm : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty IsSearchProperty = DependencyProperty.Register("IsSearch",
            typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty IsExistingProperty = DependencyProperty.Register("IsExisting",
            typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty HasWebCacheRecProperty = DependencyProperty.Register("HasWebCacheRec",
            typeof(bool), typeof(SearchMALForm), new UIPropertyMetadata(false, null));

        public static readonly DependencyProperty CrossRef_AniDB_MALResultProperty = DependencyProperty.Register("CrossRef_AniDB_MALResult",
            typeof(List<VM_CrossRef_AniDB_MAL_Response>), typeof(SearchMALForm), new UIPropertyMetadata(null, null));

        public static readonly DependencyProperty MALSearchResultsProperty = DependencyProperty.Register("MALSearchResults",
            typeof(List<VM_MALAnime_Response>), typeof(SearchMALForm), new UIPropertyMetadata(null, null));

        public bool IsSearch
        {
            get { return (bool)GetValue(IsSearchProperty); }
            set { SetValue(IsSearchProperty, value); }
        }

        public bool IsExisting
        {
            get { return (bool)GetValue(IsExistingProperty); }
            set { SetValue(IsExistingProperty, value); }
        }

        public bool HasWebCacheRec
        {
            get { return (bool)GetValue(HasWebCacheRecProperty); }
            set { SetValue(HasWebCacheRecProperty, value); }
        }

        public List<VM_CrossRef_AniDB_MAL_Response> CrossRef_AniDB_MALResult
        {
            get { return (List<VM_CrossRef_AniDB_MAL_Response>)GetValue(CrossRef_AniDB_MALResultProperty); }
            set { SetValue(CrossRef_AniDB_MALResultProperty, value); }
        }

        public List<VM_MALAnime_Response> MALSearchResults
        {
            get { return (List<VM_MALAnime_Response>)GetValue(MALSearchResultsProperty); }
            set { SetValue(MALSearchResultsProperty, value); }
        }

        public static readonly DependencyProperty AnimeNameProperty = DependencyProperty.Register("AnimeName_MAL",
            typeof(string), typeof(SearchTraktForm), new UIPropertyMetadata("", null));

        public string AnimeName_MAL
        {
            get { return (string)GetValue(AnimeNameProperty); }
            set { SetValue(AnimeNameProperty, value); }
        }

        private int AnimeID = 0;
        public int? SelectedMALID = null;

        public SearchMALForm()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            rbExisting.Checked += new RoutedEventHandler(rbExisting_Checked);
            rbSearch.Checked += new RoutedEventHandler(rbSearch_Checked);

            hlURL.Click += new RoutedEventHandler(hlURL_Click);

            rbSearch.IsChecked = true;
            rbExisting.IsChecked = false;

            btnSearch.Click += new RoutedEventHandler(btnSearch_Click);
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
            btnUseThis.Click += new RoutedEventHandler(btnUseThis_Click);
            btnUseThisExisting.Click += new RoutedEventHandler(btnUseThisExisting_Click);

            btnChkCred.Click += new RoutedEventHandler(btnChkCred_Click);

            CrossRef_AniDB_MALResult = new List<VM_CrossRef_AniDB_MAL_Response>();
        }

        void btnUseThisExisting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = 0;
                int.TryParse(txtMALID.Text, out id);
                if (id <= 0)
                {
                    MessageBox.Show(Properties.Resources.Search_InvalidMAL, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtMALID.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtMALTitle.Text.Trim()))
                {
                    MessageBox.Show(Properties.Resources.Search_EnterTitle, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtMALTitle.Focus();
                    return;
                }

                // prompt to select details
                Window wdw = GetWindow(this);

                Cursor = Cursors.Wait;
                SelectMALStartForm frm = new SelectMALStartForm();
                frm.Owner = wdw;
                frm.Init(AnimeID, AnimeName_MAL, txtMALTitle.Text.Trim(), id, null, null);
                bool? result = frm.ShowDialog();
                if (result.Value)
                {
                    DialogResult = true;
                    Cursor = Cursors.Arrow;
                    Close();

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnUseThis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;

                foreach (VM_CrossRef_AniDB_MAL_Response xref in CrossRef_AniDB_MALResult)
                {
                    LinkAniDBToMAL(xref.MALID, xref.MALTitle, xref.StartEpisodeType, xref.StartEpisodeNumber);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SelectedMALID = null;
            Close();
        }

        private void LinkAniDBToMAL(int malID, string malTitle, int epType, int epNumber)
        {
            string res = VM_ShokoServer.Instance.ShokoServices.LinkAniDBMAL(AnimeID, malID, malTitle, epType, epNumber);
            if (res.Length > 0)
                MessageBox.Show(res, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                DialogResult = true;
                SelectedMALID = malID;
                Close();
            }
        }

        private void CommandBinding_UseThis(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VM_MALAnime_Response))
                {
                    Cursor = Cursors.Wait;
                    VM_MALAnime_Response searchResult = obj as VM_MALAnime_Response;

                    // prompt to select details
                    Window wdw = GetWindow(this);

                    Cursor = Cursors.Wait;
                    SelectMALStartForm frm = new SelectMALStartForm();
                    frm.Owner = wdw;
                    frm.Init(AnimeID, AnimeName_MAL, searchResult.title, searchResult.id, null, null);
                    bool? result = frm.ShowDialog();
                    if (result.Value)
                    {
                        DialogResult = true;
                        Cursor = Cursors.Arrow;
                        Close();

                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }


        void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            HasWebCacheRec = false;
            CrossRef_AniDB_MALResult.Clear();

            if (!VM_ShokoServer.Instance.ServerOnline) return;

            Cursor = Cursors.Wait;
            try
            {
                // first find what the community recommends

                VM_CrossRef_AniDB_MAL_Response xref = (VM_CrossRef_AniDB_MAL_Response)VM_ShokoServer.Instance.ShokoServices.GetMALCrossRefWebCache(AnimeID);
                if (xref != null)
                {
                    CrossRef_AniDB_MALResult.Add(xref);
                    HasWebCacheRec = true;
                }

                // now search MAL
                MALSearchResults = VM_ShokoServer.Instance.ShokoServices.SearchMAL(txtSearch.Text.Replace("`", "'").Trim()).CastList<VM_MALAnime_Response>();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }

        }

        void hlURL_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            int.TryParse(txtMALID.Text, out id);
            if (id <= 0) return;

            Uri uri = new Uri(string.Format(Models.Constants.URLS.MAL_Series, id));
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void hlURLWebCache_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            //Uri uri = new Uri(string.Format(Shoko.Models.Constants.URLS.MAL_Series, CrossRef_AniDB_MALResult.MALID));
            //Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
        }

        void rbSearch_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        void rbExisting_Checked(object sender, RoutedEventArgs e)
        {
            EvaluateRadioButtons();
        }

        private void EvaluateRadioButtons()
        {
            IsSearch = rbSearch.IsChecked.Value;
            IsExisting = rbExisting.IsChecked.Value;

            HasWebCacheRec = IsSearch && CrossRef_AniDB_MALResult != null;
        }

        public void Init(int animeID, string animeName, string searchCriteria)
        {
            AnimeID = animeID;
            AnimeName_MAL = animeName;
            txtSearch.Text = searchCriteria;
        }

        private void btnChkCred_Click(object sender, RoutedEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            MALSettings OP = new MALSettings();
            var host = new Window();
            host.Content = OP;
            host.Title = Properties.Resources.MAL_CheckCred;
            host.Width = 475;
            host.Height = 190;
            host.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            OP.Margin = new Thickness(5);
            host.ResizeMode = ResizeMode.NoResize;
            host.ShowDialog();
        }
    }
}
