using System;
using System.Configuration;

namespace Elastacloud.WindowsAzure.ActiveDirectory
{
    /// <summary>
    /// Contains all of the necessary properties to allow a connection to the Windows Azure Active Directory
    /// </summary>
    public class ActiveDirectoryProperties
    {
        /// <summary>
        /// Whether the environment is production or stagging
        /// </summary>
        public string Environment { get { return "PROD"; } }
        /// <summary>
        /// What is the version of the data contract being used
        /// </summary>
        public string Version { get { return "0.8"; } }
        /// <summary>
        /// the domain name of the tenant
        /// </summary>
        public string DomainName { get; set; }
        /// <summary>
        /// The GUID principal id used by the tenant
        /// </summary>
        public string PrincipalId { get { return "00000002-0000-0000-c000-000000000000"; } }
        /// <summary>
        /// The Active Directory graph domain
        /// </summary>
        public string GraphDomain { get { return "graph.windows.net"; } }
        /// <summary>
        /// The STS url for Active Directory - get this right!!
        /// </summary>
        public string StsUrl { get { return "https://accounts.accesscontrol.windows.net/"; } }
        /// <summary>
        /// The GUID principal for the tenant using the service detail
        /// </summary>
        public string AppPrincipalId { get; set; }
        /// <summary>
        /// The ACS principal for the service 
        /// </summary>
        public string AcsPrincipalId { get { return "00000001-0000-0000-c000-000000000000"; } }
        /// <summary>
        /// The secret needed to access the service
        /// </summary>
        public string SymmetricKey { get; set; }
        /// <summary>
        /// What is returned from the auth with the graph API
        /// </summary>
        public string ReplicaSessionKeyHeader { get; set; }
        /// <summary>
        /// The url used to connect to the service and get graph metadata etc. back
        /// </summary>
        public Uri ConnectionUri { get { return new Uri(string.Format(@"https://{0}/{1}", GraphDomain, DomainName)); } }
        /// <summary>
        /// The address of the tenant in the cloud used to authenticate
        /// </summary>
        public string FullTenantAddress { get { return StsUrl + DomainName; } }
        /// <summary>
        /// The unique realm for the active directory 
        /// </summary>
        public string ServiceRealm { get { return String.Format("{0}/{1}@{2}", PrincipalId, GraphDomain, DomainName); } }
        /// <summary>
        /// The issuer of the domain (service principal)
        /// </summary>
        public string IssuingResource { get { return string.Format("{0}@{1}", AppPrincipalId, DomainName); } }
        /// <summary>
        /// Gets the Active Directory properties needed to return the details of the user etc.
        /// </summary>
        public static ActiveDirectoryProperties GetActiveDirectoryProperties()
        {
            return new ActiveDirectoryProperties
                {
                    DomainName = ConfigurationManager.AppSettings["DomainName"],
                    AppPrincipalId = ConfigurationManager.AppSettings["AppPrincipalId"],
                    SymmetricKey = ConfigurationManager.AppSettings["SymmetricKey"]
                };
        }
    }
}
