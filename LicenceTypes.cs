using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amnd = LicenseMobile.WSAmandaService;
using AClient = LicenseMobile.WSAmandaService.WSAmandaServicePortTypeClient;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Mvc;

namespace LicenseMobile.Models
{
    public class LicenceTypes
    {
        public static Amnd.WSTransactionResponse[] GetTypes()
        {
            Amnd.WSTransactionResponse[] LicTypes = null;

            Amnd.WSTransactionRequest transObj = new Amnd.WSTransactionRequest();
            Amnd.WSTransactionRequest[] transAr = new Amnd.WSTransactionRequest[] { transObj };

            AClient ac = new AClient("WSAmandaServiceHttpSoap11Endpoint");

            using (new OperationContextScope(ac.InnerChannel))
            {
                HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
                requestProperty.Headers["lid"] = Models.AmandaWS.Lid;
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;

                var types = ac.executeCustomTransaction(4002, transAr);
                LicTypes = types;

            }

            return LicTypes;
        }

        //public static string[] GetLicenceTypes()
        //{      
        //    string[] LicTypes = null;

        //    Amnd.WSTransactionRequest transObj = new Amnd.WSTransactionRequest();
        //    Amnd.WSTransactionRequest[] transAr = new Amnd.WSTransactionRequest[] { transObj };

        //    AClient ac = new AClient("WSAmandaServiceHttpSoap11Endpoint");

        //    using (new OperationContextScope(ac.InnerChannel))
        //    {
        //        HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
        //        requestProperty.Headers["lid"] = Models.AmandaWS.Lid;
        //        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;

        //        var types = ac.executeCustomTransaction(4002, transAr);

        //        LicTypes = types[1].columnValues;

        //    }

        //    return LicTypes;
        //}

        //public static string[] GetSubCodes()
        //{
        //    string[] SubCodes = null;

        //    Amnd.WSTransactionRequest transObj = new Amnd.WSTransactionRequest();
        //    Amnd.WSTransactionRequest[] transAr = new Amnd.WSTransactionRequest[] { transObj };

        //    AClient ac = new AClient("WSAmandaServiceHttpSoap11Endpoint");

        //    using (new OperationContextScope(ac.InnerChannel))
        //    {
        //        HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
        //        requestProperty.Headers["lid"] = Models.AmandaWS.Lid;
        //        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;

        //        var types = ac.executeCustomTransaction(4002, transAr);
              
        //        SubCodes = types[0].columnValues;

        //    }

        //    return SubCodes;
        //}

    }
}