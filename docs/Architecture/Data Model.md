# Data-Models
The following core entities can currently be created or managed by the service:

1. **Users**: User information related to an individual is stored, including their first name, last name, and other personal data.
1. **Groups**: Information about a group is stored (name, members, etc.). Multiple users can be grouped together in a group to, for example, assign specific rights to a number of users.
1. **Organizations**: Organizational data is stored.
1. **Roles**: Information about roles is stored. Roles can be used to assign specific rights to users, allowing access to files/operations.
1. **Functions**: A function consists of a tuple, namely a role and an organizational unit. It determines which rights are assigned to a user or a group. For example, with function Z23, an Administrator can read and modify all files/operations assigned to organization Z23.