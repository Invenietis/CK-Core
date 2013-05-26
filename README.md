#CK-Core#
This solution contains several projects, this is part of the [CiviKey](https://github.com/Invenietis/ck-certified) project toolbox.

##Content##
###CK.Core###
Contains several helper classes and interfaces.

One of the most useful and interesting aspect is the ReadOnly and Writable List &amp; Collections (and other Sorted / Observable implementations) objects that have been designed from the ground up to support co and contra variance.

As of 2.8.6 version, CK.Core is 4.0 / 4.5 compliant: in .Net 4.5 *System.Collections.Generic.IReadOnlyCollection&lt;T&gt;* and *System.Collections.Generic.IReadOnlyList&lt;T&gt;* appeared in the mscorlib (but without the contravariant Contains and IndexOf that we had in CK.Core). The package now contains the libs and CK.Core related objects have been prefixed with CK (to avoid any other name clash in the future).

Another important component is the ActivityLogger: it offers a different way than traditional  loggers (log4net, NLog, etc.) to capture program's activities and structure them.
ActivityLogger uses CKTrait to handle the combination of different tags in a determinist manner. 
Traits are normalized (*"Sql|DB access|Subscription" == "DB access|Sql|Subscription"* and *"DB access|Sql" > "Sql"*): a total order exists on the set of traits combinations based on lexicographical order for atomic trait and the number of traits in a composite. They support union, intersect, except and symmetric except in O(n).

###CK.Interop###
Contains LowLevel helpers, for example a DLLImportAttribute that handles defining different dlls regarding the computer's architecture (32 or 64 bit).

###CK.Reflection###
Contains Reflection helper classes.

###CK.Storage###
Contains helper classes &amp; interfaces for structured objects serialization. Embeds an implementation for Xml.

##Bug Tracker##
If you find any bug, don't hesitate to report it on : [http://civikey.invenietis.com/](http://civikey.invenietis.com/)

##Copyright and license##

This solution is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU Lesser General Public License for more details. 
You should have received a copy of the GNU Lesser General Public License 
along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
 
Copyright � 2007-2013,
    Invenietis <http://www.invenietis.com>,
    In�Tech INFO <http://www.intechinfo.fr>,
All rights reserved.
