using System;
using System.Threading.Tasks;
using FreshMvvm;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Diagnostics;

namespace HenrySample
{

	public class PagePathSegment
	{
		public string Name;
		public IHenryBasePageModel Model;
		public string Data;
	}

	public class Henry
	{
		public Henry()
		{
		}



		public static void RegisterContainer<T>(string aName) where T : class, IFreshNavigationService
		{
			FreshIOC.Container.Register(typeof(T), aName).AsSingleton();
		}

		public static void RegisterPageModel<T>(string aName = null) where T : IHenryBasePageModel
		{
			if (aName == null)
				aName = typeof(T).Name;
			FreshIOC.Container.Register(typeof(T), aName).AsSingleton();
		}

		public static IFreshNavigationService ResolveContainer(string aName)
		{
			var containerType = FreshIOC.Container.Resolve<Type>(aName);
			if (containerType == null)
				return null;
			return Activator.CreateInstance(containerType) as IFreshNavigationService;
		}

		public static IHenryBasePageModel ResolvePageModel(string aName)
		{
			var modelType = FreshIOC.Container.Resolve<Type>(aName);
			if (modelType == null)
				return null;
			return Activator.CreateInstance(modelType) as IHenryBasePageModel;
		}

		static Page GetCurrentPage()
		{
			var container = App.Current.MainPage;
			return container?.Navigation?.NavigationStack?.Last() as Page;
		}

		static FreshBasePageModel GetCurrentPageModel() {
			return GetCurrentPage()?.GetModel();
		}

		public static IPageModelCoreMethods GetCoreMethods()
		{
			return GetCurrentPageModel()?.CoreMethods;
		}

		public static string GetCurrentUrl()
		{
			if (CurrentContainer == null)
				return null;
			return UrlOfContainer(CurrentContainer);
		}

		static string UrlOfContainer(IFreshNavigationService currentContainer)
		{
			var concreteContainer = currentContainer as Page;
			var navStack = concreteContainer.Navigation.NavigationStack;
			var parts = List<string>();
			return navStack.Select((n) =>
			{
				var m = n.GetModel();
				if (m == null)
					return;
				
			});
			foreach (var n in navStack)
			{
				p
			}
		}

		public static IFreshNavigationService CurrentContainer { get; private set; }

		public static bool isRelativeUrl(string aUrl)
		{
			return !(aUrl != null && aUrl[0] == '/');
		}

		public static void SelectContainer(IFreshNavigationService newContainer)
		{
			var concreteContainer = newContainer as Page;

			if (App.Current.MainPage == null)
				App.Current.MainPage = concreteContainer;
			else
				GetCoreMethods().SwitchOutRootNavigation(newContainer.NavigationServiceName);
		}

		//public static void PushPageByName(IFreshNavigationService aContainer, string aName, object aData)
		//{
		//	throw new NotImplementedException();
		//}

		public static async Task<bool> PushSegment(IFreshNavigationService aContainer, PagePathSegment s)
		{
			if (s.Model == null)
				s.Model = ResolvePageModel(s.Name);
			s.Model.Name = s.Name;
			s.Model.Data = s.Data;
			var concreteModel = s.Model as FreshBasePageModel;
			if (concreteModel == null)
				throw new Exception("Model was not a FreshBasePageModel");
			var page = FreshPageModelResolver.ResolvePageModel(s.Data, concreteModel);
			var result = await s.Model.NavigateIn();
			if (result)
				await aContainer.PushPage(page, concreteModel, modal: false, animate: false);
			return result;
		}

		public static async Task<bool> PushSegments(IFreshNavigationService aContainer, IEnumerable<PagePathSegment> aSegments)
		{
			foreach (var s in aSegments)
			{

				bool proceed = false;
				try
				{
					proceed = await PushSegment(aContainer, s);
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.ToString());
				}
				if (!proceed)
					return false;
			}
			return true;
		}

		public static IEnumerable<PagePathSegment> SegmentsFromUrlParts(IEnumerable<string> aUrl)
		{
			var result = new List<PagePathSegment>();
			foreach (var p in aUrl)
			{
				var s = new PagePathSegment();
				s.Name = p;
				s.Data = null;  // later set from following part
			}
			return result;
		}

		public static IEnumerable<PagePathSegment> SegmentsFromUrl(string aUrl)
		{
			return SegmentsFromUrlParts(splitUrl(aUrl));
		}

		//	if (CurrentContainer==null)

		//	string[] urlParts = splitUrl(aUrl);
		//	foreach (var p in urlParts)
		//	{
		//		PushPageByName(CurrentContainer, p, null);
		//	}
		//}

		static string[] splitUrl(string aUrl)
		{
			if (aUrl == null || aUrl == "")
				return new string[] { };
			return aUrl.Split('/').OfType<String>().ToArray();
		}

