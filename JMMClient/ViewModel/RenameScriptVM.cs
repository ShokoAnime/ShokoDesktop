using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JMMClient.ViewModel
{
	public class RenameScriptVM : INotifyPropertyChanged
	{
		public int? RenameScriptID { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				var args = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, args);
			}
		}

		private string scriptName = "";
		public string ScriptName
		{
			get { return scriptName; }
			set
			{
				scriptName = value;
				NotifyPropertyChanged("ScriptName");
			}
		}

		private string script = "";
		public string Script
		{
			get { return script; }
			set
			{
				script = value;
				NotifyPropertyChanged("Script");
			}
		}

		private int isEnabledOnImport = 0;
		public int IsEnabledOnImport
		{
			get { return isEnabledOnImport; }
			set
			{
				isEnabledOnImport = value;
				NotifyPropertyChanged("IsEnabledOnImport");
				IsEnabledOnImportBool = value == 1;
			}
		}

		private bool isEnabledOnImportBool = false;
		public bool IsEnabledOnImportBool
		{
			get { return isEnabledOnImportBool; }
			set
			{
				isEnabledOnImportBool = value;
				NotifyPropertyChanged("IsEnabledOnImportBool");
			}
		}

		private string scriptNameLong = "";
		public string ScriptNameLong
		{
			get { return scriptNameLong; }
			set
			{
				scriptNameLong = value;
				NotifyPropertyChanged("ScriptNameLong");
			}
		}

		public RenameScriptVM()
		{
		}

		public RenameScriptVM(JMMServerBinary.Contract_RenameScript contract)
		{
			Populate(contract);
		}

		public void Populate(JMMServerBinary.Contract_RenameScript contract)
		{
			this.RenameScriptID = contract.RenameScriptID;
			this.ScriptName = contract.ScriptName;
			this.Script = contract.Script;
			this.IsEnabledOnImport = contract.IsEnabledOnImport;

			this.ScriptNameLong = contract.ScriptName;
			if (IsEnabledOnImportBool)
				ScriptNameLong += " (Run On Import)";

		}

		public JMMServerBinary.Contract_RenameScript ToContract()
		{
			JMMServerBinary.Contract_RenameScript contract = new JMMServerBinary.Contract_RenameScript();
			contract.IsEnabledOnImport = this.IsEnabledOnImport;
			contract.RenameScriptID = this.RenameScriptID;
			contract.Script = this.Script;
			contract.ScriptName = this.ScriptName;
			
			return contract;
		}

		public bool Save()
		{
			try
			{
				JMMServerBinary.Contract_RenameScript_SaveResponse response = JMMServerVM.Instance.clientBinaryHTTP.SaveRenameScript(this.ToContract());
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					Utils.ShowErrorMessage(response.ErrorMessage);
					return false;
				}
				else
				{
					this.Populate(response.RenameScript);
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
