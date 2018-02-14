
Topcat - the JNCC Data Resource Catalogue
====================================

A simple data resource catalogue supporting a sensible profile of UK Gemini.

Licensed under [Open Government Licence v2](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/2/).

Development
-----------

Since the recent upgrade to RavenDB 3.5 you need to **run Visual Studio as an Administrator**, unfortunatley, or the database throws errors at dev time. We're looking into why this is the case.

### Web Essentials
Install the Visual Studio Web Essentials extensions pack (and now the "Web Compiler" extension) for design-time support for CoffeeScript (.coffee) and LESS (.less). These are essential if you want to edit the application's Javascript or CSS.

There are no build-time or run-time steps used to compile Coffeescript and LESS - the Visual Studio extensions are simpler.
The application will expect the correct .js and .css files to have been design-time generated.

### AngularJS
There is (allegedly) [Resharper support](http://blogs.jetbrains.com/dotnet/2013/02/angularjs-support-for-resharper/).

### Resharper
Currently best to disable Resharper > Options > Tools > Unit Testing > Javascript Tests > 
* Enable QUnit support
* Enable Jasmine support

### RavenDB
RavenDB studio can be accessed in development at http://localhost:8888

To upgrade RavenDB, after updating the NuGet packages you currently need to update the Raven.Studio.Html5.zip file which can be got from the downloadable distribution.
Hopefully this will be embedded in a forthcoming version, making this extra step unnecessary.

Deployment
----------

Topcat runs with no special setup in Visual Studio for local development.

Here's what you need to do to create a production instance:

### Build
Run the `build/built.targets` MSBuild file (or use `build.bat`).
* Use the the Developer Command Prompt for Visual Studio
** You may need to fix up (copy) the `Microsoft.WebApplication.targets` into the necessary place in the MSBuild installation.
E.g. I copied the WebApplications folder from `C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Microsoft\VisualStudio\v15.0\WebApplications` to `C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v15.0`
* Make sure Git is available globally (e.g. install GitBash).
* Install NUnit-console 2.6.3 from https://launchpad.net/nunitv2/trunk/2.6.3

### Windows Authentication
This is an corporate / intranet application and user account details and authentication rely on
Active Directory and Windows authentication which gives us a great user experience in Chrome and IE.

You need to disable Anonymous Authentication and enable Windows Authentication in the IIS website hosting Topcat.

### RavenDB
You can deploy Raven in various ways; the recommended is using the Windows installer:

* Download the correct version from https://ravendb.net/download (see packages folder for the version number currently in use) 
* Supply the license file when requested
* Set up as an IIS-hosted application
** Port: 8090
** Path: C:\topcat\RavenDB
* Browse to http://localhost:8090/ to check the installation succeeded
* In the Create a New Database dialogue, create e.g. Catalogue.Data.Beta with Versioning Bundle

When deploying a new database instance, the **Versioning bundle** needs to be enabled.

**Important** The Catalogue.Data.dll must be deployed into Raven/Analyzers folder because RavenDB needs to be able to load the custom Lucene analyzer we use.  

LinqPad
-------
RavenDB doesn't support ad-hoc querying very well. We use LinqPad to do ad-hoc querying and basic data visualisation. 

http://www.linqpad.net/download.aspx

You need to use the RavenDB LinqPad adaptor. Our RavenDB is not up-to-date, so we need to use an older version of the driver. Browse to it in the `/linqpad` directory. Create a connection like `RavenDB: Topcat LIVE`, and add the references to the assemblies in the output folder (such as `Catalogue.Web/bin`) of your build.

    Catalogue.Data.dll
    Catalogue.Gemini.dll

You can then write queries like this:

    from r in Query<Catalogue.Data.Model.Record>()
    where r.Gemini.Title.StartsWith("Sea")
    select r

or
  
    from r in Query<Catalogue.Data.Model.Record>()
    where r.Gemini.Keywords.Any(k => k.Vocab == "http://vocab.jncc.gov.uk/jncc-category" && k.Value == "Human Activities")
    select r
    
