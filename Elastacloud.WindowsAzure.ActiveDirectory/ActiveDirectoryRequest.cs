using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure.ActiveDirectory;
using Microsoft.WindowsAzure.ActiveDirectory.Authentication;

namespace Elastacloud.WindowsAzure.ActiveDirectory
{
    public class ActiveDirectoryRequest
    {
        public DirectoryDataService Service { get; set; }
        public ActiveDirectoryProperties Properties { get; set; }

        public ActiveDirectoryRequest(ActiveDirectoryProperties properties)
        {
            Properties = properties;
            Setup();
        }

        public void Setup()
        {
            Service = new DirectoryDataService(Properties.ConnectionUri)
                          {
                              IgnoreResourceNotFoundException = true,
                              MergeOption = MergeOption.OverwriteChanges,
                              AddAndUpdateResponsePreference = DataServiceResponsePreference.IncludeContent
                          };

            // This flags ignores the resource not found exception
            // If AzureAD Service throws this exception, it returns null
            // This adds the default required headers to each request
            AddHeaders();
        }

        /// <summary>
        /// This method modifies the sending request event to add requied headers in the request
        /// So these headers are added to each request sent
        /// </summary>
        private void AddHeaders()
        {
            Service.SendingRequest += delegate(object sender1, SendingRequestEventArgs args)
                    {
                        var request = ((HttpWebRequest) args.Request);
                        request.Headers.Add(ActiveDirectoryConstants.HeaderNameAuthorization, GetAuthorizationHeader());
                        request.Headers.Add(ActiveDirectoryConstants.HeaderNameDataContractVersion, Properties.Version);
                        if (!String.IsNullOrEmpty(Properties.ReplicaSessionKeyHeader))
                        {
                            request.Headers.Add(ActiveDirectoryConstants.HeaderNameReplicaSessionKey, Properties.ReplicaSessionKeyHeader);
                        }

                        Guid clientRequestId = Guid.NewGuid();
                        request.Headers.Add(ActiveDirectoryConstants.HeaderNameClientRequestId, clientRequestId.ToString());
                    };
        }

        /// <summary>
        /// Used to get the HTTP authorization header
        /// </summary>
        /// <returns>Returned a string value containing the auth text value</returns>
        private string GetAuthorizationHeader()
        {
            string authzHeader = null;

            try
            {
                var context = new AuthenticationContext(Properties.FullTenantAddress);
                var credential = new SymmetricKeyCredential(Properties.IssuingResource, Convert.FromBase64String(Properties.SymmetricKey));
                var token = context.AcquireToken(Properties.ServiceRealm, credential);
                authzHeader = token.CreateAuthorizationHeader();
            }
            catch (Exception ex)
            {
                var aex = ex as AALException;
                throw new ApplicationException(aex.Message);
            }

            return authzHeader;
        }

        /// <summary>
        /// Extracts the replica session key header from the serviceResponse.
        /// </summary>
        /// <param name="operationResponse">Data service serviceResponse (query or changeResponse).</param>
        public void ProcessResponseHeader(OperationResponse operationResponse)
        {
            string replicaSessionKeyHeader;
            if (operationResponse.Headers.TryGetValue(ActiveDirectoryConstants.HeaderNameReplicaSessionKey, out replicaSessionKeyHeader))
            {
                Properties.ReplicaSessionKeyHeader = replicaSessionKeyHeader;
            }
        }

        /// <summary>
        /// Extracts the replica session key header from the serviceResponse.
        /// </summary>
        /// <param name="response">Data service serviceResponse (query or changeResponse).</param>
        public void ProcessResponseHeader(DataServiceResponse response)
        {
            foreach (ChangeOperationResponse change in response)
            {
                ProcessResponseHeader(change);
            }
        }

        /// <summary>
        /// Send a raw OData query with a particular return type 
        /// </summary>
        public List<ReferencedObject> RawQuery(string term1, string term2, string objectId)
        {
            string query = string.Format("{0}/{1}('{2}')/{3}", Properties.ConnectionUri, term1, objectId, term2);
            var result = Service.Execute<ReferencedObject>(new Uri(query));
            return result.ToList();
        }

        /// <summary>
        /// Gets a stream of data given a resource - doesn't do anything with content type so you have to know what this is currently
        /// </summary>
        public byte[] GetStreamData(DirectoryObject entity, string resource, int length = 32768)
        {
            DataServiceStreamResponse response = null;
            try
            {
                response = Service.GetReadStream(entity, resource, new DataServiceRequestArgs());
            }
            catch (DataServiceClientException ex)
            {
                var pex = ActiveDirectoryParsedException.Parse(ex);
                if (pex.Code == ActiveDirectoryConstants.MessageIdResourceNotFound || pex.Code == ActiveDirectoryConstants.MessageIdBadRequest)
                    return null;
            }

            if (response == null)
                return null;

            var buffer = new byte[length];

            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    int read = response.Stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }

