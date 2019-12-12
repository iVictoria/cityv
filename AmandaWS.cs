using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using Amsc = LicenseMobile.WSAmandaSecurity;
using Amnd = LicenseMobile.WSAmandaService;
using AClient = LicenseMobile.WSAmandaService.WSAmandaServicePortTypeClient;
using ASearch = LicenseMobile.WSAmandaService.WSSearchCriteria;


namespace LicenseMobile.Models
{

    public class AmandaWS
    {
        public static WSAmandaSecurity.WSPublicLoginToken auth_token = null;
        protected static string token_lid;

        protected static string GetTokenLid()
        {
            Amsc.WSAmandaSecurityServicePortTypeClient asec = new Amsc.WSAmandaSecurityServicePortTypeClient("WSAmandaSecurityServiceHttpSoap11Endpoint");

            using (new OperationContextScope(asec.InnerChannel))
            {
                HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
                requestProperty.Headers["system"] = "connectionName"; 
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;

                auth_token = asec.authenticatePublicUser("username", "password");

            }

            return auth_token.lid;
        }


        public static string Lid
        {
            get
            {
                if (token_lid == null)
                {
                    token_lid = GetTokenLid();
                }

                return token_lid;

            }
            set
            {

                token_lid = GetTokenLid();
            }
        }

    }
}