PrismAdapter
============

Statical Prism has a construct called a Mirror which creates a local source code repository and uses an adapter to synchronize with a data source. To date Statical Prism has only one adapter which just uses finsql.exe from Dynamics NAV 2013 (or higher) to export source code objects to the local mirror location.

Note: Visit http://stati-cal.com/ to learn more about mirrors and Statical Prism.

The PrismAdapter repository contains an interface for the adapter that mirrors use to access Dynamics NAV source code objects. This initial release contains a bare-bones non-code-reviewed adapter interface which will need a little more work before we freeze the interface for public contribution. However already, contributors can begin prototyping because it is likely that the basics will not change that much. We do plan for better error handling support, capability exposure as well as asynchronous methods for "version 1".

There is no specific plug-in mechanism. We simply plan to allow users and developers to drop in their adapter implementations into a Statical Prism installation extension folder and then Prism will discover these. This work has yet to start but should not take long to complete.
