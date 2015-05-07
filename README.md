
Topcat - the JNCC Data Resource Catalogue
====================================

A simple data resource catalogue supporting a sensible profile of UK Gemini.

Licensed under [Open Government Licence v2](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/2/).

Development
-----------

###Web Essentials
Install the [Visual Studio Web Essentials](http://vswebessentials.com/) extensions pack for design-time support for CoffeeScript (.coffee) and LESS (.less).
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
RavenDB studio can be accessed in development at http://localhost:8888

To upgrade RavenDB, after updating the NuGet packages you currently need to update the Raven.Studio.Html5.zip file which can be got from the downloadable distribution.
Hopefully this will be embedded in a forthcoming version, making this extra step unnecessary.

Deployment
----------

Topcat runs with no special setup in Visual Studio for local development.

Here's what you need to do to create a production instance:

###Windows Authentication
This is an corporate / intranet application and user account details and authentication rely on
Active Directory and Windows authentication which gives us a great user experience in Chrome and IE.

You need to disable Anonymous Authentication and enable Windows Authentication in the IIS website hosting Topcat.

###RavenDB
You can deploy Raven in various ways; the recommended is using the Windows installer:

* Download the correct version from https://ravendb.net/download (see packages folder for the version number currently in use) 
* Supply the license file when requested
* Set up as an IIS-hosted application
** Port: 8090
** Path: C:\topcat\RavenDB
* Browse to http://localhost:8090/ to check the installation succeeded
* In the Create a New Database dialogue, create e.g. Catalogue.Data.Beta with Versioning Bundle

When deploying a new database instance, the **Versioning bundle** needs to be enabled.

**Important** The Catalogue.Data.dll must be dpeloyed into Raven/Analyzers folder because RavenDB needs to be able to load the custom Lucene analyzer we use.  

