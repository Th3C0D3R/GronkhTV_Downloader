using GronkhTV_DL.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static GronkhTV_DL.dialog.classes.QData;

namespace GronkhTV_DL.dialog
{
	/// <summary>
	/// Interaktionslogik für SelectQualityDialog.xaml
	/// </summary>
	public partial class SelectQualityDialog : Window
	{

		public SelectQualityDialog(List<Quality> q)
        {
            ListQualities = q;
			SelectedQuality = q.FirstOrDefault();
            InitializeComponent();
			Topmost = true;
		}

		private void btnSelectQuality_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			SelectedQuality = (Quality)cbQuality.SelectedItem;
			Close();
		}
	}

}