		// remove trailing slashes, other things
		static string NormalizeUrl(string aUrl)
		{
			return aUrl;
		}

		//static string CommonUrlBase(string currentUrl, string aUrl)
		//{
		//	throw new NotImplementedException();
		//}

		// assume 
		static IEnumerable<PagePathSegment> GetSegmentsToPop(IEnumerable<PagePathSegment> aExisting, IEnumerable<PagePathSegment> aNew)
		{
			var existing = aExisting.ToArray();
			var news = aNew.ToArray();
			var i = 0;
			var result = new List<PagePathSegment>();
			var divergent = false;
			foreach (var e in existing) {				
				var n = i < news.Length ? news[i] : null;
				if (n == null)
					result.Add(e);
				else {
					if (n.Name != e.Name)
						divergent = true;
					if (divergent)
						result.Add(e);
				}
				i+= 1;
			}
			return result;
		}

		static IEnumerable<PagePathSegment> GetSegmentsToPush(IEnumerable<PagePathSegment> aExisting, IEnumerable<PagePathSegment> aNew)
		{
			var existing = aExisting.ToArray();
			var news = aNew.ToArray();

			// after common, find new ones to push
			var i = 0;
			var result = new List<PagePathSegment>();
			var divergent = false;
			foreach (var n in news)
			{
				var e = i < existing.Length ? existing[i] : null;
				if (e == null)
					result.Add(n);
				else {
					if (n.Name != e.Name)
						divergent = true;
					if (divergent)
						result.Add(n);
				}
				i += 1;
			}
			return result;
		}

		public static async Task<bool> Goto(string aUrl, bool aDestructive = true)
		{
			bool result = false;

			aUrl = NormalizeUrl(aUrl);

			if (aUrl == null)
				throw new ArgumentNullException();

			string currentUrl = GetCurrentUrl();
			string[] urlParts = splitUrl(aUrl);
			IEnumerable<PagePathSegment> urlSegments = SegmentsFromUrlParts(urlParts);

			if (urlParts.Length == 0)
				throw new Exception("Not a valid Url");

			if (isRelativeUrl(aUrl))
			{
				if (CurrentContainer == null)
					throw new Exception("Cannot goto relative url when CurrentContainer == null");

				var segments = SegmentsFromUrlParts(urlParts);
				result = await PushSegments(CurrentContainer, segments);
			}
			else {
				if (aUrl == currentUrl)
				{
					result = true;
				}
				else {

					//IFreshNavigationService newContainer = null;
					//if (CurrentContainer == null)
					//{
					//	IFreshNavigationService container = null;
					//	var name = urlParts[0];
					//	container = ResolveContainer(name);
					//	if (container == null)
					//		throw new Exception("Not able to find container " + name);
					//	CurrentContainer = newContainer = container;
					//} 

					var currentUrlParts = splitUrl(currentUrl);

					IFreshNavigationService container;
					IEnumerable<PagePathSegment> segmentsToPop = null;
					IEnumerable<PagePathSegment> segmentsToPush = null;
					IFreshNavigationService newContainer = null;

					if (currentUrl == null || urlParts[0] != currentUrlParts[0])
					{
						newContainer = ResolveContainer(urlParts[0]);
						if (newContainer == null)
							throw new Exception("Container not found");
					}

					if (currentUrl == null)
					{
						segmentsToPush = SegmentsFromUrlParts(urlParts.ToList().GetRange(1, urlParts.Length - 1));
						container = newContainer;
					}
					else {
						if (newContainer != null)   // switching containers
						{
							if (aDestructive)
							{
								if (CurrentContainer != null)
								{
									await CurrentContainer.PopToRoot(animate: false);
								}
							}
							container = newContainer;
							var containerUrl = UrlOfContainer(newContainer);
							var containerUrlSegments = SegmentsFromUrl(containerUrl);
							segmentsToPop = GetSegmentsToPop(containerUrlSegments, urlSegments);
							segmentsToPush = GetSegmentsToPush(containerUrlSegments, urlSegments);
						}
						else {                      // same container
							container = CurrentContainer;
							var containerUrl = UrlOfContainer(newContainer);
							var containerUrlSegments = SegmentsFromUrl(containerUrl);
							segmentsToPop = GetSegmentsToPop(containerUrlSegments, urlSegments);
							segmentsToPush = GetSegmentsToPush(containerUrlSegments, urlSegments);
						}
					}

					if (newContainer != null)
						SelectContainer(newContainer);
					if (segmentsToPop != null)
						foreach (var s in segmentsToPop)
						{
							await container.PopPage(modal: false, animate: false);
						}
					if (segmentsToPush != null)
						result = await PushSegments(container, segmentsToPush);
					else
						result = true;
				}
				return result;
			}
		}

}
}
