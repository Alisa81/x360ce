﻿using JocysCom.ClassLibrary.Runtime;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JocysCom.ClassLibrary.Controls
{
	/// <summary>
	/// Interaction logic for ErrorReportControl.xaml
	/// </summary>
	public partial class ErrorReportControl : UserControl
	{
		public ErrorReportControl()
		{
			InitializeComponent();
			ErrorsFolderTextBox.Text = LogHelper.Current.LogsFolder;
			MainBrowser.LoadCompleted += MainBrowser_LoadCompleted;
			RefreshErrorsComboBox();
		}

		void RefreshErrorsComboBox()
		{
			var dir = new DirectoryInfo(LogHelper.Current.LogsFolder);
			var errors = dir.GetFiles("*.htm").OrderByDescending(x => x.CreationTime).ToArray();
			ErrorComboBox.ItemsSource = errors;
			ErrorComboBox.DisplayMemberPath = nameof(FileInfo.Name);
			if (errors.Length > 0)
				ErrorComboBox.SelectedIndex = 0;
			else
				MainBrowser.Navigate("about:blank");
		}

		private void MainBrowser_LoadCompleted(object sender, NavigationEventArgs e)
		{
			var doc = (IHTMLDocument3)MainBrowser.Document;
			if (doc == null)
				return;
			var body = doc.getElementsByTagName("body").OfType<IHTMLElement>().First();
			if (body == null)
				return;
			body.insertAdjacentHTML("afterbegin", "<p>Hi,</p><p></p><p>I would like to report a problem. Error details attached below:</p>");
			body.setAttribute("contentEditable", "true");
		}

		private void OpenErrorsFolder_Click(object sender, RoutedEventArgs e)
		{
			ControlsHelper.OpenPath(ErrorsFolderTextBox.Text);
		}

		public void SetErrors(string errorsFolder = null)
		{
			errorsFolder = LogHelper.Current.LogsFolder;
		}

		private void ErrorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = ErrorComboBox.SelectedItem as FileInfo;
			if (item == null)
			{
				MainBrowser.Navigate("about:blank");
			}
			else
			{
				var uri = new System.Uri(item.FullName);
				MainBrowser.Navigate(uri.AbsoluteUri);
			}
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			var win = (Window)Parent;
			win.DialogResult = false;
		}

		private void ClearErrorsButton_Click(object sender, RoutedEventArgs e)
		{
			var form = new MessageBoxForm();
			form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			ControlsHelper.CheckTopMost(form);
			var result = form.ShowForm("Do you want to clear all errors?", "Clear Errors?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button2);
			if (result != System.Windows.Forms.DialogResult.Yes)
				return;
			var dir = new DirectoryInfo(LogHelper.Current.LogsFolder);
			var fis = dir.GetFiles("*.htm").OrderByDescending(x => x.CreationTime).ToArray();
			foreach (var fi in fis)
			{
				try
				{
					fi.Delete();
				}
				catch (Exception)
				{
				}
			}
			RefreshErrorsComboBox();
		}

		private void OpenMailButton_Click(object sender, RoutedEventArgs e)
		{
			ControlsHelper.OpenUrl("mailto://" + ToEmailTextBox.Text);
		}
	}
}