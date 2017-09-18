using NLog;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel;
using Shoko.Desktop.ViewModel.Server;
using Shoko.Models.Client;
using Shoko.Models.Server;

namespace Shoko.Desktop.Forms
{
    /// <summary>
    /// Interaction logic for ManageCustomTags.xaml
    /// </summary>
    public partial class ManageCustomTags : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ManageCustomTags()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnAddCustomTag.Click += btnAddCustomTag_Click;
            btnClose.Click += new RoutedEventHandler(btnClose_Click);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        void btnAddCustomTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                string res = "";

                if (string.IsNullOrWhiteSpace(txtTagName.Text))
                {
                    MessageBox.Show(Shoko.Commons.Properties.Resources.CustomTag_EnterName, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTagName.Focus();
                    return;
                }

                VM_CustomTag contract = new VM_CustomTag();
                contract.TagName = txtTagName.Text.Trim();
                contract.TagDescription = txtTagDescription.Text.Trim();


                CL_Response<CustomTag> resp = VM_ShokoServer.Instance.ShokoServices.SaveCustomTag(contract);

                if (!string.IsNullOrEmpty(resp.ErrorMessage))
                {
                    MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    VM_ShokoServer.Instance.AllCustomTags.Add((VM_CustomTag) resp.Result);
                    VM_ShokoServer.Instance.ViewCustomTagsAll.Refresh();
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

        private void CommandBinding_DeleteCustomTag(object sender, ExecutedRoutedEventArgs e)
        {
            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                Cursor = Cursors.Wait;
                string res = "";


                // NOTE if we are disabling an image we should also make sure it is not the default
                VM_CustomTag tag = null;
                if (obj.GetType() == typeof(VM_CustomTag))
                {
                    tag = (VM_CustomTag)obj;
                    res = VM_ShokoServer.Instance.ShokoServices.DeleteCustomTag(tag.CustomTagID);
                }

                if (res.Length > 0)
                {
                    MessageBox.Show(res, Shoko.Commons.Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    VM_CustomTag ctagToRemove = null;
                    foreach (VM_CustomTag ctag in VM_ShokoServer.Instance.AllCustomTags)
                    {
                        if (ctag.CustomTagID == tag.CustomTagID)
                        {
                            ctagToRemove = ctag;
                            break;
                        }

                    }

                    if (ctagToRemove != null)
                    {
                        VM_ShokoServer.Instance.AllCustomTags.Remove(ctagToRemove);
                        VM_ShokoServer.Instance.ViewCustomTagsAll.Refresh();

                        //TODO: Custom Tags -  update any cached data for affected anime
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
    }
}
