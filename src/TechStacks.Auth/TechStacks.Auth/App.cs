using System;
using ServiceStack;
using Android.Content;
using System.Collections.Generic;
using Android.App;
using System.Net;
using TechStacks.ServiceModel;
using System.Threading.Tasks;

namespace TechStacks.Auth
{
	public class App
	{
		public static App Instance = new App();

		JsonServiceClient client;

		public App() {
			client = new JsonServiceClient("http://techstacks.io");
		}

		public void UpdateCookies(CookieContainer cookies)
		{
			client.CookieContainer = cookies;
		}

		public GetUserFeedResponse GetUserFeed()
		{
			return client.Get (new GetUserFeed ());
		}
	}
}

