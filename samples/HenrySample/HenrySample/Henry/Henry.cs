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

	public static class Henry
	{

		static string EnsureSuffix(string aName, string aSuffix)
		{
			if (!aName.EndsWith(aSuffix))
				aName += aSuffix;
			return aName;
		}

		static string TrimSuffix(string aName, string aSuffix)
		{
			if (aName.EndsWith(aSuffix))
				aName = aName.Substring(0, aName.Length - aSuffix.Length);
			return aName;
		}


		static string FullContainerName(string aName)
		{
			return EnsureSuffix(aName,"Container");
		}

		static string BriefContainerName(string aName)
		{
			return TrimSuffix(aName,"Container");
		}

		static string FullPageModelName(string aName)
		{
			return EnsureSuffix(aName, "PageModel");
		}

		static string BriefPageModelName(string aName)
		{
			return TrimSuffix(aName, "PageModel");
		}

		public static void RegisterContainer<T>(string aName) where T : class, IFreshNavigationService
		{
			aName = FullContainerName(aName);
			FreshIOC.Container.Register(typeof(T), aName+"Type");
		}


		public static void RegisterPageModel<T>(string aName = null) where T : IHenryBasePageModel
		{
			if (aName == null)
				aName = typeof(T).Name;
			aName = FullPageModelName(aName);
			FreshIOC.Container.Register(typeof(T), aName);
		}


		//public static IFreshNavigationService ResolveContainer(string aName)
		//{
		//	aName = NormalizeContainerName(aName);
		//	var containerType = FreshIOC.Container.Resolve<Type>(aName+"Type");



		//	if (containerType == null)
		//		return null;
		//	return Activator.CreateInstance(containerType) as IFreshNavigationService;
		//}

		static FreshTinyIoC.ResolveOptions quietResolveoptions = new FreshTinyIoC.ResolveOptions()
		{
			UnregisteredResolutionAction = FreshTinyIoC.UnregisteredResolutionActions.GenericsOnly,
			NamedResolutionFailureAction = FreshTinyIoC.NamedResolutionFailureActions.AttemptUnnamedResolution
		};
		public static ResolveType QuietResolve<ResolveType>(string aName) where ResolveType : class
		{
			var type = typeof(ResolveType);
			if (FreshTinyIOCBuiltIn.Current.CanResolve(type, aName, quietResolveoptions))
				return FreshTinyIOCBuiltIn.Current.Resolve(type, aName, quietResolveoptions) as ResolveType;
			else
				return null;
		}

		public static IHenryBasePageModel ResolvePageModel(string aName)
		{
			aName = FullPageModelName(aName);
			var modelType = QuietResolve<Type>(aName);
			if (modelType == null)
				return null;
			return Activator.CreateInstance(modelType) as IHenryBasePageModel;
		}

		static Page GetCurrentPage()
		{
			var container = App.Current.MainPage;
			return container?.Navigation?.NavigationStack?.Last() as Page;
		}

		static FreshBasePageModel GetCurrentPageModel()
		{
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
			var parts = navStack.Select((n) =>
			{
				var m = n.GetModel() as IHenryBasePageModel;
				return m.Name;
			}).ToList();
			parts.Insert(0, BriefContainerName(currentContainer.NavigationServiceName));
			return "/" + String.Join("/", parts);
		}

		public static IFreshNavigationService CurrentContainer
		{
			get
			{
				return App.Current.MainPage as IFreshNavigationService;
			}
		}

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
			Page page = ResolveSegmentPageModel(s);
			var result = await s.Model.NavigateIn();
			if (result)
			{
				Debug.WriteLine("Pushing " + s.Name);
				await aContainer.PushPage(page, s.Model as FreshBasePageModel, modal: false, animate: false);
				Debug.WriteLine("Pushed " + s.Name);
			}
			return result;
		}

		static Page ResolveSegmentPageModel(PagePathSegment s)
		{
			if (s.Model == null)
				s.Model = ResolvePageModel(s.Name);
			s.Model.Name = s.Name;
			s.Model.Data = s.Data;
			FreshBasePageModel concreteModel = s.Model as FreshBasePageModel;
			if (concreteModel == null)
				throw new Exception("Model was not a FreshBasePageModel");
			return FreshPageModelResolver.ResolvePageModel(s.Data, concreteModel);
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
				//await Task.Delay(100);
			}
			return true;
		}

		public static IEnumerable<PagePathSegment> SegmentsFromUrlParts(IEnumerable<string> aUrl)
		{
			return aUrl.Select<String, PagePathSegment>(p => new PagePathSegment() { Name = p, Data = null });
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
			if (aUrl == null || aUrl == "" || aUrl == "/")
				return new string[] { };
			aUrl = aUrl.TrimStart(new char[] { '/' });
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
			foreach (var e in existing)
			{
				var n = i < news.Length ? news[i] : null;
				if (n == null)
					result.Add(e);
				else {
					if (n.Name != e.Name)
						divergent = true;
					if (divergent)
						result.Add(e);
				}
				i += 1;
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

					IFreshNavigationService destContainer = null;
					IEnumerable<PagePathSegment> segmentsToPop = null;
					IEnumerable<PagePathSegment> segmentsToPush = null;
					IFreshNavigationService newContainer = null;
					Type containerType = null;

					bool firstNavigation = currentUrl == null;
					bool sameContainer = !firstNavigation && urlParts[0] == currentUrlParts[0];

					destContainer = sameContainer ? CurrentContainer : QuietResolve<IFreshNavigationService>(FullContainerName(urlParts[0]));

					// FreshMVVM Code
					//var page = FreshPageModelResolver.ResolvePageModel<T>(data);
					//var navigationName = Guid.NewGuid().ToString();
					//var naviationContainer = new FreshNavigationContainer(page, navigationName);
					////await PushNewNavigationServiceModal(naviationContainer, page.GetModel(), animate);
					//
					//public Task PushNewNavigationServiceModal(IFreshNavigationService newNavigationService, FreshBasePageModel basePageModels, bool animate = true)
					//{
					//	return PushNewNavigationServiceModal(newNavigationService, new FreshBasePageModel[] { basePageModels }, animate);
					//}
					//
					//public async Task PushNewNavigationServiceModal(IFreshNavigationService newNavigationService, FreshBasePageModel[] basePageModels, bool animate = true)
					//{
					//	var navPage = newNavigationService as Page;
					//	if (navPage == null)
					//		throw new Exception("Navigation service is not Page");
					//
					//	foreach (var pageModel in basePageModels)
					//	{
					//		pageModel.CurrentNavigationServiceName = newNavigationService.NavigationServiceName;
					//		pageModel.PreviousNavigationServiceName = _currentPageModel.CurrentNavigationServiceName;
					//		pageModel.IsModalFirstChild = true;
					//	}
					//
					//	IFreshNavigationService rootNavigation = FreshIOC.Container.Resolve<IFreshNavigationService>(_currentPageModel.CurrentNavigationServiceName);
					//	await rootNavigation.PushPage(navPage, null, true, animate);
					//}
					//return navigationName;

					if (firstNavigation)
					{
						segmentsToPush = SegmentsFromUrlParts(urlParts.ToList().GetRange(1, urlParts.Length - 1));
					}
					else {

						if (aDestructive && !sameContainer)
						{
							if (CurrentContainer != null)
							{
								await CurrentContainer.PopToRoot(animate: false);
							}
						}

						if (destContainer == null)
						{
							segmentsToPush = SegmentsFromUrlParts(urlParts.ToList().GetRange(1, urlParts.Length - 1));
						}
						else {
							var containerUrl = UrlOfContainer(destContainer);
							var containerUrlSegments = SegmentsFromUrl(containerUrl);
							segmentsToPop = GetSegmentsToPop(containerUrlSegments, urlSegments);
							segmentsToPush = GetSegmentsToPush(containerUrlSegments, urlSegments);
						}
					}

					if (destContainer == null)	// create destContainer with first page
					{
						if (segmentsToPush == null || !segmentsToPush.Any())
							throw new Exception("Cannot navigate to a container without a root page");
						var rootPageSegment = segmentsToPush.First();
						segmentsToPush = segmentsToPush.Count()==1 ? null : segmentsToPush.ToList().GetRange(1, segmentsToPush.Count() - 1);

						var page = ResolveSegmentPageModel(rootPageSegment);
						var proceed = await rootPageSegment.Model.NavigateIn();
						if (!proceed)
						{
							// do what?
						}
						destContainer = CreateContainer(urlParts[0], page);
						//var navigationName = Guid.NewGuid().ToString();
						//var naviationContainer = new FreshNavigationContainer(page, navigationName);

						//	var navPage = newNavigationService as Page;
						//	if (navPage == null)
						//		throw new Exception("Navigation service is not Page");
						//
						//	foreach (var pageModel in basePageModels)
						//	{
						//		pageModel.CurrentNavigationServiceName = newNavigationService.NavigationServiceName;
						//		pageModel.PreviousNavigationServiceName = _currentPageModel.CurrentNavigationServiceName;
						//		pageModel.IsModalFirstChild = true;
						//	}
						//
						//	IFreshNavigationService rootNavigation = FreshIOC.Container.Resolve<IFreshNavigationService>(_currentPageModel.CurrentNavigationServiceName);
						//	await rootNavigation.PushPage(navPage, null, true, animate);
					}
					if (destContainer == null)
						throw new Exception("Failed to get destination container");

					SelectContainer(destContainer);						
					if (segmentsToPop != null)
						foreach (var s in segmentsToPop)
						{
							await destContainer.PopPage(modal: false, animate: false);
						}
					if (segmentsToPush != null)
						result = await PushSegments(destContainer, segmentsToPush);
					else
						result = true;
				}
			}
			return result;
		}

		static IFreshNavigationService CreateContainer(string aName, Page aPage)
		{
			IFreshNavigationService result = null;
			aName = FullContainerName(aName);
			var containerType = QuietResolve<Type>(aName + "Type");
			if (containerType == typeof(FreshNavigationContainer)) {
				return new FreshNavigationContainer(aPage,aName);
			}
			else {
				throw new Exception("Container type not found or not yet supported");
			}
		}
	}	
}
