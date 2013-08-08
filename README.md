JNCC Catalogue
==============

Web Essentials
--------------

Install the Visual Studio extensions pack for design-time support for CoffeeScript (.coffee) and LESS (.less).
These are essential if you want to edit the application's Javascript or CSS.
There are no build-time or run-time steps used to compile Coffeescript and LESS - the Visual Studio extensions are simpler.
The application will expect the correct .js and .css files to have be design-time generated.
http://visualstudiogallery.msdn.microsoft.com/07d54d12-7133-4e15-becb-6f451ea3bea6
*This may be in Visual Studio Update 2 now!*

AngularJS
---------
There is Resharper support http://blogs.jetbrains.com/dotnet/2013/02/angularjs-support-for-resharper/

Setting up RavenDB
------------------

You can deploy Raven in various ways; the recommended is using IIS.
http://ravendb.net/docs/server/deployment/as-iis-application

* Download, unblock and unzip, and copy Web folder to e.g. e:\catalogue-deployments\Live\RavenDB\Web folder
* <add key="Raven/AnonymousAccess" value="All"/>
* iis: Catalogue.Data.Live -> Web folder on port e.g. 8080
* ensure app pool on .net 4
* disable overlapped recycle
* enable write to ravendb folder:
** ICACLS e:\catalogue-deployments\Live\RavenDB\Web /grant "IIS AppPool\Catalogue.Data.Live":F
** C:\Windows\System32\inetsrv\config\applicationHost.config ensure startMode="AlwaysRunning":
** <add name="Catalogue.Data.Live" managedRuntimeVersion="v4.0" startMode="AlwaysRunning" />



