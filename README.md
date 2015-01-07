
Topcat - the JNCC Datasets Catalogue
====================================

A simple data resource catalogue supporting a sensible profile of UK Gemini.

Licensed under [Open Government Licence v2](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/2/).



Development
-----------

###Web Essentials
Install the [Visual Studio Web Essentials] (http://vswebessentials.com/) extensions pack for design-time support for CoffeeScript (.coffee) and LESS (.less).
These are essential if you want to edit the application's Javascript or CSS.
There are no build-time or run-time steps used to compile Coffeescript and LESS - the Visual Studio extensions are simpler.
The application will expect the correct .js and .css files to have been design-time generated.

###AngularJS
There is (allegedly) [Resharper support](http://blogs.jetbrains.com/dotnet/2013/02/angularjs-support-for-resharper/).

###Resharper
Currently best to disable Resharper > Options > Tools > Unit Testing > Javascript Tests > 
* Enable QUnit support
* Enable Jasmine support

###RavenDB
RavenDB studio can be accessed in the development environment by browsing too: http://localhost:8888/raven/studio.html

Deployment
----------

Topcat should run with no special setup on a vanilla Visual Studio installation for local development. Here's what you need to do to create a production instance.

###Windows Authentication
This is an corporate / intranet application and user account details and authentication rely on
Active Directory and Windows authentication which gives us a great user experience in Chrome and IE.

You need to disable Anonymous Authentication and enable Windows Authentication in the IIS website hosting Topcat.

###RavenDB
You can deploy Raven in various ways; the recommended is using the Windows installer:

* Download the correct version from https://ravendb.net/download (see packages folder for the version number currently in use) 
* Supply the license file when requested
* Set up as an IIS-hosted application
** Web site name: Topcat.Raven
** Port: 8090
** Path: C:\topcat\RavenDB
* Browse to http://localhost:8090/ to check the installation succeeded
* In the Create a New Database dialogue, create e.g. Catalogue.Data.Beta with Versioning Bundle

The Windows installer should replace most of the following manual steps - but [here they are just in case] (http://ravendb.net/docs/server/deployment/as-iis-application])

* Download, unblock and unzip, and copy Web folder to e.g. e:\catalogue-deployments\Live\RavenDB\Web folder
* `<add key="Raven/AnonymousAccess" value="All"/>`
* iis: Catalogue.Data.Live -> Web folder on port e.g. 8080
* ensure app pool on .net 4
* disable overlapped recycle
* enable write to ravendb folder:
* `ICACLS e:\catalogue-deployments\Live\RavenDB\Web /grant "IIS AppPool\Catalogue.Data.Live":F`
* C:\Windows\System32\inetsrv\config\applicationHost.config ensure startMode="AlwaysRunning". `<add name="Catalogue.Data.Live" managedRuntimeVersion="v4.0" startMode="AlwaysRunning" />`

When creating a new database instance, the **Versioning bundle** needs to be enabled.

The Catalogue.Data.dll must be copied into Raven/Analyzers folder because RavenDB needs to be able to load the custom Lucene analyzer we use.  
