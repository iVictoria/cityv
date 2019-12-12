using System;
using System.Drawing;
using Amnd = LicenseMobile.WSAmandaService;
using System.Globalization;
using AClient = LicenseMobile.WSAmandaService.WSAmandaServicePortTypeClient;
using ASearch = LicenseMobile.WSAmandaService.WSSearchCriteria;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace LicenseMobile
{
    public partial class Property2015 : System.Web.UI.Page
    {
      
        static string lid = "";

        //Variables for Bootstrap Design
        static string resultcont = "<div class='container'>";
        static string row = "<div class='row'>";
        static string box = "<div class='col-sm-6'>";
        static string inBox = "<div class='well'>";
        static string TitleAdd = "<ul class='list-group'><li class='list-group-item active'><span class='glyphicon glyphicon-map-marker' aria-hidden='true'></span><b>Address Details</b></li>";
        static string TitleLic = "<li class='list-group-item active'><span class='glyphicon glyphicon-briefcase' aria-hidden='true'></span> <b>License Details</b></li>";
        static string pendingTitleAdd = "<ul class='list-group'><li class='list-group-item list-group-item-danger'><span class='glyphicon glyphicon-map-marker' aria-hidden='true'></span> <b>Address Details</b></li>";
        static string pendingTitleLic = "<li class='list-group-item list-group-item-danger'><span class='glyphicon glyphicon-briefcase' aria-hidden='true'></span>  <b>License Details</b></li>";
        static string item = "<li class='list-group-item'>";
        static string divEnd = "</div>";
        static string ulEnd = "</ul>";
        static string liEnd = "</li>";
        static string pendingPoint = "<a name='pending'></a><br />";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "hash", "location.hash = '#result';", true);
            }

            if (String.IsNullOrEmpty(lid))
            {
                lid = Models.AmandaWS.Lid;
            }

        }
        public bool IsNumeric(string value)
        {
            Int32 isnull = 0;
            bool result = Int32.TryParse(value, out isnull);

            if (result)
            {
                if (isnull >= 0)
                    return true;
            }

            return false;
        }

        protected void lkbtnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect("Property2015.aspx");
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            int cnt = 0;
            err.Text = "";
            lbresult.Text = "";
            ShowSearchResult.Text = resultcont + row;
            string tempSNUMBER = txt_SNumber.Text;
            bool numeric = true;
            string result = ""; // pending
            int pendingCnt = 0;
            string licType = "";
            string businessname = " ";
            string applicant = " ";


            // status codes: 14 pending, 2 issued, 7 closed
            #region AmandaWS client and search criteria
            AClient ac = new AClient("WSAmandaServiceHttpSoap11Endpoint");

            using (new OperationContextScope(ac.InnerChannel))
            {
                HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
                requestProperty.Headers["lid"] = lid;
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;


                Amnd.WSValidOperator vo = ac.getValidOperators();

                ASearch fTypes = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "folderType",
                    operatorSpecified = true,
                    @operator = vo.IN,
                    value = new string[] { "BROK", "CBOX", "GL", "FWKS" },
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.AND
                };

                ASearch expDate = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "expirydate",
                    operatorSpecified = true,
                    @operator = vo.GREATER_THAN,
                    value = new string[] { String.Format("{0:dd-MMM-yyyy}", DateTime.Today.AddYears(-14)) },
                    negativeSpecified = true,
                    negative = false,
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.AND
                };

                ASearch finalDate = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "finaldate",
                    operatorSpecified = true,
                    @operator = vo.IS_NULL,
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.AND,
                    value = new string[] { vo.CLOSE_PARENTHESES.ToString() }
                };

                ASearch exStatus = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "statuscode",
                    operatorSpecified = true,
                    @operator = vo.EQUAL,
                    value = new string[] { "7" },
                    negativeSpecified = true,
                    negative = true,
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.NONE
                };

                ASearch notPending = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "statuscode",
                    operatorSpecified = true,
                    @operator = vo.IN,
                    value = new string[] { "14", "126" },
                    negativeSpecified = true,
                    negative = true,
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.AND
                };

                ASearch pendingSearch = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "statuscode",
                    operatorSpecified = true,
                    @operator = vo.IN,
                    value = new string[] { "14", "126" },
                    negativeSpecified = true,
                    negative = false,
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.NONE
                };

                ASearch bizSearch = new ASearch
                {
                    tableName = "folderinfo",
                    fieldName = "infovalue",
                    value = new string[] { " " },
                    conjuctiveOperatorSpecified = true,
                    operatorSpecified = true,
                    @operator = vo.LIKE,
                    conjuctiveOperator = vo.AND
                };

                ASearch streetSearch = new ASearch();
                ASearch civicNo = new ASearch();
                ASearch[] query = new ASearch[] { fTypes, expDate, streetSearch, civicNo, finalDate, notPending, exStatus }; // address + issued
                ASearch[] pendingQuery = new ASearch[] { fTypes, streetSearch, civicNo, pendingSearch }; //  pending + address
                ASearch[] bizQuery = new ASearch[] { fTypes, bizSearch, expDate, finalDate, notPending, exStatus }; // business name + issued
                ASearch[] pendingBiz = new ASearch[] { fTypes, bizSearch, expDate, pendingSearch }; // business name + pending
                #endregion


                #region Location Validity Check
                if ((txt_SNumber.Text.Length < 1) && (txtSName.Text.Length < 1) && (txtBName.Text.Length < 1))
                {
                    err.Text = err.Text + "<br />Please provide a location info or a business name.";
                    err.Visible = true;
                }
                else if (((txt_SNumber.Text.Length > 1) || (txtSName.Text.Length > 1)) && (txtBName.Text.Length > 1))
                {
                    err.Text = err.Text + "<br />Please provide only a location info or a business name.";
                    err.Visible = true;
                }
                else if (((txt_SNumber.Text.Length > 0) || (txtSName.Text.Length > 0) || (txtBName.Text.Length < 1)))
                {
                    if (txt_SNumber.Text.Length > 0 || txtSName.Text.Length > 0)
                    {
                        if (txt_SNumber.Text.Trim() == "")
                        {
                            txt_SNumber.Text = "0";
                        }

                        if (!IsNumeric(txt_SNumber.Text))
                        {

                            err.Text = err.Text + "<br>" + "Street Number Must be Numric ";
                            txt_SNumber.Text = "";
                            numeric = false;

                        }
                        #endregion
                        #region Location Search
                        else
                        {
                            /////////////////////////////////////////////////////////////
                            //sql = "SELECT * FROM property WHERE ";
                            if (txtSName.Text.Length > 0)
                            {
                                //string UpperSName = txtSName.Text.Replace("'", "''").ToUpper();
                                //sql = sql + " UPPER(propstreet) LIKE('%" + UpperSName + "%') AND";

                                streetSearch.tableName = "Property";
                                streetSearch.fieldName = "propStreetUpper";
                                streetSearch.value = new string[] { txtSName.Text.ToUpper().Trim() };
                                streetSearch.operatorSpecified = true;
                                streetSearch.@operator = vo.LIKE;
                                streetSearch.conjuctiveOperatorSpecified = true;
                                streetSearch.conjuctiveOperator = vo.AND;

                            }
                            if (txt_SNumber.Text.Length > 0 && txt_SNumber.Text != "0")
                            {
                                //sql = sql + " prophouse LIKE('" + txt_SNumber.Text + "') AND";

                                civicNo.tableName = "Property";
                                civicNo.fieldName = "propHouse";
                                civicNo.conjuctiveOperatorSpecified = true;
                                civicNo.operatorSpecified = true;
                                civicNo.@operator = vo.LIKE;
                                civicNo.value = new string[] { txt_SNumber.Text.Trim() };
                                civicNo.conjuctiveOperator = vo.AND;


                            }


                            try
                            {
                                //(new System.Xml.Serialization.XmlSerializer(query.GetType())).Serialize(new System.IO.StreamWriter(@"c:\temp\text.xml"), query);
                                cnt = ac.searchFolderCount(query);
                                pendingCnt = ac.searchFolderCount(pendingQuery);


                                if (cnt > 200)
                                {
                                    if (err.Text != "")
                                        err.Text += " ";
                                    err.Text = err.Text + "More than " + cnt + " issued records" + "<br>";
                                    return;
                                }



                                if (cnt > 0)
                                {

                                    var folders = ac.searchFolder(query, 0, cnt, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });

                                    foreach (var f in folders)
                                    {

                                        try
                                        {

                                            var p = ac.getProperty(Convert.ToInt32(f.propertyRSN));

                                            var bizname = ac.getFolderInfoByInfoCode(Convert.ToInt32(f.folderRSN), new int[] { 40020, 40049 });

                                            if (bizname != null)
                                            {
                                                if (!String.IsNullOrEmpty(bizname[0].infoValue))
                                                    businessname = bizname[0].infoValue;
                                                if (!String.IsNullOrEmpty(bizname[1].infoValue))
                                                    applicant = bizname[1].infoValue;
                                            }

                                            var fInfo = ac.getFolderFreeFormByCode(Convert.ToInt32(f.folderRSN), new int[] { 100 });

                                            if (fInfo == null)
                                            {
                                                licType = " "; //TODO: custom query
                                            }
                                            else
                                            {
                                                licType = fInfo[0].c01;
                                            }

                                            ShowSearchResult.Text = ShowSearchResult.Text + box + inBox;
                                            ShowSearchResult.Text = ShowSearchResult.Text + TitleAdd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>PropID       : </b>" + f.propertyRSN + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Civic Number : </b>" + p.propHouse + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Street Name  : </b>" + p.propStreet + " " + p.propStreetType + liEnd;
                                            //ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Unit Number  : </b>" + p.propUnit + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Postal Code  : </b>" + p.propPostal + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + TitleLic;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>FolderID              : </b>" + f.folderRSN + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Type                  : </b>" + licType + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Business Name         : </b>" + businessname + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Applicant: </b>   " + applicant + liEnd;
                                            ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>License Status        : </b>" + f.statusDesc + liEnd;

                                            if (!Convert.IsDBNull(f.indate))
                                            {
                                                ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Application Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.indate) + liEnd;
                                            }

                                            if (!Convert.IsDBNull(f.issueDate))
                                            {
                                                ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Issue Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.issueDate) + liEnd;
                                            }

                                            if (!Convert.IsDBNull(f.expiryDate))
                                            {
                                                ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Expiry Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.expiryDate) + liEnd;
                                            }

                                            ShowSearchResult.Text = ShowSearchResult.Text + ulEnd + divEnd + divEnd;
                                        }
                                        catch
                                        {
                                            // skip record if bad data
                                        }
                                    }
                                }


                                if (pendingCnt < 1)
                                {
                                    result += pendingPoint;
                                    //err.Text = err.Text + " No pending records." + "<br />";

                                }

                                if (pendingCnt > 200)
                                {

                                    if (err.Text != "")
                                        err.Text += " <br />";
                                    err.Text = err.Text + "More than " + pendingCnt + " Pending records" + "<br />";
                                    return;

                                }


                                if (pendingCnt > 0)
                                {
                                    var pending = ac.searchFolder(pendingQuery, 0, pendingCnt, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });

                                    foreach (var f in pending)
                                    {
                                        try
                                        {

                                            var p = ac.getProperty(Convert.ToInt32(f.propertyRSN));
                                            var bizname = ac.getFolderInfoByInfoCode(Convert.ToInt32(f.folderRSN), new int[] { 40020, 40049 });
                                            var fee = ac.getFolderFee(new int[] { Convert.ToInt32(f.folderRSN) }, true);
                                            var fInfo = ac.getFolderFreeFormByCode(Convert.ToInt32(f.folderRSN), new int[] { 100 });

                                            if (bizname != null)
                                            {
                                                if (!String.IsNullOrEmpty(bizname[0].infoValue))
                                                    businessname = bizname[0].infoValue;
                                                if (!String.IsNullOrEmpty(bizname[1].infoValue))
                                                    applicant = bizname[1].infoValue;
                                            }

                                            double outFee = 0;
                                            if (fee != null) outFee = fee.totalOutstanding;

                                            if (fInfo == null)
                                            {
                                                licType = " "; //TODO: custom query
                                            }
                                            else
                                            {
                                                licType = fInfo[0].c01;
                                            }

                                            result = result + box + inBox;
                                            result += pendingPoint;

                                            result = result + pendingTitleAdd;
                                            result = result + item + "<b>Civic Number : </b>" + p.propHouse + liEnd;
                                            result = result + item + "<b>Street Name  : </b>" + p.propStreet + " " + p.propStreetType + liEnd;
                                            //result = result + item + "<b>Unit Number  : </b>" + p.propUnit + liEnd;
                                            result = result + item + "<b>City         : </b>" + p.propCity + liEnd;
                                            result = result + item + "<b>Postal Code         : </b>" + p.propPostal + liEnd;

                                            result = result + pendingTitleLic;
                                            result = result + item + "<b>Type                  : </b>" + licType + liEnd;
                                            result = result + item + "<b>Business Name        : </b>" + businessname + liEnd;
                                            //result = result + item + "<b>Applicant        : </b>" + applicant + liEnd;
                                            result = result + item + "<b>Status               : </b> Pending<br>";
                                            result = result + item + "<b>Application Date     : </b>" + String.Format("{0:MMM d, yyyy}", f.indate) + liEnd;
                                            result = result + item + "<b>Outstanding Fee      : </b>$" + outFee + liEnd;
                                            result = result + item + "<b>Outstanding Comments      : </b>" + f.folderCondition + liEnd;

                                            result = result + ulEnd + divEnd + divEnd;
                                        }
                                        catch
                                        {
                                            // skip if bad data
                                        }
                                    }


                                }


                            }
                            catch (Exception exp)
                            {
                                err.Visible = true;
                                err.Text = err.Text + "<br>";
                                err.Text = err.Text + "Error : " + exp.Message + "<br>";
                                err.Text = err.Text + "Error Detail : " + exp.StackTrace + "<br>";

                            }
                            #endregion

                        }//if(!IsNumeric(txt_SNumber.Text))

                        txtSName.Text = txtSName.Text.Replace("''", "'");
                        txt_SNumber.Text = txt_SNumber.Text.Replace("''", "'");

                        if (cnt < 1 && numeric)
                        {
                            err.Text = err.Text + "No records for <b>location info " + txt_SNumber.Text + " " + txtSName.Text + "</b> in Issued records." + "<br/>";

                        }

                        else if (numeric)
                        {
                            if (cnt > 1)
                            {
                                lbresult.Text = lbresult.Text + "<span class='badge'>" + cnt + "</span><span class='label label-primary'>  Issued records</span>";
                            }
                            else
                            {
                                lbresult.Text = lbresult.Text + "<span class='badge'>" + cnt + "</span><span class='label label-primary'>  Issued record</span>";
                            }

                        }

                        if (pendingCnt > 1)
                        {
                            lbresult.Text = lbresult.Text + "<br /><span class='badge'>" + pendingCnt + "</span><a href='#pending'><span class='label label-danger'>Pending records</span></a>";

                        }
                        else if (pendingCnt == 1)
                        {

                            lbresult.Text = lbresult.Text + "<br /><span class='badge'>" + pendingCnt + "</span><a href='#pending'><span class='label label-danger'>Pending record</span></a>";
                        }

                        else
                        {
                            err.Text = err.Text + " No pending records." + "<br />";

                        }

                        if (IsNumeric(txt_SNumber.Text))
                            ShowSearchResult.Text = ShowSearchResult.Text + result; //+ GetPendingLicenses(txt_SNumber.Text, txtSName.Text, txtUnit.Text, txtBName.Text);
                        txtBName.Text = "";
                        txt_SNumber.Text = tempSNUMBER;

                    }
                } // Location search 
                #region Business Name Search
                else if (((txt_SNumber.Text.Length < 1) && (txtSName.Text.Length < 1)) && (txtBName.Text.Length > 0))
                {
                    err.Text = "";
                    ShowSearchResult.Text = "";
                    ShowSearchResult.Text = resultcont + row;

                    bizSearch.value = new string[] { txtBName.Text.TrimEnd() };

                    //(new System.Xml.Serialization.XmlSerializer(query.GetType())).Serialize(new System.IO.StreamWriter(@"c:\temp\text.xml"), query);

                    try
                    {

                        cnt = ac.searchFolderCount(bizQuery);
                        pendingCnt = ac.searchFolderCount(pendingBiz);

                        if (cnt == 0)
                        {
                            err.Text = err.Text + "No records for <b>Business Name including " + txtBName.Text + "</b> in Issued records." + "<br />";

                        }

                        //else if (cnt == 0 && pendingCnt > 0)
                        //{
                        //    err.Text = err.Text + "No records for <b>Business Name including " + txtBName.Text + "</b> in Issued records." + "<br />";
                        //}

                        //else
                        //{
                        var folders = ac.searchFolder(bizQuery, 0, cnt, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });

                        if (cnt > 200)
                        {
                            if (err.Text != "")
                                err.Text += " ";
                            err.Text = err.Text + "More than " + cnt + " records" + "<br />";
                            return;
                        }

                        if (cnt > 0)
                        {

                            foreach (var fold in folders)
                            {
                                try
                                {
                                    var f = ac.getFolder(Convert.ToInt32(fold.folderRSN));
                                    var p = ac.getProperty(Convert.ToInt32(f.propertyRSN));

                                    var bizname = ac.getFolderInfoByInfoCode(Convert.ToInt32(f.folderRSN), new int[] { 40020, 40049 });
                                    var fInfo = ac.getFolderFreeFormByCode(Convert.ToInt32(f.folderRSN), new int[] { 100 });

                                    if (bizname != null)
                                    {
                                        if (!String.IsNullOrEmpty(bizname[0].infoValue))
                                            businessname = bizname[0].infoValue;
                                        if (!String.IsNullOrEmpty(bizname[1].infoValue))
                                            applicant = bizname[1].infoValue;
                                    }

                                    if (fInfo == null)
                                    {
                                        licType = " "; //TODO: custom query
                                    }
                                    else
                                    {
                                        licType = fInfo[0].c01;
                                    }


                                    ShowSearchResult.Text = ShowSearchResult.Text + box + inBox;
                                    ShowSearchResult.Text = ShowSearchResult.Text + TitleAdd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>PropID       : </b>" + f.propertyRSN + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Civic Number : </b>" + p.propHouse + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Street Name  : </b>" + p.propStreet + " " + p.propStreetType + liEnd;
                                    //ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Unit Number  : </b>" + p.propUnit + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Postal Code  : </b>" + p.propPostal + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + TitleLic;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>FolderID              : </b>" + f.folderRSN + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Type                  : </b>" + licType + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Business Name         : </b>" + businessname + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Applicant: </b>   " + applicant + liEnd;
                                    ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>License Status        : </b>" + f.statusDesc + liEnd;

                                    if (!Convert.IsDBNull(f.indate))
                                    {
                                        ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Application Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.indate) + liEnd;
                                    }

                                    if (!Convert.IsDBNull(f.issueDate))
                                    {
                                        ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Issue Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.issueDate) + liEnd;
                                    }

                                    if (!Convert.IsDBNull(f.expiryDate))
                                    {
                                        ShowSearchResult.Text = ShowSearchResult.Text + item + "<b>Expiry Date : " + "</b>" + String.Format("{0:MMM d, yyyy}", f.expiryDate) + liEnd;
                                    }

                                    ShowSearchResult.Text = ShowSearchResult.Text + ulEnd + divEnd + divEnd;
                                }
                                catch
                                {
                                    // skip if bad data
                                }
                            }
                        }

                        #region Pending Search
                        if (pendingCnt > 200)
                        {
                            if (err.Text != "")
                                err.Text += " <br />";
                            err.Text = err.Text + "More than " + pendingCnt + " Pending records" + "<br />";
                            return;
                        }

                        if (pendingCnt < 1)
                        {
                            result += pendingPoint;
                            err.Text = err.Text + " No pending records." + "<br />";

                        }

                        if (pendingCnt > 0 && pendingCnt < 200)
                        {
                            var pending = ac.searchFolder(pendingBiz, 0, pendingCnt, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });

                            foreach (var f in pending)
                            {
                                try
                                {
                                    var p = ac.getProperty(Convert.ToInt32(f.propertyRSN));
                                    var bizname = ac.getFolderInfoByInfoCode(Convert.ToInt32(f.folderRSN), new int[] { 40020, 40049 });
                                    var fee = ac.getFolderFee(new int[] { Convert.ToInt32(f.folderRSN) }, true);
                                    var fInfo = ac.getFolderFreeFormByCode(Convert.ToInt32(f.folderRSN), new int[] { 100 });

                                    if (bizname != null)
                                    {
                                        if (!String.IsNullOrEmpty(bizname[0].infoValue))
                                            businessname = bizname[0].infoValue;
                                        if (!String.IsNullOrEmpty(bizname[1].infoValue))
                                            applicant = bizname[1].infoValue;
                                    }

                                    if (fInfo == null)
                                    {
                                        licType = " "; //TODO: custom query
                                    }
                                    else
                                    {
                                        licType = fInfo[0].c01;
                                    }

                                    double outFee = 0;
                                    if (fee != null) outFee = fee.totalOutstanding;


                                    result = result + box + inBox;
                                    result += pendingPoint;

                                    result = result + pendingTitleAdd;
                                    result = result + item + "<b>Civic Number : </b>" + p.propHouse + liEnd;
                                    result = result + item + "<b>Street Name  : </b>" + p.propStreet + " " + p.propStreetType + liEnd;
                                    //result = result + item + "<b>Unit Number  : </b>" + p.propUnit + liEnd;
                                    result = result + item + "<b>City         : </b>" + p.propCity + liEnd;
                                    result = result + item + "<b>Postal Code         : </b>" + p.propPostal + liEnd;

                                    result = result + pendingTitleLic;
                                    result = result + item + "<b>Type                  : </b>" + licType + liEnd;
                                    result = result + item + "<b>Business Name        : </b>" + businessname + liEnd;
                                    //result = result + item + "<b>Applicant        : </b>" + applicant + liEnd;
                                    result = result + item + "<b>Status               : </b> Pending<br>";
                                    result = result + item + "<b>Application Date     : </b>" + String.Format("{0:MMM d, yyyy}", f.indate) + liEnd;
                                    result = result + item + "<b>Outstanding Fee      : </b>$" + outFee + liEnd;
                                    result = result + item + "<b>Outstanding Comments      : </b>" + f.folderCondition + liEnd;

                                    result = result + ulEnd + divEnd + divEnd;
                                }
                                catch
                                {
                                    // skip if bad data
                                }

                            }


                        }
                        #endregion

                        if (cnt == 1)
                        {
                            lbresult.Text = lbresult.Text + "<span class='badge'>" + cnt + "</span><span class='label label-primary'>  Issued record</span>";
                        }
                        else if (cnt > 1)
                        {
                            lbresult.Text = lbresult.Text + "<span class='badge'>" + cnt + "</span><span class='label label-primary'>  Issued records</span>";
                        }

                        if (pendingCnt > 1)
                        {
                            lbresult.Text = lbresult.Text + "<br /><span class='badge'>" + pendingCnt + "</span><a href='#pending'><span class='label label-danger'>Pending records</span></a>";

                        }
                        else if (pendingCnt == 1)
                        {

                            lbresult.Text = lbresult.Text + "<br /><span class='badge'>" + pendingCnt + "</span><a href='#pending'><span class='label label-danger'>Pending record</span></a>";
                        }


                        //} // else

                    }
                    catch (Exception ex)
                    {
                        err.Visible = true;
                        err.Text = err.Text + "<br>";
                        err.Text = err.Text + "Error : " + ex.Message + "<br>";
                        err.Text = err.Text + "Error Detail : " + ex.StackTrace + "<br>";

                    }

                    #endregion

                    ShowSearchResult.Text = ShowSearchResult.Text + result;

                }//Business name search

                ShowSearchResult.Text = ShowSearchResult.Text + divEnd;
                err.ForeColor = Color.Red;






            }
        } // using
    }
}