using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FreshMvvm;

namespace HenrySample
{
	// all PageModels must inherit from FreshBasePageModel and implement this eg. use HenryBasePageModel
	public interface IHenryBasePageModel
	{
		string Name { get; set; }		// contains brief name from url & registration
		string Data { get; set; }		// contains data from url, so is a string (or null) but is often an integer id as a string
		Task<bool> Loading();				// implement this async method to load model, optionally based on Data. Return true to proceed or false to cancel navigation
	}

	public class HenryBasePageModel : FreshBasePageModel, IHenryBasePageModel
	{
		public string Name { get; set; }
		public string Data { get; set; }

		public virtual async Task<bool> Loading()
		{
			//	Debug.WriteLine(this.GetType().Name + " NavigateIn");
			return true;
		}

		//public virtual async Task<bool> Loading()
		//{
		//	Debug.WriteLine(this.GetType().Name + " NavigateIn");
		//	return true;
		//}

		//protected override void ViewIsAppearing(object sender, EventArgs e)
		//{
		//	base.ViewIsAppearing(sender, e);
		//	Debug.WriteLine(this.GetType().Name + " Appearing");
		//}

		//protected override void ViewIsDisappearing(object sender, EventArgs e)
		//{
		//	base.ViewIsDisappearing(sender, e);
		//	Debug.WriteLine(this.GetType().Name + " Disappearing");
		//}
	}
}
