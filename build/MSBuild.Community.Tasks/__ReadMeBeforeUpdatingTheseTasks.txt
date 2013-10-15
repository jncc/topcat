This folder just contains the contents of the MSBuild Community Tasks bin folder that gets installed with the installer (plus this file).

If you need to upgrade the tasks, you should just be able to copy them over the files in here.

The tasks should not need to be "installed" on a build machine. The master build script sets a property to make the MSBuild.Community.Tasks.targets file look in this folder, and not in Program Files or Windows or wherever it installs them. 