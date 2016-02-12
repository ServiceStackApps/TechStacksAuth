using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Auth;
using ServiceStack;
using TechStacks.ServiceModel;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using ServiceStack.Text;

namespace TechStacks.Auth
{
	[Activity (Label = "TechStacks.Auth", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			JsonServiceClient client = new JsonServiceClient ("http://techstacks.io/");

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				var auth = new ServiceStackAuthenticator("http://techstacks.io","twitter",(jsonServiceClient) => {
					//Custom get user details function, usually your user profile/session info exposed via a ServiceStack Service.
					//The provided jsonServiceClient is authenticated, so can call auth services to get user details directly.
					//'Account' also has a Dictionary<string,string> for user details values if required, these are then available in 'Completed'.
					var userSessionInfo = jsonServiceClient.Get<UserSessionInfo>("/my-session");
					return new Account(userSessionInfo.UserName,jsonServiceClient.CookieContainer);
				});
				StartActivity(auth.GetUI(this));
				auth.Completed += (sender, eventArgs) => {
					if(eventArgs.IsAuthenticated) {
						GetUserFeedResponse response = null;
						try {
							//Grab auth cookies for app JsonServiceClient
							client.CookieContainer = eventArgs.Account.Cookies;
							response = client.Get(new GetUserFeed());

							var intent = new Intent(this, typeof(UserFeed));
							intent.PutStringArrayListExtra("techstackfeed", response.Results.Select(x => x.Name).ToList());
							StartActivity(intent);
						} catch(Exception) {
							//Failed to get user feed.
							//Show error message.
						}
					}
				};
			};
		}
	}

	public class ServiceStackAuthenticator : WebAuthenticator
	{
		readonly Uri authUrl;
		private readonly string serviceStackBaseUrl;
		readonly Func<ServiceClientBase, Account> getCustomUserDetails;
		ServiceClientBase jsonServiceClient;

		public Func<string, ServiceClientBase> ServiceClientFactory { get; set; }
		public Func<Uri,bool> OnSuccessPredicate { get; set; }
		Account account;

		public ServiceStackAuthenticator(string serviceStackBaseUrl, 
			string provider,
			Func<ServiceClientBase, Account> getUserDetails)
		{
			authUrl = new Uri(serviceStackBaseUrl + "/auth/" + provider);
			if (getUserDetails == null)
			{
				throw new ArgumentNullException("getUserDetails");
			}
			this.serviceStackBaseUrl = serviceStackBaseUrl;
			getCustomUserDetails = getUserDetails;
		}

		public override Task<Uri> GetInitialUrlAsync()
		{
			return Task.Run(() => authUrl);
		}

		public override void OnPageLoading(Uri url)
		{
			bool uriTestResult = OnSuccessPredicate != null
				? OnSuccessPredicate(url)
				: url.Fragment.Contains("s=1") || url.Query.Contains("s=1");

			jsonServiceClient = ServiceClientFactory == null ? new JsonServiceClient(serviceStackBaseUrl) : ServiceClientFactory(serviceStackBaseUrl);

			if (authUrl.Host == url.Host && uriTestResult)
			{
				var cookie = Android.Webkit.CookieManager.Instance.GetCookie(url.AbsoluteUri);
				jsonServiceClient.CookieContainer = jsonServiceClient.CookieContainer ?? new CookieContainer();
				jsonServiceClient.CookieContainer.SetCookies(new Uri(url.AbsoluteUri), cookie);

				account = getCustomUserDetails(jsonServiceClient);
				OnSucceeded(account);
			}
			base.OnPageLoading(url);
		}

		public override void OnPageLoaded(Uri url)
		{
			//Required override, but don't need to wait for it to finish loading, already have auth cookies.
			//See 'OnPageLoading'.
		}
	}

	public class UserSessionInfo
	{
		public string Address { get; set; }
		public string Address2 { get; set; }
		public DateTime? BirthDate { get; set; }
		public string BirthDateRaw { get; set; }
		public string City { get; set; }
		public string Company { get; set; }
		public string Country { get; set; }
		public DateTime CreatedAt { get; set; }
		public string Culture { get; set; }
		public string DisplayName { get; set; }
		public string Email { get; set; }
		public string FacebookUserId { get; set; }
		public string FacebookUserName { get; set; }
		public string FirstName { get; set; }
		public string FullName { get; set; }
		public string Gender { get; set; }
		public string Id { get; set; }
		public virtual bool IsAuthenticated { get; set; }
		public string Language { get; set; }
		public DateTime LastModified { get; set; }
		public string LastName { get; set; }
		public string MailAddress { get; set; }
		public string Nickname { get; set; }
		public List<string> Permissions { get; set; }
		public string PhoneNumber { get; set; }
		public string PostalCode { get; set; }
		public string PrimaryEmail { get; set; }
		public string ReferrerUrl { get; set; }
		public string RequestTokenSecret { get; set; }
		public List<string> Roles { get; set; }
		public virtual string Sequence { get; set; }
		public string State { get; set; }
		public long Tag { get; set; }
		public string TimeZone { get; set; }
		public string TwitterScreenName { get; set; }
		public string TwitterUserId { get; set; }
		public string UserAuthId { get; set; }
		public string UserAuthName { get; set; }
		public string UserName { get; set; }
	}
}