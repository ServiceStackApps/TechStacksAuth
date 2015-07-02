
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TechStacks.Auth
{
	[Activity (Label = "UserFeed")]			
	public class UserFeed : ListActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			var techStacks = Intent.Extras.GetStringArrayList ("techstackfeed") ?? new List<string> ();
			this.ListAdapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1, techStacks);
			// Create your application here
		}
	}
}

