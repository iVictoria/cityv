using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Linq;
using Amnd = LicenseMobile.WSAmandaService;
using AClient = LicenseMobile.WSAmandaService.WSAmandaServicePortTypeClient;
using ASearch = LicenseMobile.WSAmandaService.WSSearchCriteria;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace LicenseMobile.Models
{
    public class PersonModel
    {
        public List<DisplayInfomation> displayInfoList { set; get; }
        public SearchData searchData { set; get; }

        public bool OverTen { set; get; }

        public PersonModel()
        {

        }

        // primary function when the Search button is clicked on the Finder (query) page 
        public void ExecuteQuery()
        {
            displayInfoList = new List<DisplayInfomation>();
            string licNoQuery = " ";
            string licYrQuery = " ";

            // creates a SOAP client to perform the Web service methods
            AClient ac = new AClient("WSAmandaServiceHttpSoap11Endpoint");
            using (new OperationContextScope(ac.InnerChannel))
            {
                HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty();
                requestProperty.Headers["lid"] = Models.AmandaWS.Lid;
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestProperty;

                #region SearchCriteria
                Amnd.WSValidOperator vo = ac.getValidOperators();


                ASearch fTypes = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "folderType",
                    operatorSpecified = true,
                    @operator = vo.IN,
                    value = new string[] { "GM", "CTR", "NEWS", "RVE" },
                    conjuctiveOperatorSpecified = true,
                    conjuctiveOperator = vo.NONE
                };

                ASearch firstName = new ASearch
                {
                    tableName = "People",
                    fieldName = "nameFirst",
                    value = null,
                    operatorSpecified = true,
                    @operator = vo.LIKE,
                    conjuctiveOperator = vo.AND,
                    conjuctiveOperatorSpecified = true
                };

                ASearch lastName = new ASearch
                {
                    tableName = "People",
                    fieldName = "nameLast",
                    value = null,
                    operatorSpecified = true,
                    @operator = vo.LIKE,
                    conjuctiveOperator = vo.AND,
                    conjuctiveOperatorSpecified = true
                };

                ASearch orgName = new ASearch
                {
                    tableName = "People",
                    fieldName = "organizationName",
                    value = null,
                    operatorSpecified = true,
                    @operator = vo.LIKE,
                    conjuctiveOperator = vo.AND,
                    conjuctiveOperatorSpecified = true
                };

                ASearch licenceType = new ASearch
                {
                    tableName = "folder",
                    fieldName = "subCode",
                    value = null,
                    @operator = vo.EQUAL,
                    operatorSpecified = true,
                    conjuctiveOperator = vo.AND,
                    conjuctiveOperatorSpecified = true,
                };

                ASearch licYear = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "folderYear",
                    @operator = vo.EQUAL,
                    operatorSpecified = true,
                    conjuctiveOperator = vo.AND,
                    conjuctiveOperatorSpecified = true
                };

                ASearch licenceNo = new ASearch
                {
                    tableName = "Folder",
                    fieldName = "foldersequence",
                    @operator = vo.EQUAL,
                    operatorSpecified = true,
                    conjuctiveOperator = vo.NONE,
                    conjuctiveOperatorSpecified = true
                };

                ASearch opNameSearch = new ASearch
                {
                    tableName = "FolderInfo",
                    fieldName = "infoValue",
                    infoCode = 40020,
                    infoCodeSpecified = true,
                    value = new string[] { searchData.opName },
                    conjuctiveOperator = vo.NONE,
                    conjuctiveOperatorSpecified = true,
                    @operator = vo.LIKE,
                    operatorSpecified = true
                };

                ASearch VinSearch = new ASearch
                {
                    tableName = "FolderInfo",
                    fieldName = "infoValue",
                    infoCode = 40102,
                    infoCodeSpecified = true,
                    value = new string[] { searchData.vin },
                    conjuctiveOperator = vo.NONE,
                    conjuctiveOperatorSpecified = true,
                    @operator = vo.LIKE,
                    operatorSpecified = true
                };

                ASearch ONPlate = new ASearch
                {
                    tableName = "FolderInfo",
                    fieldName = "infoValue",
                    infoCode = 40103,
                    infoCodeSpecified = true,
                    value = new string[] { searchData.plateNumber },
                    conjuctiveOperator = vo.NONE,
                    conjuctiveOperatorSpecified = true,
                    @operator = vo.LIKE,
                    operatorSpecified = true
                };

                ASearch CityPlate = new ASearch
                {
                    tableName = "FolderInfo",
                    fieldName = "infoValue",
                    infoCode = 40104,
                    infoCodeSpecified = true,
                    value = new string[] { searchData.cityPlateNumber },
                    conjuctiveOperator = vo.NONE,
                    conjuctiveOperatorSpecified = true,
                    @operator = vo.LIKE,
                    operatorSpecified = true
                };


                ASearch[] search = new ASearch[] { firstName, lastName, licenceType, orgName, fTypes };

                if (!String.IsNullOrEmpty(searchData.firstName))
                {
                    firstName.value = new string[] { searchData.firstName };
                }

                if (!String.IsNullOrEmpty(searchData.lastName))
                {
                    lastName.value = new string[] { searchData.lastName };
                }

                if (!String.IsNullOrEmpty(searchData.orgName))
                {
                    orgName.value = new string[] { searchData.orgName };
                }

                if (!String.IsNullOrEmpty(searchData.licenseType))
                {
                    //licenceType.value = new string[] { searchData.licenseType };

                    licenceType.value = new string[] { searchData.SubCode[Array.IndexOf(searchData.LicTypes, searchData.licenseType)] };
                }


                search = search.Where(c => c.value != null).ToArray(); //removes empty search criteria fields

                if (!String.IsNullOrEmpty(searchData.cityPlateNumber))
                {
                    search = null;
                    search = new ASearch[] { CityPlate };
                }

                if (!String.IsNullOrEmpty(searchData.plateNumber))
                {
                    search = null;
                    search = new ASearch[] { ONPlate };
                }


                if (!String.IsNullOrEmpty(searchData.vin))
                {
                    search = null;
                    search = new ASearch[] { VinSearch };
                }

                if (!String.IsNullOrEmpty(searchData.opName))
                {
                    search = null;
                    search = new ASearch[] { opNameSearch };
                }

                if (!String.IsNullOrEmpty(searchData.licenseNumber))
                {
                   
                    if (searchData.licenseNumber.Length > 2) // exact search
                    {
                        // separates the search data for the folder year and folder sequence searches
                        licNoQuery = Char.IsNumber(searchData.licenseNumber, 2) ? searchData.licenseNumber.Substring(2) : searchData.licenseNumber.Substring(3);
                        licYrQuery = searchData.licenseNumber.Substring(0, 2); // searches folderYear column
                        search = null;
                        licenceNo.value = new string[] { licNoQuery };
                        licYear.value = new string[] { licYrQuery };
                        search = new ASearch[] { licYear, licenceNo };
                    }
                    else // possibility to search license year with other criteria, if only to avoid app crashes when 2 chars are entered
                    {
                        search = null;
                        licYear.value = new string[] { searchData.licenseNumber };
                        search = new ASearch[] { licYear, firstName, lastName, orgName, licenceType, fTypes };
                        search = search.Where(c => c.value != null).ToArray();
                    }
                }
                #endregion

                //(new System.Xml.Serialization.XmlSerializer(search.GetType())).Serialize(new System.IO.StreamWriter(@"c:\temp\text.xml"), search);
                int cnt = ac.searchFolderCount(search);
                Amnd.WSFolder[] result;

                if (cnt > 10)
                {
                    result = ac.searchFolder(search, 0, 10, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });
                    OverTen = true;
                }
                else
                {
                    result = ac.searchFolder(search, 0, cnt, new string[] { "folder.folderRSN DESC", "folder.expiryDate DESC" });
                    OverTen = false;
                }

                if (cnt > 0)
                {
                    foreach (var r in result)
                    {
                        string vin = " ";
                        string plate = " ";
                        string modelyear = " ";
                        string make = " ";
                        string cityLic = " ";
                        string alias = " ";

                        var p = ac.getFolderPeople(Convert.ToInt32(r.folderRSN));

                        if (p != null)
                        {
                            // DOB
                            var pInfo = ac.getPeopleInfo(Convert.ToInt32(p[0].peopleRSN));
                            string dob = " ";
                            DateTime dobDb = DateTime.Today;

                            Amnd.WSPeopleInfo dobRow = null;

                            if (pInfo[0] != null && pInfo != null)
                            {
                                dobRow = Array.Find(pInfo, col => col.infoCode.Equals(40117));

                                if (!String.IsNullOrEmpty(dobRow.infoValue))
                                {
                                    if (DateTime.Parse(dobRow.infoValue) < DateTime.Today)
                                    {
                                        dobDb = DateTime.Parse(dobRow.infoValue).Date;
                                        dob = String.Format("{0:MMM d, yyyy}", dobDb);
                                    }
                                }

                            }
                            // license type
                            var fFree = ac.getFolderFreeFormByCode(Convert.ToInt32(r.folderRSN), new int[] { 100 });
                            string licType = (fFree == null && String.IsNullOrWhiteSpace(fFree[0].c01)) ? " " : fFree[0].c01;

                            // vehicle details/InfoCode
                            var fInfo = ac.getFolderInfoByInfoCode(Convert.ToInt32(r.folderRSN), new int[] { 40020, 40101, 40100, 40102, 40103, 40104 });

                            Amnd.WSFolderInfo aliasRow, modYrRow, mkRow, vinRow, ontRow, ctyRow;

                            if (fInfo != null) //&& (!String.IsNullOrWhiteSpace(fInfo[0].infoValue) || (!String.IsNullOrEmpty(fInfo[0].infoValue)))
                            {
                                aliasRow = fInfo.FirstOrDefault(col => col.infoCode == 40020);
                                alias = String.IsNullOrWhiteSpace(aliasRow.infoValue) ? " " : aliasRow.infoValue;

                                if (fInfo.Length > 2)
                                {
                                    mkRow = fInfo.FirstOrDefault(col => col.infoCode == 40101);
                                    make = String.IsNullOrWhiteSpace(mkRow.infoValue) ? " " : mkRow.infoValue;

                                    modYrRow = fInfo.FirstOrDefault(col => col.infoCode == 40100);
                                    modelyear = String.IsNullOrWhiteSpace(modYrRow.infoValue) ? " " : modYrRow.infoValue;

                                    vinRow = fInfo.FirstOrDefault(col => col.infoCode == 40102);
                                    vin = String.IsNullOrWhiteSpace(vinRow.infoValue) ? " " : vinRow.infoValue;
                                    ontRow = fInfo.FirstOrDefault(col => col.infoCode == 40103);
                                    plate = String.IsNullOrWhiteSpace(ontRow.infoValue) ? " " : ontRow.infoValue;
                                    ctyRow = fInfo.FirstOrDefault(col => col.infoCode == 40104);
                                    cityLic = String.IsNullOrWhiteSpace(ctyRow.infoValue) ? " " : ctyRow.infoValue;
                                }
                            }

                            // license number
                            string licNo = (String.IsNullOrWhiteSpace(r.folderYear) || String.IsNullOrWhiteSpace(r.folderSequence)) ? " " : (r.folderYear + " " + r.folderSequence);

                            // first & last & org names
                            string firstNm = String.IsNullOrWhiteSpace(p[0].nameFirst) ? (String.IsNullOrWhiteSpace(p[1].nameFirst) ? " " : p[1].nameFirst) : p[0].nameFirst;
                            string lastNm = String.IsNullOrWhiteSpace(p[0].nameLast) ? (String.IsNullOrWhiteSpace(p[1].nameLast) ? " " : p[1].nameLast) : p[0].nameLast;
                            string orgNm = String.IsNullOrWhiteSpace(p[0].organizationName) ? (String.IsNullOrWhiteSpace(p[1].organizationName) ? " " : p[1].organizationName) : p[0].organizationName;

                            // address
                            string addLine1 = String.IsNullOrWhiteSpace(p[0].addressLine1) ? " " : p[0].addressLine1;
                            string cityStr = String.IsNullOrWhiteSpace(p[0].addrCity) ?
                                (String.IsNullOrWhiteSpace(p[0].addressLine2) ? " " :
                                p[0].addressLine2.Substring(0, p[0].addressLine2.Length - 7)) : p[0].addrCity;
                            string postStr = String.IsNullOrWhiteSpace(p[0].addrPostal) ?
                                (String.IsNullOrWhiteSpace(p[0].addressLine2) ? " " :
                                p[0].addressLine2.Substring(p[0].addressLine2.Length - 7)) : p[0].addrPostal;

                            DisplayInfomation displayInfo = new DisplayInfomation()
                            {
                                person = new Person(),
                                card = new Card()
                            };


                            displayInfo.person.personId = Convert.ToString(p[0].peopleRSN); //+ Convert.ToString(r.folderRSN); this actually doesn't matter
                            displayInfo.person.firstName = firstNm;
                            displayInfo.person.lastName = lastNm;
                            displayInfo.person.organizationName = orgNm;
                            displayInfo.person.dateOfBirth = String.Format("{0:MMM d, yyyy}", dob);
                            displayInfo.person.address = addLine1;
                            displayInfo.person.city = cityStr;
                            displayInfo.person.postalCode = postStr;
                            displayInfo.person.operatingName = alias;
                            displayInfo.card.CardId = Convert.ToString(r.folderRSN);
                            displayInfo.card.licenseNumber = licNo;
                            displayInfo.card.vehicleMaker = make;
                            displayInfo.card.vehicleModelYear = modelyear;
                            displayInfo.card.issueDate = String.Format("{0:MMM d, yyyy}", r.indate);
                            displayInfo.card.expiryDate = String.Format("{0:MMM d, yyyy}", r.expiryDate);
                            displayInfo.card.licenseType = licType;
                            displayInfo.card.vehiclePlateNumber = plate;
                            displayInfo.card.cityLicensePlate = cityLic;
                            displayInfo.card.vin = vin;
                            displayInfo.card.personId = Convert.ToString(p[0].peopleRSN);

                            //get photo
                            try
                            {
                                var fa = ac.getFolderAttachment(Convert.ToInt32(r.folderRSN));

                                if (fa[0] != null)
                                {
                                    var attach = Array.Find(fa, col => col.attachmentCode.Equals(40005));
                                    //displayInfo.person.personId = Convert.ToString(Convert.ToInt32(attach.attachmentRSN));
                                    var pic = ac.getAttachmentContent(Convert.ToInt32(attach.attachmentRSN));

                                    displayInfo.card.imageData = pic.content;
                                }
                            }
                            catch //(FaultException)
                            {
                                // abort photo check if no image exists or the file server cannot be accessed
                                displayInfo.card.imageData = null;
                            }

                            displayInfoList.Add(displayInfo);
                        }
                    }
                }


            } // using
        }
    }
}
