using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LCHARMS.Identity
{
    // this stuff is not in the ID provider because it is not a matter of authentication
    //  LIdentities can be from remote or local servers.  This part of the service already knows if the user is authorized to access
    //  something, so they are free to create/manage groups

    //  should ensure a different authorization call is used for these since they're in system directories
    public class GroupManager
    {
        //these are somewhat misnormers, as groups can contain groups as well.
        // the service that calls this should make sure that the user has write permission to the top group, and read or write to the bottom group
        //group to user ID
        Dictionary<LIdentity, List<LIdentity>> Groups = new Dictionary<LIdentity, List<LIdentity>>();
        //User ID to group
        Dictionary<LIdentity, List<LIdentity>> Users = new Dictionary<LIdentity, List<LIdentity>>();
        public GroupManager()
        {
            //load all groups
        }
        public void LoadGroups()
        {
        }
        public void SaveGroups()
        {
        }

        public void AddUserToGroup(LIdentity GroupID, LIdentity UserID)
        {
            if (!Groups.ContainsKey(GroupID))
            {
                Groups[GroupID] = new List<LIdentity>();
            }
            Groups[GroupID].Add(UserID);

            if (!Users.ContainsKey(UserID))
            {
                Users[UserID] = new List<LIdentity>();
            }
            Users[UserID].Add(GroupID);
        }
        public void RemoveUserFromGroup(LIdentity GroupID, LIdentity UserID)
        {
            if (Groups.ContainsKey(GroupID))
            {
                if (Groups[GroupID].Contains(UserID))
                    Groups[GroupID].Remove(UserID);
            }
            if (Users.ContainsKey(UserID))
            {
                if (Users[UserID].Contains(GroupID))
                    Users[UserID].Remove(GroupID);
            }
        }


    }
}
