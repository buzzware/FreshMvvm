using System;

using Xamarin.Forms;

namespace HenrySample
{
	public class BasePage : ContentPage
	{
		protected void LinkButtonClicked(object sender, System.EventArgs e)
		{
			var btn = sender as Button;
			if (btn != null)
				Henry.Goto(btn.Text,aDestructive:false);
		}		
	}
}

