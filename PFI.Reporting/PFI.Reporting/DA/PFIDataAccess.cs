using Mongoose.Core.Common;
using Mongoose.Core.Extensions;
using Mongoose.IDO;
using Mongoose.IDO.Protocol;
using PFI.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFI.Reporting.DA
{
    public class PFIDataAccess
    {
        private IIDOExtensionClassContext Context { get; set; }
        public PFIDataAccess(IIDOExtensionClassContext context)
        {
            this.Context = context;
        }
        public SLFamCodeAll[] SLFamCodeAlls(string SiteRef)
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            SLFamCodeAll slFamCodeAll;
            List<SLFamCodeAll> slFamCodeAlls = new List<SLFamCodeAll>();

            try
            {
                request = new LoadCollectionRequestData
                {
                    IDOName = "SLFamCodeAlls",
                    PropertyList = new PropertyList("SiteRef, FamilyCode, Description"),
                    Filter = $"SiteRef = '{SiteRef}'",
                    OrderBy = "SiteRef, FamilyCode, Description",
                    RecordCap = 0
                };

                response = Context.Commands.LoadCollection(request);

                for (int i = 0; i < response.Items.Count; i++)
                {
                    slFamCodeAll = new SLFamCodeAll();
                    slFamCodeAll.SiteRef = response[i, "SiteRef"].Value;
                    slFamCodeAll.FamilyCode = LowerTrim(response[i, "FamilyCode"].Value);
                    slFamCodeAll.Description = response[i, "Description"].Value;

                    slFamCodeAlls.Add(slFamCodeAll);
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("{0}{1}{2}", "SLFamCodeAlls", Environment.NewLine, ex.Message)));
            }

            return slFamCodeAlls.ToArray();
        }
        public SLSlsmanAll[] SLSlsmanAlls(string SiteRef)
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            SLSlsmanAll slSlsmanAll;
            List<SLSlsmanAll> slSlsmanAlls = new List<SLSlsmanAll>();

            try
            {
                request = new LoadCollectionRequestData
                {
                    IDOName = "SLSlsmanAlls",
                    PropertyList = new PropertyList("SiteRef, Slsman, DerSlsmanName"),
                    Filter = $"SiteRef = '{SiteRef}'",
                    OrderBy = "SiteRef, Slsman, DerSlsmanName",
                    RecordCap = 0
                };

                response = Context.Commands.LoadCollection(request);

                for (int i = 0; i < response.Items.Count; i++)
                {
                    slSlsmanAll = new SLSlsmanAll();
                    slSlsmanAll.SiteRef = response[i, "SiteRef"].Value;
                    slSlsmanAll.Slsman = LowerTrim(response[i, "Slsman"].Value);
                    slSlsmanAll.DerSlsmanName = response[i, "DerSlsmanName"].Value;

                    slSlsmanAlls.Add(slSlsmanAll);
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("{0}{1}{2}", "SLSlsmanAlls", Environment.NewLine, ex.Message)));
            }

            return slSlsmanAlls.ToArray();
        }
        public PFI_FiscalYear[] FiscalYears(string SiteRef)
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            PFI_FiscalYear ue_PFI_PeriodsAll;
            List<PFI_FiscalYear> ue_PFI_PeriodsAlls = new List<PFI_FiscalYear>();

            try
            {
                request = new LoadCollectionRequestData
                {
                    IDOName = "ue_PFI_PeriodsAlls",
                    PropertyList = new PropertyList("SiteRef, FiscalYear"),
                    Filter = $"SiteRef = '{SiteRef}'",
                    OrderBy = "SiteRef, FiscalYear",
                    Distinct = true,
                    RecordCap = 0
                };

                response = Context.Commands.LoadCollection(request);

                for (int i = 0; i < response.Items.Count; i++)
                {
                    ue_PFI_PeriodsAll = new PFI_FiscalYear();
                    ue_PFI_PeriodsAll.SiteRef = response[i, "SiteRef"].Value;
                    ue_PFI_PeriodsAll.FiscalYear = response[i, "FiscalYear"].GetValue<int>(-1);

                    ue_PFI_PeriodsAlls.Add(ue_PFI_PeriodsAll);
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("{0}{1}{2}", "ue_PFI_PeriodsAlls", Environment.NewLine, ex.Message)));
            }

            return ue_PFI_PeriodsAlls.ToArray();
        }
        public ue_PFI_SalespersonFCBudgetAll[] ue_PFI_SalespersonFCBudgetAll(string SiteRef)
        {
            //Infor applies record caps to CAs.  I cannot control what the caps are.
            //So what this does is it loops through pulling from the IDO.  It will pull as many records as 
            //possible and if the returned record count isn't 0 it will try pulling more based on the RP.
            //What this means is that we are garanteed at least 2 IDO hits.
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            ue_PFI_SalespersonFCBudgetAll ue_PFI_SalespersonFCBudgetAll;
            List<ue_PFI_SalespersonFCBudgetAll> ue_PFI_SalespersonFCBudgetAlls = new List<ue_PFI_SalespersonFCBudgetAll>();
            string filter;
            string lastRowPointer = null;
            int batchSize; 

            try
            {
                batchSize = 0;
                do
                {
                    if (string.IsNullOrWhiteSpace(lastRowPointer))
                    {
                        filter = $"SiteRef = '{SiteRef}'";
                    }
                    else
                    {
                        //If it isn't the first loop I want to grab the next batch by looking for RP greater than the last RP found.
                        filter = $"SiteRef = '{SiteRef}' and RowPointer > '{lastRowPointer}'";
                    }

                    request = new LoadCollectionRequestData
                    {
                        IDOName = "ue_PFI_SalespersonFCBudgetAlls",
                        PropertyList = new PropertyList("SiteRef, FiscalYear, FamilyCode, SalesPerson, Budget, ActualOverride, Notes, RowPointer, FamilyCodeCategory"),
                        Filter = filter,
                        OrderBy = "RowPointer", //Sorting by RowPointer so no duplicates in the result set.
                        RecordCap = 0 //It means pull back as many records as possible.
                    };

                    response = Context.Commands.LoadCollection(request);

                    batchSize = response.Items.Count;

                    for (int i = 0; i < response.Items.Count; i++)
                    {
                        ue_PFI_SalespersonFCBudgetAll = new ue_PFI_SalespersonFCBudgetAll();
                        ue_PFI_SalespersonFCBudgetAll.SiteRef = response[i, "SiteRef"].Value;
                        ue_PFI_SalespersonFCBudgetAll.FiscalYear = response[i, "FiscalYear"].GetValue<int>(-1);
                        ue_PFI_SalespersonFCBudgetAll.FamilyCode = LowerTrim(response[i, "FamilyCode"].Value);
                        ue_PFI_SalespersonFCBudgetAll.SalesPerson = LowerTrim(response[i, "SalesPerson"].Value);
                        ue_PFI_SalespersonFCBudgetAll.Budget = response[i, "Budget"].GetValue<decimal>(-1);
                        ue_PFI_SalespersonFCBudgetAll.ActualOverride = response[i, "ActualOverride"].GetValue<decimal>(-1);
                        ue_PFI_SalespersonFCBudgetAll.Notes = response[i, "Notes"].Value;
                        ue_PFI_SalespersonFCBudgetAll.FamilyCodeCategory = response[i, "FamilyCodeCategory"].Value;

                        //Want the last rowpointer found in the response so I can get the next batch.
                        lastRowPointer = response[i, "RowPointer"].Value;
                        ue_PFI_SalespersonFCBudgetAlls.Add(ue_PFI_SalespersonFCBudgetAll);
                    }
                } while (batchSize > 0);
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("{0}{1}{2}", "ue_PFI_SalespersonFCBudgetAll", Environment.NewLine, ex.Message)));
            }

            return ue_PFI_SalespersonFCBudgetAlls.ToArray();
        }
        public ue_PFI_GrossProfitReportSale[] ue_PFI_GrossProfitReportSale(string SiteRef)
        {
            //I checked by exporting all ~11k records to excel and making sure rowpointer was unique.
            //Infor applies record caps to CAs.  I cannot control what the caps are.
            //So what this does is it loops through pulling from the IDO.  It will pull as many records as 
            //possible and if the returned record count isn't 0 it will try pulling more based on the RP.
            //What this means is that we are garanteed at least 2 IDO hits.
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            ue_PFI_GrossProfitReportSale ue_PFI_GrossProfitReportSale;
            List<ue_PFI_GrossProfitReportSale> ue_PFI_GrossProfitReportSales = new List<ue_PFI_GrossProfitReportSale>();
            string filter;
            string lastRowPointer = null;
            int batchSize;

            try
            {
                batchSize = 0;
                do
                {
                    if (string.IsNullOrWhiteSpace(lastRowPointer))
                    {
                        filter = $"SiteRef = '{SiteRef}'";
                    }
                    else
                    {
                        //If it isn't the first loop I want to grab the next batch by looking for RP greater than the last RP found.
                        filter = $"SiteRef = '{SiteRef}' and RowPointer > '{lastRowPointer}' ";
                    }
                    
                    request = new LoadCollectionRequestData
                    {
                        IDOName = "ue_PFI_GrossProfitReportSales",
                        PropertyList = new PropertyList("SiteRef, " +
                            "CoLine, CoNum, CoRelease, DerExtendedPrice, DerFamilyCode, DerFiscalYear, " +
                            "DerIsNonItemCoLine, DerItem, DerSalesPerson, DerSalesmanName, InvLine, InvNum, InvoiceDate, " +
                            "InvSeq, ItemProductCode, Price, QtyInvoiced, SalesPersonOutside, SalesPersonRefNum, " +
                            "RowPointer"),
                        Filter = filter,
                        OrderBy = "RowPointer", //Sorting by RowPointer so no duplicates in the result set.
                        RecordCap = 0 //It means pull back as many records as possible.
                    };

                    response = Context.Commands.LoadCollection(request);

                    batchSize = response.Items.Count;

                    for (int i = 0; i < response.Items.Count; i++)
                    {
                        ue_PFI_GrossProfitReportSale = new ue_PFI_GrossProfitReportSale();
                        ue_PFI_GrossProfitReportSale.SiteRef = response[i, "SiteRef"].Value;
                        ue_PFI_GrossProfitReportSale.DerFamilyCode = LowerTrim(response[i, "DerFamilyCode"].Value);
                        ue_PFI_GrossProfitReportSale.DerSalesPerson = LowerTrim(response[i, "DerSalesPerson"].Value);
                        ue_PFI_GrossProfitReportSale.DerFiscalYear = response[i, "DerFiscalYear"].GetValue<int>(-1);

                        ue_PFI_GrossProfitReportSale.CoLine = response[i, "CoLine"].GetValue<int>(-1);
                        ue_PFI_GrossProfitReportSale.CoNum = response[i, "CoNum"].Value;
                        ue_PFI_GrossProfitReportSale.CoRelease = response[i, "CoRelease"].GetValue<int>(-1);
                        ue_PFI_GrossProfitReportSale.DerExtendedPrice = response[i, "DerExtendedPrice"].GetValue<decimal>(-1);
                        ue_PFI_GrossProfitReportSale.DerIsNonItemCoLine = response[i, "DerIsNonItemCoLine"].GetValue<bool>(false);
                        ue_PFI_GrossProfitReportSale.DerItem = response[i, "DerItem"].Value;
                        ue_PFI_GrossProfitReportSale.DerSalesmanName = response[i, "DerSalesmanName"].Value;
                        ue_PFI_GrossProfitReportSale.InvLine = response[i, "InvLine"].GetValue<long>(-1);
                        ue_PFI_GrossProfitReportSale.InvNum = response[i, "InvNum"].Value;
                        ue_PFI_GrossProfitReportSale.InvoiceDate = response[i, "InvoiceDate"].GetValue<DateTime>(DateTime.MinValue);
                        ue_PFI_GrossProfitReportSale.InvSeq = response[i, "InvSeq"].GetValue<long>(-1);
                        ue_PFI_GrossProfitReportSale.ItemProductCode = response[i, "ItemProductCode"].Value;
                        ue_PFI_GrossProfitReportSale.Price = response[i, "Price"].GetValue<decimal>(-1);
                        ue_PFI_GrossProfitReportSale.QtyInvoiced = response[i, "QtyInvoiced"].GetValue<decimal>(-1);
                        ue_PFI_GrossProfitReportSale.SalesPersonOutside = response[i, "SalesPersonOutside"].GetValue<bool>(false);
                        ue_PFI_GrossProfitReportSale.SalesPersonRefNum = response[i, "SalesPersonRefNum"].Value;
                        
                        //Want the last rowpointer found in the response so I can get the next batch.
                        lastRowPointer = response[i, "RowPointer"].Value;
                        ue_PFI_GrossProfitReportSales.Add(ue_PFI_GrossProfitReportSale);
                    }
                } while (batchSize > 0);
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("{0}{1}{2}", "ue_PFI_GrossProfitReportSales", Environment.NewLine, ex.Message)));
            }

            return ue_PFI_GrossProfitReportSales.ToArray();
        }
        public ue_PFI_FamilyCodeCategories[] ue_PFI_FamilyCodeCategories()
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            ue_PFI_FamilyCodeCategories ue_PFI_FamilyCodeCategory;
            string filter;
            string lastRowPointer = null;
            int batchSize;
            DataTable responseDT;
            string json;
            List<ue_PFI_FamilyCodeCategories> temp;
            List<ue_PFI_FamilyCodeCategories> ue_PFI_FamilyCodeCategories;

            ue_PFI_FamilyCodeCategories = new List<ue_PFI_FamilyCodeCategories>();

            try
            {   
                batchSize = 0;
                do
                {
                    if (string.IsNullOrWhiteSpace(lastRowPointer))
                    {
                        filter = $" 1 = 1 "; //SyteLine's IDO needs a valid filter or it will cause order by errors.
                    }
                    else
                    {
                        //If it isn't the first loop I want to grab the next batch by looking for RP greater than the last RP found.
                        filter = $" RowPointer > '{lastRowPointer}' ";
                    }

                    request = new LoadCollectionRequestData
                    {
                        IDOName = "ue_PFI_FamilyCodeCategories",
                        PropertyList = new PropertyList("FamilyCodeCategory, BookingInvoiceCode, RowPointer"),
                        Filter = filter,
                        OrderBy = "RowPointer", //Sorting by RowPointer so no duplicates in the result set.
                        RecordCap = 0 //It means pull back as many records as possible.
                    };

                    response = Context.Commands.LoadCollection(request);

                    batchSize = response.Items.Count;

                    //Didn't want to cast to DT then do JSON Serial/Deserial on an empty collection.
                    if (batchSize > 0)
                    {
                        //Brian Antos Method
                        //He turns the result to a DataTable then into Json then deserializes into a List object
                        responseDT = new IDODataSet(response).ToDataTable();
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(responseDT);
                        temp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ue_PFI_FamilyCodeCategories>>(json);

                        //Want the last rowpointer found in the response so I can get the next batch.
                        ue_PFI_FamilyCodeCategory = temp.LastOrDefault();

                        if (ue_PFI_FamilyCodeCategory != null)
                            lastRowPointer = ue_PFI_FamilyCodeCategory.RowPointer;

                        ue_PFI_FamilyCodeCategories.AddRange(temp);
                    }                    
                } while (batchSize > 0);
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("Method:'{0}' {1} Exception:'{2}'", "ue_PFI_FamilyCodeCategories", Environment.NewLine, ex.Message)));
            }

            return ue_PFI_FamilyCodeCategories.ToArray();
        }
        public SLCoItems[] SLCoItems()
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            SLCoItems slCoItem;
            string filter;
            string lastRowPointer = null;
            int batchSize;
            List<SLCoItems> slCoItems;

            slCoItems = new List<SLCoItems>();

            try
            {
                batchSize = 0;
                do
                {
                    if (string.IsNullOrWhiteSpace(lastRowPointer))
                    {
                        filter = $" 1 = 1 "; //SyteLine's IDO needs a valid filter or it will cause order by errors.
                    }
                    else
                    {
                        //If it isn't the first loop I want to grab the next batch by looking for RP greater than the last RP found.
                        filter = $" RowPointer > '{lastRowPointer}' ";
                    }

                    request = new LoadCollectionRequestData
                    {
                        IDOName = "ue_PFI_SLCoItems",
                        PropertyList = new PropertyList("CoNum, CoOrderDate, Item, Description, QtyOrderedConv, PriceConv, DerNetPrice" +
                                        ", CostConv, DerDomCurrCode, DerQtyReadyConv, DerQtyRsvdConv, DerQtyPickedConv, DerQtyPackedConv" +
                                        ", DerQtyShippedConv, DerQtyInvoicedConv, RowPointer, ue_ItemAllFamilyCode, ue_PFI_FamilyCodeCategory "),
                        Filter = filter,
                        OrderBy = "RowPointer", //Sorting by RowPointer so no duplicates in the result set.
                        RecordCap = 0 //It means pull back as many records as possible.
                    };

                    response = Context.Commands.LoadCollection(request);

                    batchSize = response.Items.Count;

                    for (int i = 0; i < response.Items.Count; i++)
                    {
                        slCoItem = new SLCoItems();

                        slCoItem.CoNum = response[i, "CoNum"].Value;
                        slCoItem.CoOrderDate = response[i, "CoOrderDate"].GetValue<DateTime>(new DateTime(1900,1,1));
                        slCoItem.Item = response[i, "Item"].Value;
                        slCoItem.Description = response[i, "Description"].Value;
                        slCoItem.QtyOrderedConv = response[i, "QtyOrderedConv"].GetValue<decimal>(0);
                        slCoItem.PriceConv = response[i, "PriceConv"].GetValue<decimal>(0);
                        slCoItem.DerNetPrice = response[i, "DerNetPrice"].GetValue<decimal>(0);
                        slCoItem.CostConv = response[i, "CostConv"].GetValue<decimal>(0);
                        slCoItem.DerDomCurrCode = response[i, "DerDomCurrCode"].Value;
                        slCoItem.DerQtyReadyConv = response[i, "DerQtyReadyConv"].GetValue<decimal>(0);
                        slCoItem.DerQtyRsvdConv = response[i, "DerQtyRsvdConv"].GetValue<decimal>(0);
                        slCoItem.DerQtyPickedConv = response[i, "DerQtyPickedConv"].GetValue<decimal>(0);
                        slCoItem.DerQtyPackedConv = response[i, "DerQtyPackedConv"].GetValue<decimal>(0);
                        slCoItem.DerQtyShippedConv = response[i, "DerQtyShippedConv"].GetValue<decimal>(0);
                        slCoItem.DerQtyInvoicedConv = response[i, "DerQtyInvoicedConv"].GetValue<decimal>(0);
                        slCoItem.RowPointer = response[i, "RowPointer"].Value;
                        slCoItem.ue_ItemAllFamilyCode = response[i, "ue_ItemAllFamilyCode"].Value;
                        slCoItem.ue_PFI_FamilyCodeCategory = response[i, "ue_PFI_FamilyCodeCategory"].Value;

                        lastRowPointer = slCoItem.RowPointer;
                        slCoItems.Add(slCoItem);
                    }

                } while (batchSize > 0);
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("Method:'{0}' {1} Exception:'{2}'", "SLCoItems", Environment.NewLine, ex.Message)));
            }

            return slCoItems.ToArray();
        }
        public string GetCurrentSite()
        {
            LoadCollectionResponseData response;
            LoadCollectionRequestData request;
            int batchSize;
            string site = null;

            try
            {
                request = new LoadCollectionRequestData
                {
                    IDOName = "SLParms",
                    PropertyList = new PropertyList("Site"),
                    RecordCap = 1 
                };

                response = Context.Commands.LoadCollection(request);

                batchSize = response.Items.Count;

                if (batchSize > 0)
                {
                    site = response[0, "Site"].Value;
                }
            }
            catch (Exception ex)
            {
                throw (new Exception(string.Format("Method:'{0}' {1} Exception:'{2}'", "GetCurrentSite", Environment.NewLine, ex.Message)));
            }

            return site;
        }
        private string LowerTrim(string str)
        {
            if(!string.IsNullOrWhiteSpace(str))
            {
                return str.ToLower().Trim();
            }
            else
            {
                return string.Empty;
            } 
        }
    }
}
