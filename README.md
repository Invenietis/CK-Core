#CK-Core#
This solution contains basic projects: adapters, useful tools such as the ActivityMonitor.

##Content##
###CK.Core###
Contains several helper classes and interfaces.

CKTrait handle the combination of different tags (strings) in a determinist manner. 
Traits are normalized and ordered strings combinations (*"Sql|DB access|Subscription" == "DB access|Sql|Subscription"* and *"DB access|Sql" > "Sql"*): a total order exists on the set of traits combinations based on lexicographical order for atomic trait and the number of traits in a composite. They support union, intersect, except and symmetric except in O(n).
AppSettings handles simple global settings in a portable way.

###CK.ActivityMonitor###
Contains the ActivityMonitor base implementation.

###CK.Monitoring###
Provides monitoring tools such as the *GrandOutput*.
Simple default configuration:
```
CK.Core.SystemActivityMonitor.RootLogPath = @"C:\Test\Logs";
CK.Monitoring.GrandOutput.EnsureActiveDefaultWithDefaultSettings();
```
Any *ActivityMonitor* created after this code snipped will be configured to output to the GrandOutput.

To avoid hardcoding the log path, simply use your standard application configuration file with the following application settings key:
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="CK.Core.SystemActivityMonitor.RootLogPath" value="Misc\Logs" />
  </appSettings>
</configuration>
```
###CK.Interop###
Contains LowLevel helpers, for example a DLLImportAttribute that handles defining different dlls regarding the computer's architecture (32 or 64 bit).

###CK.Reflection###
Contains Reflection helper classes.

##Bug Tracker##
If you find any bug, don't hesitate to report it on : [https://github.com/Invenietis/ck-core/issues/](https://github.com/Invenietis/ck-core/issues/)

##Copyright and license##

This solution is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU Lesser General Public License for more details.
<http://www.gnu.org/licenses/>. 
 
Copyright � 2007-2015,
    Invenietis <http://www.invenietis.com>,
All rights reserved.
