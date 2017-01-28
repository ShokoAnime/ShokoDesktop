using System;
using System.ComponentModel;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Shoko.Models.Server;
// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_RenameScript : RenameScript, INotifyPropertyChangedExt
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public new string ScriptName
        {
            get { return base.ScriptName; }
            set
            {
                base.ScriptName = this.SetField(base.ScriptName, value);
            }
        }

        public new string Script
        {
            get { return base.Script; }
            set
            {
                base.Script = this.SetField(base.Script, value, ()=>Script, ()=>ScriptNameLong);

            }
        }

        public new int IsEnabledOnImport
        {
            get { return base.IsEnabledOnImport; }
            set
            {
                base.IsEnabledOnImport = this.SetField(base.IsEnabledOnImport, value, ()=>IsEnabledOnImport, ()=>IsEnabledOnImportBool);
            }
        }

        public bool IsEnabledOnImportBool => IsEnabledOnImport==1;

        public string ScriptNameLong
        {
            get
            {
                if (IsEnabledOnImport == 1)
                    return Script + " (Run On Import)";
                return Script;
            }
        }


        public void Populate(RenameScript contract)
        {
            RenameScriptID = contract.RenameScriptID;
            ScriptName = contract.ScriptName;
            Script = contract.Script;
            IsEnabledOnImport = contract.IsEnabledOnImport;
        }



        public bool Save()
        {
            try
            {
                CL_Response<RenameScript> response = VM_ShokoServer.Instance.ShokoServices.SaveRenameScript(this);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Utils.ShowErrorMessage(response.ErrorMessage);
                    return false;
                }
                else
                {
                    Populate(response.Result);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
                return false;
            }
        }
    }
}
