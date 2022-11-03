package crc64a3293165ad3fd123;


public class WifiConnector_NetworkCallback
	extends android.net.ConnectivityManager.NetworkCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAvailable:(Landroid/net/Network;)V:GetOnAvailable_Landroid_net_Network_Handler\n" +
			"n_onUnavailable:()V:GetOnUnavailableHandler\n" +
			"";
		mono.android.Runtime.register ("Bajaj.Droid.Dependencies.WifiConnector+NetworkCallback, SML.Android", WifiConnector_NetworkCallback.class, __md_methods);
	}


	public WifiConnector_NetworkCallback ()
	{
		super ();
		if (getClass () == WifiConnector_NetworkCallback.class)
			mono.android.TypeManager.Activate ("Bajaj.Droid.Dependencies.WifiConnector+NetworkCallback, SML.Android", "", this, new java.lang.Object[] {  });
	}

	public WifiConnector_NetworkCallback (android.net.ConnectivityManager p0)
	{
		super ();
		if (getClass () == WifiConnector_NetworkCallback.class)
			mono.android.TypeManager.Activate ("Bajaj.Droid.Dependencies.WifiConnector+NetworkCallback, SML.Android", "Android.Net.ConnectivityManager, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public void onAvailable (android.net.Network p0)
	{
		n_onAvailable (p0);
	}

	private native void n_onAvailable (android.net.Network p0);


	public void onUnavailable ()
	{
		n_onUnavailable ();
	}

	private native void n_onUnavailable ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
