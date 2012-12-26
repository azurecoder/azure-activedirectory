using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.ActiveDirectory;

namespace Elastacloud.WindowsAzure.ActiveDirectory
{
    /// <summary>
    /// Used to get all user related activities via the Activity Directory graph
    /// </summary>
    public class ActiveDirectoryUser
    {
        /// <summary>
        /// The request used to get the user activities
        /// </summary>
        private readonly ActiveDirectoryRequest _request;

        /// <summary>
        /// The Active Directory user that owns this class
        /// </summary>
        private readonly User _user;

        /// <summary>
        /// The groups that the user belongs to 
        /// </summary>
        private List<string> _groupMemberships; 

        /// <summary>
        /// Used to construct an AD user tasks object
        /// </summary>
        public ActiveDirectoryUser(string upn)
        {
            _request = new ActiveDirectoryRequest(ActiveDirectoryProperties.GetActiveDirectoryProperties());
            // TODO: put a check in here to make sure that config == the upn after @
            _user = GetByUpn(upn);
        }

        /// <summary>
        /// Gets all users from the AD tenant
        /// </summary>
        /// <returns>A list of users in the tenant</returns>
        public List<User> GetAllUsers()
        {
            QueryOperationResponse<User> userQuery = null;

            _request.InvokeOperationWithRetry(() =>
                {
                    userQuery = _request.Service.Users.Execute() as QueryOperationResponse<User>;
                });

            _request.ProcessResponseHeader(userQuery);
            return userQuery.ToList();
        }

        /// <summary>
        /// Gets a user with a particular upn (email format)
        /// </summary>
        /// <param name="upn">e.g. richard@elastapop.onmicrosoft.com</param>
        /// <returns>A User object</returns>
        private User GetByUpn(string upn)
        {
            QueryOperationResponse<User> userQuery = null;

            _request.InvokeOperationWithRetry(() =>
                {
                    userQuery = _request.Service.Users.AddQueryOption("$filter", "UserPrincipalName eq '" + upn + "'").Execute()
                        as QueryOperationResponse<User>;
                });

            _request.ProcessResponseHeader(userQuery);
            return userQuery.FirstOrDefault();
        }

        /// <summary>
        /// Used to get the firstname of the of the user
        /// </summary>
        public string Firstname
        {
            get { return _user.GivenName; }
        }

        /// <summary>
        /// Used to get the surname of the user
        /// </summary>
        public string Surname
        {
            get { return _user.Surname; }
        }

        /// <summary>
        /// Used to get the display name of the user
        /// </summary>
        public string DisplayName
        {
            get { return _user.DisplayName; }
        }

        /// <summary>
        /// The job title of the user in the AD
        /// </summary>
        public string JobTitle { get { return _user.JobTitle ?? "Unknown"; } }
        /// <summary>
        /// The preferred language if specified defaults to English
        /// </summary>
        public string Language { get { return _user.PreferredLanguage ?? "English"; } }
        /// <summary>
        /// the mobile number of the user
        /// </summary>
        public string MobileNumber { get { return _user.Mobile ?? "Unknown"; } }
        /// <summary>
        /// The office fax number if specified
        /// </summary>
        public string FaxNumber { get { return _user.FacsimileTelephoneNumber ?? "Unknown"; } }
        /// <summary>
        /// the office phone number if specified
        /// </summary>
        public string PhoneNumber { get { return _user.TelephoneNumber ?? "Unknown"; } }
        /// <summary>
        /// the department that the user works in if specified
        /// </summary>
        public string Department { get { return _user.Department ?? "Unknown"; } }
        /// <summary>
        /// The address of the user if specified
        /// </summary>
        public string Address { get { return _user.StreetAddress ?? "Unknown"; } }
        /// <summary>
        /// The address of the city in the office details if specified
        /// </summary>
        public string City { get { return _user.City ?? "Unknown"; } }
        /// <summary>
        /// The user's country if specified
        /// </summary>
        public string Country { get { return _user.Country ?? "Unknown"; } }

        /// <summary>
        /// The thumbnail address of the image if it exists
        /// </summary>
        public Bitmap Thumbnail
        {
            get
            {
                var byBitmap = _request.GetStreamData(_user, "ThumbnailPhoto");
                if (byBitmap == null)
                    return null;
                var stream = new MemoryStream(byBitmap);
                return new Bitmap(stream);
            }
        }

        /// <summary>
        /// Gets the groups that the User belongs to 
        /// </summary>
        public List<string> Groups
        {
            get
            {
                // cache the group memberships ... most likely the lifetime of this class will be short in any case
                if (_groupMemberships != null)
                    return _groupMemberships;
                _groupMemberships = new List<string>();
                var query = _request.RawQuery("Users", "MemberOf", _user.ObjectId.ToString());
                var groups = query.Where(a => a.ObjectType == "Group");
                var referencedObjects = groups as IList<ReferencedObject> ?? groups.ToList();
                if (!referencedObjects.Any())
                    return _groupMemberships;
                _groupMemberships.AddRange(referencedObjects.Select(@group => @group.DisplayName));
                return _groupMemberships;
            }
        }

        /// <summary>
        /// Gets whether the user is a member of a particular group
        /// </summary>
        /// <param name="groupName">the name of the group</param>
        /// <returns>a boolean value showing whether the user is a member or not</returns>
        public bool IsInGroup(string groupName)
        {
            return Groups.Contains(groupName);
        }
    }
}
