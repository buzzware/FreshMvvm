using System;
using System.Threading.Tasks;
using FreshMvvm;

namespace HenrySample
{

	public interface IHenryBasePageModel
	{
		string Name { get; set; }
		string Data { get; set; }
Task<bool> NavigateIn();
	}

	public class HenryBasePageModel : FreshBasePageModel, IHenryBasePageModel
	{
		public string Name { get; set; }
		public string Data { get; set; }

		public virtual async Task<bool> NavigateIn()
		{
			return true;
		}
	}
}
