package crc64a3293165ad3fd123;


public class TimeTrackingServiceBindder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Bajaj.Droid.Dependencies.TimeTrackingServiceBindder, SML.Android", TimeTrackingServiceBindder.class, __md_methods);
	}


	public TimeTrackingServiceBindder ()
	{
		super ();
		if (getClass () == TimeTrackingServiceBindder.class)
			mono.android.TypeManager.Activate ("Bajaj.Droid.Dependencies.TimeTrackingServiceBindder, SML.Android", "", this, new java.lang.Object[] {  });
	}


	public TimeTrackingServiceBindder (java.lang.String p0)
	{
		super (p0);
		if (getClass () == TimeTrackingServiceBindder.class)
			mono.android.TypeManager.Activate ("Bajaj.Droid.Dependencies.TimeTrackingServiceBindder, SML.Android", "System.String, mscorlib", this, new java.lang.Object[] { p0 });
	}

	public TimeTrackingServiceBindder (crc64a3293165ad3fd123.TimeTrackingServiceConnection p0)
	{
		super ();
		if (getClass () == TimeTrackingServiceBindder.class)
			mono.android.TypeManager.Activate ("Bajaj.Droid.Dependencies.TimeTrackingServiceBindder, SML.Android", "Bajaj.Droid.Dependencies.TimeTrackingServiceConnection, SML.Android", this, new java.lang.Object[] { p0 });
	}

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
