﻿using Xamarin.Forms;
using FreshMvvm;

namespace HenrySample
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			Henry.RegisterContainer<FreshNavigationContainer>("Login");
			Henry.RegisterContainer<FreshNavigationContainer>("Main");

			Henry.RegisterPageModel<FirstPageModel>();
			Henry.RegisterPageModel<SecondPageModel>();
			Henry.RegisterPageModel<ThirdPageModel>();

			Henry.Goto("/Login/First");
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
