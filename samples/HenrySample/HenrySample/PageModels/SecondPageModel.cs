using System;
using System.Threading.Tasks;

namespace HenrySample
{
	public class SecondPageModel : HenryBasePageModel
	{

		public override async System.Threading.Tasks.Task<bool> NavigateIn()
		{
			await Task.Delay(2000);
			return true;
		}
	}
}
