## TechStacksAuth
This project shows client examples of connecting and authenticating with the TechStacks application using various technologies.

### Using Xamarin.Auth with ServiceStack

Xamarin.Auth is a Xamarin component that makes integrating your Xamarin Android or iOS application simple when connecting to OAuth and various other providers. Connecting to your ServiceStack server with existing OAuth support such as Twitter, GitHub and Facebook via Xamarin.Auth is also easy!

Xamarin.Auth is extensible and already provides a good base for handling our authentication with ServiceStack. In this example we create a custom `WebAuthenticator` called `ServiceStackAuthenticator`. This custom authenticator allows us to call our ServiceStack instance directly and reuse existing OAuth integration, eg `TwitterAuthProvider`.

![](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/xamarin-android-auth-demo.gif)

Xamarin's authentication library requires the creation of an `Account` object that represents the authenticated users details, user name, email etc. These details will vary can be constructed from your own ServiceStack service.

Once the authentication process has been completed and the users details collected from your ServiceStack server, simple check `IsAuthenticated` to verify if the process was successful.

``` csharp
button.Click += delegate {
	var auth = 
	    new ServiceStackAuthenticator("http://techstacks.io","twitter",
	    (jsonServiceClient) => {
		//Custom user details service.
		var userSessionInfo = jsonServiceClient.Get<UserSessionInfo>("/my-session");
		return new Account(userSessionInfo.UserName,jsonServiceClient.CookieContainer);
	});
	//Start authentication activity.
	StartActivity(auth.GetUI(this));
	//Wire completed event.
	auth.Completed += (sender, eventArgs) => {
		if(eventArgs.IsAuthenticated) {
			GetUserFeedResponse response = null;
			//Grab auth cookies for app JsonServiceClient
			client.CookieContainer = eventArgs.Account.Cookies;
			//Get user feed for next activity.
			response = client.Get(new GetUserFeed());
			
			var intent = new Intent(this, typeof(UserFeed));
			intent.PutStringArrayListExtra(
			    "techstackfeed", 
			    response.Results.Select(x => x.Name).ToList());
			    
			//Start user feed activity
			StartActivity(intent);
		}
	};
};
```

The above example first constructs the authenticator passing is:

- `baseUrl` Base URL of the ServiceStack server, eg http://techstacks.io
- `provider` The authentication provider you want to use, in this case, `twitter`
- `getUserDetails` Func that given a `ServiceClientBase` returns a valid `Account` object.

> For more control, two optional functions can be provided.
>  - `successUriPredicate` A predicate to overriding the successful URI check
>  - `serviceClientFactory` for providing your own instance of a `ServiceClientBase`. Authentication cookies will be populated on successful authentication.

Once the `Completed` event has been fired, authentication cookies can be accessed via the `eventArgs.Account.Cookies` property. These can then be passed to your own instance of the `ServiceClientBase` (eg `JsonServiceClient`) to make subsequent authenticated calls.
