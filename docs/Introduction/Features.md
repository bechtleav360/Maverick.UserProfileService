# Features

* Manages user data including their relations
* Used entities are hierarchical structured and therefore are stored in a graph.  
  like:
    * _Users_ are stored in groups
    * _Organizations_ can contain other _organizations_
    * _Users_ or _groups_ can be assigned to _Functions_ or _Roles_
    * _Functions_ limit access of their containing _role_ by an additional _organization_
    * All kind of _Assignments_ can be triggered in the future for an user
    * _Volatile Data_ can be stored in Postgres and can be used as an container without persisted in the eventstore
* Can store the incoming requests in various ways due to its "data projections"
* Contains entity models for roles and functions that can be used as source of a [RBAC](https://en.wikipedia.org/wiki/Role-based_access_control) system or [OPA](https://www.openpolicyagent.org/)