                    ms.Write(buffer, 0, read);
                }
            }
        }

        #region Senders and Handlers

        /// <summary>
        /// Delegate for invoking networking operations with retry.
        /// </summary>
        /// <param name="operation">the operation to invoke with retry</param>
        public void InvokeOperationWithRetry(Action operation)
        {
            int retryremaining = ActiveDirectoryConstants.MaxRetryAttempts;
            while (retryremaining > 0)
            {
                try
                {
                    operation();

                    // The operation is successful, so retryremaining=0
                    retryremaining = 0;
                }
                catch (InvalidOperationException ex)
                {
                    // Operation not successful

                    // De-serialize error message to check the error code from AzureAD Service
                    var parsedException = ActiveDirectoryParsedException.Parse(ex);
                    if (parsedException == null)
                    {
                        // Could not parse the exception so it wasn't in the format of DataServiceException
                        throw ex;
                    }
                    else
                    {
                        // Look at the error code to determine if we want to retry on this exception 
                        switch (parsedException.Code)
                        {
                                // These are the errors we dont want to rety on
                                // Please look at the definistions for details about each of these
                            case ActiveDirectoryConstants.MessageIdAuthorizationIdentityDisabled:
                            case ActiveDirectoryConstants.MessageIdAuthorizationIdentityNotFound:
                            case ActiveDirectoryConstants.MessageIdAuthorizationRequestDenied:
                            case ActiveDirectoryConstants.MessageIdBadRequest:
                            case ActiveDirectoryConstants.MessageIdBindingRedirectionInternalServerError:
                            case ActiveDirectoryConstants.MessageIdContractVersionHeaderMissing:
                            case ActiveDirectoryConstants.MessageIdHeaderNotSupported:
                            case ActiveDirectoryConstants.MessageIdInternalServerError:
                            case ActiveDirectoryConstants.MessageIdInvalidDataContractVersion:
                            case ActiveDirectoryConstants.MessageIdInvalidReplicaSessionKey:
                            case ActiveDirectoryConstants.MessageIdInvalidRequestUrl:
                            case ActiveDirectoryConstants.MessageIdMediaTypeNotSupported:
                            case ActiveDirectoryConstants.MessageIdMultipleObjectsWithSameKeyValue:
                            case ActiveDirectoryConstants.MessageIdObjectNotFound:
                            case ActiveDirectoryConstants.MessageIdResourceNotFound:
                            case ActiveDirectoryConstants.MessageIdThrottledPermanently:
                            case ActiveDirectoryConstants.MessageIdThrottledTemporarily:
                            case ActiveDirectoryConstants.MessageIdUnauthorized:
                            case ActiveDirectoryConstants.MessageIdUnknown:
                            case ActiveDirectoryConstants.MessageIdUnsupportedQuery:
                            case ActiveDirectoryConstants.MessageIdUnsupportedToken:
                                {
                                    retryremaining = 0;

                                    // We just create a new expection with the msg
                                    // and throw it so that the 'OnException' handler handles it
                                    throw new InvalidOperationException(parsedException.Message.Value, ex);
                                }

                                // This exception means that the user's data is not in current DataCenter, 
                                // special handling is required for this exception
                            case ActiveDirectoryConstants.MessageIdBindingRedirection:
                                {
                                    HandleBindingRedirectionException(parsedException, operation);
                                    retryremaining = 0;
                                    break;
                                }

                                // This means that the replica we were trying to go to was unavailable, 
                                // retry will possibly go to another replica and may work
                            case ActiveDirectoryConstants.MessageIdReplicaUnavailable:
                                {
                                    retryremaining--;
                                    break;
                                }

                                // This means that the token has expired. 
                            case ActiveDirectoryConstants.MessageIdExpired:
                                {
                                    // Renew the token and retry the operation
                                    retryremaining--;
                                    break;
                                }

                            default:
                                {
                                    // Not sure what happened, dont want to retry
                                    retryremaining = 0;
                                    break;
                                }
                        }
                    }
                }
                finally
                {
                    foreach (EntityDescriptor ed in Service.Entities)
                    {
                        if (ed.State == EntityStates.Added || ed.State == EntityStates.Deleted ||
                            ed.State == EntityStates.Modified)
                        {
                            Service.Detach(ed.Entity);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to handle binding redirection exception. This exception means that the 
        /// user's data is located in another data center. This exception's details returns
        /// several urls that may work in this case. At least one url is guaranteed to work
        /// So we need to get all the URLs and try them
        /// </summary>
        /// <param name="parsedException">The binding redirection exception we received</param>
        /// <param name="operation">The operation to try</param>
        private void HandleBindingRedirectionException(ActiveDirectoryParsedException parsedException, Action operation)
        {
            var urls = (from ed in parsedException.Values.ErrorDetail where ed.Name.StartsWith("Url") select ed.Value).ToList();

            // Go thru the error details name\value pair

            // Now try each URL
            foreach (string newUrl in urls)
            {
                // We permanantly change the dataservice to point to the new URL
                // as none of the operations will work on the current url
                Service = new DirectoryDataService(new Uri(string.Format("{0}/{1}", newUrl, Properties.FullTenantAddress)));

                // This adds the default required headers to each request
                AddHeaders();

                try
                {
                    // try the operation
                    operation();

                    // if the operation is successful, break out of the loop
                    // all the subsequent operations will go to the new URL
                    break;
                }
                catch (Exception)
                {
                    // nothing can be done, try next URL
                }
            }
        }
        #endregion
    }
}
