PrismAdapter
============

Statical Prism has a construct called a Mirror which creates a local source code repository and uses an adapter to synchronize with a data source. To date Statical Prism has only one adapter which just uses finsql.exe from Dynamics NAV 2013 (or higher) to export source code objects to the local mirror location. This allows Statical Prism users to seamlessly integrate with existing Dynamics NAV environments and inspect code and be alerted when objects are updated.

Note: Visit http://stati-cal.com/ to learn more about mirrors and Statical Prism.

The PrismAdapter repository contains an interface for the adapter that mirrors use to access Dynamics NAV source code objects. This 1.0.0 release is our best effort for an adapter interface which is now frozen with respect to backwards compatibility. You are of course welcome to contribute fixes and improvements if you'd like

Please note that we still have not made a dynamic loader for PrismAdapters so at this point new adapters must be sent to us for inclusion in Statical Prism. However we do expect to have one at some point in the future - depending on public interest.
