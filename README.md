#CK-Core#
This solution contains several projects, this is part of the [CiviKey](https://github.com/Invenietis/ck-certified) project toolbox.

##Content##
###CK.Core###
Contains several helper classes and interfaces.
The most useful and interesting aspect is the ReadOnly and Writable List & Collections (and other Sorted / Observable implementations) framework that have been designed 
from the ground up to support co and contra variance.

###CK.Interop###
Contains LowLevel helpers, for example a DLLImportAttribute that handles defining different dlls regarding the computer's architecture (32 or 64 bit).

###CK.MultiTrait###
Contains classes that can be used to handle the combination of different tags. 
For example, making sure that Alt+Ctrl+Home == Ctrl+Alt+Home == Alt+Home+Ctrl. 
It is also used by the CiviKey project to handle the fact that Ctrl+Alt+Suppr & Alt+Ctrl+Suppr both trigger the Ctrl+Alt+Suppr action.

###CK.Reflection###
Contains Reflection helper classes.

###CK.Storage###
Contains helper classes & interfaces for structured objects serialization. Embeds an implementation for Xml.

##Bug Tracker##
If you find any bug, don't hesitate to report it on : [http://civikey.invenietis.com/](http://civikey.invenietis.com/)

##Copyright and license##

This solution is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU Lesser General Public License for more details. 
You should have received a copy of the GNU Lesser General Public License 
along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
 
Copyright © 2007-2012,
    Invenietis <http://www.invenietis.com>,
    In’Tech INFO <http://www.intechinfo.fr>,
All rights reserved.
