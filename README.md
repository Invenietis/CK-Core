#CK-Core#
This solution contains basic helpers and useful tools.

There is currently only one project: CK.Core.

CKTrait handle the combination of different tags (strings) in a determinist manner. 
Traits are normalized and ordered strings combinations (*"Sql|DB access|Subscription" == "DB access|Sql|Subscription"* and *"DB access|Sql" > "Sql"*): a total order exists on the set of traits combinations based on lexicographical order for atomic trait and the number of traits in a composite. They support union, intersect, except and symmetric except in O(n).

##Bug Tracker##
If you find any bug, don't hesitate to report it on : [https://github.com/Invenietis/CK-Core/issues/](https://github.com/Invenietis/CK-Core/issues/)

##Copyright and license##

This solution is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU Lesser General Public License for more details.
<http://www.gnu.org/licenses/>. 
 
Copyright � 2007-2017,
    Invenietis <http://www.invenietis.com>,
All rights reserved.
