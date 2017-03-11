#CK-Core#
This solution contains basic projects: adapters, useful tools such as the ActivityMonitor.

##Content##
###CK.Core###
Contains several helper classes and interfaces.

CKTrait handle the combination of different tags (strings) in a determinist manner. 
Traits are normalized and ordered strings combinations (*"Sql|DB access|Subscription" == "DB access|Sql|Subscription"* and *"DB access|Sql" > "Sql"*): a total order exists on the set of traits combinations based on lexicographical order for atomic trait and the number of traits in a composite. They support union, intersect, except and symmetric except in O(n).

###CK.ActivityMonitor###
Contains the ActivityMonitor base implementation.

##Bug Tracker##
If you find any bug, don't hesitate to report it on : [https://github.com/Invenietis/ck-core/issues/](https://github.com/Invenietis/ck-core/issues/)

##Copyright and license##

This solution is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU Lesser General Public License for more details.
<http://www.gnu.org/licenses/>. 
 
Copyright � 2007-20117,
    Invenietis <http://www.invenietis.com>,
All rights reserved.
