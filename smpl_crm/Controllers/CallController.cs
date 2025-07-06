using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using smpl_crm.Data;
using smpl_crm.Helpers;
using smpl_crm.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace smpl_crm.Controllers
{
    public class CallController : Controller
    {
        #region TrnCall/TrnCallAction CRUD Functions
        //TrnCallList Json Result called by Jquery - Datatable Library for "Read" Function
        [HttpGet]
        public JsonResult TrnCallList(jQueryDataTableParamModel param)
        {
            var crmDc = new smpl_crmDataContext();

            var trnCallList = from calls in crmDc.innosoft_trnCalls
                join customers in crmDc.innosoft_mstCustomers on calls.CustomerId equals customers.Id
                join products in crmDc.innosoft_mstProducts on calls.ProductId equals products.Id
                join callStatuses in crmDc.innosoft_mstCallStatus on calls.CallStatusId equals callStatuses.Id
                join staff in crmDc.innosoft_mstStaffs on calls.AssignedToId equals staff.Id
                let staffAssigned = staff.Name
                select new
                {
                    calls.Id,
                    calls.DateCalled,
                    StaffName = staffAssigned,
                    CustomerName = customers.Name,
                    ProductName = products.Product,
                    callStatuses.CallStatus
                };

            bool isCallDateSortable = Convert.ToBoolean(Request["bSortable_2"]);
            bool isStaffSortable = Convert.ToBoolean(Request["bSortable_3"]);
            bool isCustomerNameSortable = Convert.ToBoolean(Request["bSortable_4"]);
            bool isProductNameSortable = Convert.ToBoolean(Request["bSortable_5"]);
            bool isCallStatusSortable = Convert.ToBoolean(Request["bSortable_6"]);
            int sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

            //Initial sort for jquery datatables, Sorted By Date in Descending Order 
            var sortedTrnCallListObIdDesc = from tcall in trnCallList
                orderby tcall.DateCalled descending
                select tcall;
            //Sort Date Ascending
            var sortedTrnCallListObDateAsc = from tcall in trnCallList
                orderby (sortColumnIndex == 2 && isCallDateSortable ? tcall.DateCalled : null) ascending
                select tcall;
            //Sort Date Descending
            var sortedTrnCallListObDateDesc = from tcall in trnCallList
                orderby (sortColumnIndex == 2 && isCallDateSortable ? tcall.DateCalled : null) descending
                select tcall;
            //Sort by any field selected except for date in ascending order
            var sortedTrnCallListAsc = from tcall in trnCallList
                orderby (sortColumnIndex == 3 && isStaffSortable
                    ? tcall.StaffName
                    : sortColumnIndex == 4 && isCustomerNameSortable
                        ? tcall.CustomerName
                        : sortColumnIndex == 5 && isProductNameSortable
                            ? tcall.ProductName
                            : sortColumnIndex == 6 && isCallStatusSortable
                                ? tcall.CallStatus
                                : "") ascending
                select tcall;
            //Sort by any field selected except for date in descending order
            var sortedTrnCallListDesc = from tcall in trnCallList
                orderby (sortColumnIndex == 3 && isStaffSortable
                    ? tcall.StaffName
                    : sortColumnIndex == 4 && isCustomerNameSortable
                        ? tcall.CustomerName
                        : sortColumnIndex == 5 && isProductNameSortable
                            ? tcall.ProductName
                            : sortColumnIndex == 6 && isCallStatusSortable
                                ? tcall.CallStatus
                                : "") descending
                select tcall;

            string sortDirection = Request["sSortDir_0"];
            //Sort Evaluation
            var sortedTrnCallList = sortDirection == "asc" && sortColumnIndex == 2
                ? sortedTrnCallListObDateAsc
                : sortDirection == "desc" && sortColumnIndex == 2
                    ? sortedTrnCallListObDateDesc
                    : sortDirection == "asc" && sortColumnIndex > 2
                        ? sortedTrnCallListAsc
                        : sortDirection == "asc" && sortColumnIndex == 0
                            ? sortedTrnCallListObIdDesc
                            : sortedTrnCallListDesc;

            //Initial return value if parameters in param are empty
            var filterTrnCallList = from sTcl in sortedTrnCallList
                select new
                {
                    sTcl.Id,
                    sTcl.DateCalled,
                    sTcl.StaffName,
                    sTcl.CustomerName,
                    sTcl.ProductName,
                    sTcl.CallStatus
                };

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                bool isCallDateSearchable = Convert.ToBoolean(Request["bSearchable_2"]);
                bool isStaffSearchable = Convert.ToBoolean(Request["bSearchable_3"]);
                bool isCustomerNameSearchable = Convert.ToBoolean(Request["bSearchable_4"]);
                bool isProductNameSearchable = Convert.ToBoolean(Request["bSearchable_5"]);
                bool isCallStatusSearchable = Convert.ToBoolean(Request["bSearchable_6"]);
                //The return value if parameters in param are not empty
                filterTrnCallList = from sTcl in sortedTrnCallList
                    where
                        isCallDateSearchable && sTcl.DateCalled.ToString().ToLower().Contains(param.sSearch.ToLower()) ||
                        isStaffSearchable && sTcl.StaffName.ToLower().Contains(param.sSearch.ToLower()) ||
                        isCustomerNameSearchable && sTcl.CustomerName.ToLower().Contains(param.sSearch.ToLower()) ||
                        isProductNameSearchable && sTcl.ProductName.ToLower().Contains(param.sSearch.ToLower()) ||
                        isCallStatusSearchable && sTcl.CallStatus.ToLower().Contains(param.sSearch.ToLower())
                    select new
                    {
                        sTcl.Id,
                        sTcl.DateCalled,
                        sTcl.StaffName,
                        sTcl.CustomerName,
                        sTcl.ProductName,
                        sTcl.CallStatus
                    };
            }
            //The display format of return
            var filteredTrnCallList = from sTcl in filterTrnCallList
                select new
                {
                    sTcl.Id,
                    DateCalled = String.Format("{0:MMM d yyyy}", sTcl.DateCalled.Value),
                    sTcl.StaffName,
                    sTcl.CustomerName,
                    sTcl.ProductName,
                    sTcl.CallStatus
                };

            var displayedTrnCallList = filteredTrnCallList.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            //Value to list
            var result = displayedTrnCallList.ToList();
            //Final Return
            return Json(new
            {
                param.sEcho,
                iTotalRecords = crmDc.innosoft_trnCalls.Count(),
                iTotalDisplayRecords = filteredTrnCallList.Count(),
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        //TrnCallDetail Json Result Called by Jquery - Knockout Library for "Read" Function
        [HttpGet]
        public JsonResult TrnCallDetail(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();
            var call = new TrnCall();

            if (id == 0)
            {
                return Json(call, JsonRequestBehavior.AllowGet);
            }

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            call.Id = trnCallDetail.Id;
            call.DateCalled = DateTime.Parse(trnCallDetail.DateCalled.ToString()).ToShortDateString() == "" ? DateTime.Now.ToShortDateString() : DateTime.Parse(trnCallDetail.DateCalled.ToString()).ToShortDateString();
            call.CustomerId = Int32.Parse(trnCallDetail.CustomerId.ToString());
            call.ProductId = Int32.Parse(trnCallDetail.ProductId.ToString());
            call.Caller = trnCallDetail.Caller;
            call.Issue = trnCallDetail.Issue;
            call.CallStatusId = Int32.Parse(trnCallDetail.CallStatusId.ToString());
            call.AnsweredById = Int32.Parse(trnCallDetail.AnsweredById.ToString());
            call.AssignedtoId = Int32.Parse(trnCallDetail.AssignedToId.ToString());

            return Json(call, JsonRequestBehavior.AllowGet);
        }

        //TrnCallDetailMaxId Json Result Called by Jquery - Knockout Library for "Read" Function
        [HttpGet]
        public JsonResult TrnCallDetailMaxId()
        {
            var crmDc = new smpl_crmDataContext();

            var trnCallDetailMaxId = (from i in crmDc.innosoft_trnCalls
                                        select i.Id).Max();

            return Json(trnCallDetailMaxId, JsonRequestBehavior.AllowGet);
        }

        //TrnCallDetail Json Result called by Jquery - Knockout Library for "Create" & "Update" Function
        public JsonResult TrnCallDetailSave(TrnCall call)
        {
            using (var crmDc = new smpl_crmDataContext())
            {

                if (call.Id != 0)
                {
                    innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.Single(c => c.Id == call.Id);

                    trnCallDetail.DateCalled = DateTime.Parse(call.DateCalled);
                    trnCallDetail.CustomerId = call.CustomerId;
                    trnCallDetail.ProductId = call.ProductId;
                    trnCallDetail.Caller = call.Caller;
                    trnCallDetail.Issue = call.Issue;
                    trnCallDetail.CallStatusId = call.CallStatusId;
                    trnCallDetail.AnsweredById = call.AnsweredById;
                    trnCallDetail.AssignedToId = call.AssignedtoId;

                    crmDc.SubmitChanges();
                }
                else
                {
                    var trnCallDetail = new innosoft_trnCall
                    {
                        DateCalled = DateTime.Parse(call.DateCalled),
                        CustomerId = call.CustomerId,
                        ProductId = call.ProductId,
                        Caller = call.Caller,
                        Issue = call.Issue,
                        CallStatusId = call.CallStatusId,
                        AnsweredById = call.AnsweredById,
                        AssignedToId = call.AssignedtoId
                    };

                    crmDc.innosoft_trnCalls.InsertOnSubmit(trnCallDetail);
                    crmDc.SubmitChanges();
                }
            }

            return Json(call, JsonRequestBehavior.AllowGet);
        }

        //TrnCallDetail Json Result called by Jquery - knockout Library for "Delete" Function
        public JsonResult TrnCallListDelete(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCall = crmDc.innosoft_trnCalls.First(c => c.Id == id);
            if (trnCall != null) crmDc.innosoft_trnCalls.DeleteOnSubmit(trnCall);

            try
            {
                innosoft_trnCallAction trnCallAction = crmDc.innosoft_trnCallActions.First(c => c.CallId == id);
                if (trnCallAction != null) crmDc.innosoft_trnCallActions.DeleteOnSubmit(trnCallAction);
            }
            catch(Exception)
            {
                //Do nothing...
            }

            crmDc.SubmitChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //TrnCallActionList Json Result called by Jquery - Datatable for "Read" Function
        [HttpGet]
        public JsonResult TrnCallActionList(Int64 callId)
        {
            var crmDc = new smpl_crmDataContext();

            var callActs = from callAction in crmDc.innosoft_trnCallActions
                join staff in crmDc.innosoft_mstStaffs on callAction.ActedBy equals staff.Id
                join actionType in crmDc.innosoft_mstActionTypes on callAction.ActionTypeId equals actionType.Id
                where callAction.CallId == callId
                select new
                {
                    callAction.Id,
                    callAction.CallId,
                    ActedBy = staff.Name, //*
                    CallAction = callAction.Action, //*
                    callAction.EncodedDate,
                    callAction.TargetDate,
                    AcceptedDate = String.Format("{0:MMM d yyyy}", callAction.AcceptedDate), //*
                    actionType.ActionType, //*
                    callAction.Cost,
                    callAction.NumberOfHours,
                    callAction.Done //*
                };

            return Json(new {aaData = callActs.ToList()}, JsonRequestBehavior.AllowGet);
        }

        //TrnCallActionDetail Json Result called by Jquery - Knockout Library for "Read" Function
        [HttpGet]
        public JsonResult TrnCallActionDetail(Int64 callActionId)
        {
            var crmDc = new smpl_crmDataContext();
            var callActions = new TrnCallAction();

            try
            {
                innosoft_trnCallAction trnCallAction = crmDc.innosoft_trnCallActions.First(c => c.Id == callActionId);

                callActions.CallActionId = trnCallAction.Id;
                callActions.CallId = Int32.Parse(trnCallAction.CallId.ToString());
                callActions.ActedBy = Int32.Parse(trnCallAction.ActedBy.ToString());
                callActions.Action = trnCallAction.Action;
                callActions.EncodedDate = DateTime.Parse(trnCallAction.EncodedDate.ToString()).ToShortDateString() == "" ? DateTime.Now.ToShortDateString() : DateTime.Parse(trnCallAction.EncodedDate.ToString()).ToShortDateString();
                callActions.TargetDate = DateTime.Parse(trnCallAction.TargetDate.ToString()).ToShortDateString() == "" ? DateTime.Now.ToShortDateString() : DateTime.Parse(trnCallAction.TargetDate.ToString()).ToShortDateString();
                callActions.TargetTime = String.Format("{0:t}", trnCallAction.TargetDate) == "" ? String.Format("{0:t}", DateTime.Now) : String.Format("{0:t}", trnCallAction.TargetDate);
                callActions.AcceptedDate = DateTime.Parse(trnCallAction.AcceptedDate.ToString()).ToShortDateString() == "" ? DateTime.Now.ToShortDateString() : DateTime.Parse(trnCallAction.AcceptedDate.ToString()).ToShortDateString();
                callActions.AcceptedTime = String.Format("{0:t}", trnCallAction.AcceptedDate) == "" ? String.Format("{0:t}", DateTime.Now) : String.Format("{0:t}", trnCallAction.AcceptedDate);
                callActions.AcceptedBy = trnCallAction.AcceptedBy;
                callActions.ActionTypeId = Int32.Parse(trnCallAction.ActionTypeId.ToString());
                callActions.Cost = Decimal.Parse(trnCallAction.ActedBy.ToString());
                callActions.NumberOfHours = Decimal.Parse(trnCallAction.NumberOfHours.ToString());
                callActions.Done = Boolean.Parse(trnCallAction.Done.ToString());
            }
            catch (Exception)
            {
                callActions.CallActionId = 0;
                callActions.CallId = 0;
                callActions.ActedBy = 0;
                callActions.Action = "";
                callActions.EncodedDate = "";
                callActions.TargetDate = "";
                callActions.TargetTime = "";
                callActions.AcceptedDate = "";
                callActions.AcceptedTime = "";
                callActions.ActionTypeId = 0;
                callActions.Cost = 0;
                callActions.NumberOfHours = 0;
                callActions.Done = false;
            }
            return Json(callActions, JsonRequestBehavior.AllowGet);
        }

        //TrnCallActionDetail Json Result called by Jquery - Knockout Library for "Create" & "Update" Function
        public JsonResult TrnCallActionDetailSave(TrnCallAction callAction)
        {
            var crmDc = new smpl_crmDataContext();

            DateTime TargetDate = DateTime.Now;
            DateTime AcceptedDate = DateTime.Now;

            TimeSpan tsNumberOfHours;

            //TargetDate = DateTime.Parse(callAction.TargetDate + " " + callAction.TargetTime);
            //AcceptedDate = DateTime.Parse(callAction.AcceptedDate + " " + callAction.AcceptedTime);

            tsNumberOfHours = AcceptedDate - TargetDate;

            if (callAction.CallActionId != 0)
            {
                innosoft_trnCallAction trnCallActionDetail = crmDc.innosoft_trnCallActions.Single(c => c.Id == callAction.CallActionId);   

                trnCallActionDetail.CallId = callAction.CallId;
                trnCallActionDetail.ActedBy = callAction.ActedBy;
                trnCallActionDetail.Action = callAction.Action;
                trnCallActionDetail.EncodedDate = DateTime.Parse(callAction.EncodedDate);
                trnCallActionDetail.TargetDate = TargetDate;
                trnCallActionDetail.AcceptedDate = AcceptedDate;
                trnCallActionDetail.AcceptedBy = callAction.AcceptedBy;
                trnCallActionDetail.ActionTypeId = callAction.ActionTypeId;
                trnCallActionDetail.Cost = callAction.Cost;
                trnCallActionDetail.NumberOfHours = Decimal.Parse(tsNumberOfHours.TotalHours.ToString(CultureInfo.InvariantCulture)) ;
                trnCallActionDetail.Done = callAction.Done;

                crmDc.SubmitChanges();
            }
            else
            {
                var trnCallActionDetail = new innosoft_trnCallAction
                {
                    CallId = callAction.CallId,
                    ActedBy = callAction.ActedBy,
                    Action = callAction.Action,
                    EncodedDate = DateTime.Parse(callAction.EncodedDate),
                    TargetDate = DateTime.Parse(callAction.TargetDate + " " + callAction.TargetTime),
                    AcceptedDate = DateTime.Parse(callAction.AcceptedDate + " " + callAction.AcceptedTime),
                    AcceptedBy = callAction.AcceptedBy,
                    ActionTypeId = callAction.ActionTypeId,
                    Cost = callAction.Cost,
                    NumberOfHours = Decimal.Parse(tsNumberOfHours.TotalHours.ToString(CultureInfo.InvariantCulture)) ,
                    Done = callAction.Done
                };

                crmDc.innosoft_trnCallActions.InsertOnSubmit(trnCallActionDetail);
                crmDc.SubmitChanges();
            }

            return Json(callAction, JsonRequestBehavior.AllowGet);
        }

        //TrnCallActionDetail Json Result called by Jquery - knockout Library for "Delete" Function
        public JsonResult TrnCallActionListDelete(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCallAction trnCallAction = crmDc.innosoft_trnCallActions.First(c => c.Id == id);

            if (trnCallAction != null) crmDc.innosoft_trnCallActions.DeleteOnSubmit(trnCallAction);
            crmDc.SubmitChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Select2 Functions

        #region Select2 for TrnCallDetail

        #region Select Customer
        //Linq to JsonpResult Customer select2 Format
        public static Select2PagedResult CustomerToSelect2Format(IEnumerable<MstCustomer> customer)
        {
            var jsonCustomer = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstCustomer a in customer)
            {
                jsonCustomer.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.Name
                });
            }

            return jsonCustomer;
        }

        //Linq to JsonResult Initial Selection for Jquery - select2 ComboBox 
        public JsonResult SelectCustomerById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            MstCustomer cust = (from c in crmDc.innosoft_mstCustomers
                                where c.Id == SelectCustomerId(id)
                                select new MstCustomer
                                {
                                    Id = c.Id,
                                    Name = c.Name
                                }).Single();

            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        //Find CustomerId by querying innosoft_TrnCall SQL_To_LINQ scheme criteria by CallId
        public int SelectCustomerId(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            int custId = int.Parse(trnCallDetail.CustomerId.ToString());

            return custId;
        }

        #endregion

        //Linq to JsonpResult source query for Jquery - select2 ComboBox 
        public ActionResult SelectCustomer(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstCustomer> cust = from c in crmDc.innosoft_mstCustomers
                                           where c.Name.Contains(searchTerm ?? "")
                                           orderby c.Name
                                           select new MstCustomer
                                           {
                                               Id = c.Id,
                                               Name = c.Name
                                           };

            List<MstCustomer> filteredCust = cust.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstCustomer> customer = filteredCust;

            Select2PagedResult pagedCustomers = CustomerToSelect2Format(customer);

            return new JsonpResult
            {
                Data = pagedCustomers,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #region Select Product

        public ActionResult SelectProduct(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstProduct> prod = from p in crmDc.innosoft_mstProducts
                                          where p.Product.Contains(searchTerm ?? "")
                                          orderby p.Product
                                          select new MstProduct
                                          {
                                              Id = p.Id,
                                              Product = p.Product
                                          };

            List<MstProduct> filteredProduct = prod.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstProduct> product = filteredProduct;

            Select2PagedResult pagedProducts = ProductToSelect2Format(product);

            return new JsonpResult
            {
                Data = pagedProducts,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult ProductToSelect2Format(IEnumerable<MstProduct> product)
        {
            var jsonCustomer = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstProduct a in product)
            {
                jsonCustomer.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.Product
                });
            }

            return jsonCustomer;
        }

        public JsonResult SelectProductById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            MstProduct prod = (from c in crmDc.innosoft_mstProducts
                               where c.Id == SelectProductId(id)
                               select new MstProduct
                               {
                                   Id = c.Id,
                                   Product = c.Product
                               }).Single();

            return Json(prod, JsonRequestBehavior.AllowGet);
        }

        public int SelectProductId(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            int prodId = int.Parse(trnCallDetail.ProductId.ToString());

            return prodId;
        }

        #endregion

        #region Select Call Status

        public ActionResult SelectCallStatus(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstCallStatus> callStat = from c in crmDc.innosoft_mstCallStatus
                                                 where c.CallStatus.Contains(searchTerm ?? "")
                                                 orderby c.CallStatus
                                                 select new MstCallStatus
                                                 {
                                                     Id = c.Id,
                                                     CallStatus = c.CallStatus
                                                 };

            List<MstCallStatus> filteredCallStatus =
                callStat.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstCallStatus> callStatus = filteredCallStatus;

            Select2PagedResult pagedCallStatus = CallStatusToSelect2Format(callStatus);

            return new JsonpResult
            {
                Data = pagedCallStatus,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult CallStatusToSelect2Format(IEnumerable<MstCallStatus> callStatus)
        {
            var jsonCallStatus = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstCallStatus a in callStatus)
            {
                jsonCallStatus.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.CallStatus
                });
            }

            return jsonCallStatus;
        }

        public JsonResult SelectCallStatusById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            MstCallStatus callStat = (from c in crmDc.innosoft_mstCallStatus
                                      where c.Id == SelectCallStatusId(id)
                                      select new MstCallStatus
                                      {
                                          Id = c.Id,
                                          CallStatus = c.CallStatus
                                      }).Single();

            return Json(callStat, JsonRequestBehavior.AllowGet);
        }

        public int SelectCallStatusId(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            int callStatId = int.Parse(trnCallDetail.CallStatusId.ToString());

            return callStatId;
        }

        #endregion

        #region Select Staffs Answered By

        public ActionResult SelectAnsweredBy(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstStaff> staff = from c in crmDc.innosoft_mstStaffs
                                         where c.Name.Contains(searchTerm ?? "")
                                         orderby c.Name
                                         select new MstStaff
                                         {
                                             Id = c.Id,
                                             Name = c.Name
                                         };

            List<MstStaff> filteredStaffs = staff.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstStaff> staffs = filteredStaffs;

            Select2PagedResult pagedStaffs = StaffsAnsweredByToSelect2Format(staffs);

            return new JsonpResult
            {
                Data = pagedStaffs,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult StaffsAnsweredByToSelect2Format(IEnumerable<MstStaff> staffs)
        {
            var jsonStaffs = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstStaff a in staffs)
            {
                jsonStaffs.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.Name
                });
            }

            return jsonStaffs;
        }

        public JsonResult SelectAnsweredByById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            MstStaff callStat = (from c in crmDc.innosoft_mstStaffs
                                 where c.Id == SelectAnsweredById(id)
                                 select new MstStaff
                                 {
                                     Id = c.Id,
                                     Name = c.Name
                                 }).Single();

            return Json(callStat, JsonRequestBehavior.AllowGet);
        }

        public int SelectAnsweredById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            int answeredById = int.Parse(trnCallDetail.AnsweredById.ToString());

            return answeredById;
        }

        #endregion

        #region Select Staffs Assigned To

        public ActionResult SelectAssignedTo(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstStaff> staff = from c in crmDc.innosoft_mstStaffs
                                         where c.Name.Contains(searchTerm ?? "")
                                         orderby c.Name
                                         select new MstStaff
                                         {
                                             Id = c.Id,
                                             Name = c.Name
                                         };

            List<MstStaff> filteredStaffs = staff.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstStaff> staffs = filteredStaffs;

            Select2PagedResult pageStaffs = StaffsAssignedToToSelect2Format(staffs);

            return new JsonpResult
            {
                Data = pageStaffs,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult StaffsAssignedToToSelect2Format(IEnumerable<MstStaff> staffs)
        {
            var jsonStaffs = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstStaff a in staffs)
            {
                jsonStaffs.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.Name
                });
            }

            return jsonStaffs;
        }

        public JsonResult SelectAssignedToById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            MstStaff answeredTo = (from c in crmDc.innosoft_mstStaffs
                                   where c.Id == SelectAssignedToId(id)
                                   select new MstStaff
                                   {
                                       Id = c.Id,
                                       Name = c.Name
                                   }).Single();

            return Json(answeredTo, JsonRequestBehavior.AllowGet);
        }

        public int SelectAssignedToId(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCall trnCallDetail = crmDc.innosoft_trnCalls.First(c => c.Id == id);

            int answeredTo = int.Parse(trnCallDetail.AssignedToId.ToString());

            return answeredTo;
        }

        #endregion

        #endregion

        #region Select2 for TrnCallActionDetail

        #region Select ActedBy

        public ActionResult SelectActedBy(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstStaff> staff = from c in crmDc.innosoft_mstStaffs
                                         where c.Name.Contains(searchTerm ?? "")
                                         orderby c.Name
                                         select new MstStaff
                                         {
                                             Id = c.Id,
                                             Name = c.Name
                                         };

            List<MstStaff> filteredStaffs = staff.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList();

            List<MstStaff> staffs = filteredStaffs;

            Select2PagedResult pageStaffs = StaffsActedByToSelect2Format(staffs);

            return new JsonpResult
            {
                Data = pageStaffs,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult StaffsActedByToSelect2Format(IEnumerable<MstStaff> staffs)
        {
            var jsonStaffs = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstStaff a in staffs)
            {
                jsonStaffs.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.Name
                });
            }

            return jsonStaffs;
        }

        public JsonResult SelectActedByById(int id)
        {
            var crmDc = new smpl_crmDataContext();
            var actedBy = new MstStaff
            {
                Id = 0,
                Name = ""
            };

            if (id != 0)
            {
                actedBy = (from c in crmDc.innosoft_mstStaffs
                           where c.Id == SelectActedById(id)
                           select new MstStaff
                           {
                               Id = c.Id,
                               Name = c.Name
                           }).Single();
            }

            return Json(actedBy, JsonRequestBehavior.AllowGet);
        }

        public int SelectActedById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCallAction trnCallDetail = crmDc.innosoft_trnCallActions.First(c => c.Id == id);

            int actedBy = int.Parse(trnCallDetail.ActedBy.ToString());

            return actedBy;
        }

        #endregion

        #region Select ActionType

        public ActionResult SelectActionType(string searchTerm, int pageSize, int pageNum)
        {
            var crmDc = new smpl_crmDataContext();

            IQueryable<MstActionType> actionType = from c in crmDc.innosoft_mstActionTypes
                                                   where c.ActionType.Contains(searchTerm ?? "")
                                                   orderby c.ActionType
                                                   select new MstActionType
                                                   {
                                                       Id = c.Id,
                                                       ActionType = c.ActionType
                                                   };

            List<MstActionType> filteredActionType =
                actionType.Skip((pageNum - 1) * pageSize).Take(pageSize).ToList().ToList();

            List<MstActionType> actionTypes = filteredActionType;

            Select2PagedResult pageActionTypes = ActionTypeToSelect2Format(actionTypes);

            return new JsonpResult
            {
                Data = pageActionTypes,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static Select2PagedResult ActionTypeToSelect2Format(IEnumerable<MstActionType> actionType)
        {
            var jsonActionTypes = new Select2PagedResult { Results = new List<Select2Result>() };

            foreach (MstActionType a in actionType)
            {
                jsonActionTypes.Results.Add(new Select2Result
                {
                    id = a.Id.ToString(CultureInfo.InvariantCulture),
                    text = a.ActionType
                });
            }

            return jsonActionTypes;
        }

        public JsonResult SelectActionTypeById(int id)
        {
            var crmDc = new smpl_crmDataContext();

            var actiontType = new MstActionType
            {
                Id = 0,
                ActionType = ""
            };

            if (id != 0)
            {
                actiontType = (from c in crmDc.innosoft_mstActionTypes
                               where c.Id == SelectActionTypeId(id)
                               select new MstActionType
                               {
                                   Id = c.Id,
                                   ActionType = c.ActionType
                               }).Single();
            }

            return Json(actiontType, JsonRequestBehavior.AllowGet);
        }

        public int SelectActionTypeId(int id)
        {
            var crmDc = new smpl_crmDataContext();

            innosoft_trnCallAction trnCallActionDetail = crmDc.innosoft_trnCallActions.First(c => c.Id == id);

            int actiontType = int.Parse(trnCallActionDetail.ActionTypeId.ToString());

            return actiontType;
        }
        #endregion

        #region Select2 for Done
        public JsonResult SelectDoneByText(int id)
        {
            var crmDc = new smpl_crmDataContext();
            var done = new MstDone
            {
                Done = false.ToString()
            };
            
            if (id != 0)
            {
                innosoft_trnCallAction trnCallActionDetail = crmDc.innosoft_trnCallActions.First(c => c.Id == id);
                
                done = new MstDone
                {
                    Done = trnCallActionDetail.Done.ToString().ToLower()
                };
            }

            return Json(done, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #endregion

        #region Report Calls iTextSharp

        public ActionResult RepCallActionView(Int64 id)
        {

            var crmDc = new smpl_crmDataContext();

            var trnCallAction = (from calls in crmDc.innosoft_trnCalls
                                 join callActions in crmDc.innosoft_trnCallActions on calls.Id equals callActions.CallId
                                 join customers in crmDc.innosoft_mstCustomers on calls.CustomerId equals customers.Id
                                 join products in crmDc.innosoft_mstProducts on calls.ProductId equals products.Id
                                 join callStatuses in crmDc.innosoft_mstCallStatus on calls.CallStatusId equals callStatuses.Id
                                 join staff in crmDc.innosoft_mstStaffs on calls.AssignedToId equals staff.Id
                                 let staffAssigned = staff.Name
                                 where callActions.Id == id
                                 select new
                                 {
                                     TickedId = callActions.Id,
                                     TicketDate = callActions.EncodedDate,
                                     StaffAssigned = staffAssigned,
                                     CallAction = callActions.Action,
                                     CallNo = calls.Id,
                                     DateOfCall = calls.DateCalled,
                                     Client = customers.Name,
                                     ProductName = products.Product,
                                     CallIssue = calls.Issue,
                                     CallActionCost = callActions.Cost
                                 }).Single();

            var workStream = new MemoryStream();
            var document = new Document(PageSize.A4,40,40,40,40);

            var defaultFont = new Font(Font.FontFamily.HELVETICA, 10);
            var whiteFont = new Font(Font.FontFamily.HELVETICA, 10, 0, BaseColor.WHITE);

            PdfWriter.GetInstance(document, workStream).CloseStream = false;
            document.Open();

            string imagePath = Server.MapPath("~");

            var tableHeadings = new PdfPTable(8)
            {
                TotalWidth = 500f,
                LockedWidth = true,
                HorizontalAlignment = 0,
            };

            Image logo = Image.GetInstance(imagePath + "/logo/StreetSmart.png");

            logo.ScalePercent(12f);

            var tableHeadingsLogoCol = new PdfPCell(logo)
            {
                Colspan = 1,
                Rowspan = 3,
                Border = Rectangle.NO_BORDER
            };
            tableHeadings.AddCell(tableHeadingsLogoCol);

            var tableHeadingsTable = new PdfPTable(6);

            var tableHeadingsLogoText = new PdfPCell(new Paragraph("Streetsmarts Solution", defaultFont))
            {
                Colspan = 7,
                Border = Rectangle.NO_BORDER
            };
            tableHeadingsTable.AddCell(tableHeadingsLogoText);

            tableHeadingsLogoText = new PdfPCell(new Paragraph("347-B R. Palma St. San Roque Cebu City 6000", defaultFont))
            {
                Colspan = 7,
                Border = Rectangle.NO_BORDER
            };
            tableHeadingsTable.AddCell(tableHeadingsLogoText);

            tableHeadingsLogoText = new PdfPCell(new Paragraph("Contact No: +639351228470", defaultFont))
            {
                Colspan = 7,
                Border = Rectangle.NO_BORDER             
            };
            tableHeadingsTable.AddCell(tableHeadingsLogoText);

            var tableHeadingsTableCol = new PdfPCell(tableHeadingsTable)
            {
                Colspan = 7,
                Rowspan = 3,
                Border = Rectangle.NO_BORDER
            };

            tableHeadings.AddCell(tableHeadingsTableCol);

            var table = new PdfPTable(8)
            {
                TotalWidth = 500f, 
                LockedWidth = true, 
                HorizontalAlignment = 0
            };

            var cellCallTicketHeader = new PdfPCell(new Phrase("Call Ticket", new Font(Font.FontFamily.HELVETICA, 12)))
            {
                Colspan = 8,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table.AddCell(cellCallTicketHeader);

            table.AddCell(new Phrase("Ticket No: ", defaultFont));
            var cellTicketNo = new PdfPCell(new Phrase(trnCallAction.TickedId.ToString(CultureInfo.InvariantCulture), defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellTicketNo);

            table.AddCell(new Phrase("Call No: ", defaultFont));
            var cellCallNo = new PdfPCell(new Phrase(trnCallAction.CallNo.ToString(CultureInfo.InvariantCulture), defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellCallNo);

            table.AddCell(new Phrase("Date: ", defaultFont));
            var cellTicketDate = new PdfPCell(new Phrase(String.Format("{0:d}", trnCallAction.TicketDate), defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellTicketDate);

            table.AddCell(new Phrase("Call Date: ", defaultFont));
            var cellDateOfCall = new PdfPCell(new Phrase(String.Format("{0:d}", trnCallAction.DateOfCall), defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellDateOfCall);

            table.AddCell(new Phrase("Staff: ", defaultFont));
            var cellStaffAssigned = new PdfPCell(new Phrase(trnCallAction.StaffAssigned, defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellStaffAssigned);

            table.AddCell(new Phrase("Client: ", defaultFont));
            var cellClient = new PdfPCell(new Phrase(trnCallAction.Client, defaultFont))
            {
                Colspan = 3
            };
            table.AddCell(cellClient);

            var tableNested1 = new PdfPTable(1);

            var tableNested1Cell = new PdfPCell(new Phrase("Action: ", defaultFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            tableNested1Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            tableNested1Cell = new PdfPCell(new Phrase("1", whiteFont)) 
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            tableNested1Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            tableNested1Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            tableNested1Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested1.AddCell(tableNested1Cell);

            var cellNested1 = new PdfPCell(tableNested1);

            table.AddCell(cellNested1);

            var cellCallAction = new PdfPCell(new Phrase(trnCallAction.CallAction, defaultFont))
            {
                Colspan = 3,
                Rowspan = 6
            };

            table.AddCell(cellCallAction);

            var tableNested2 = new PdfPTable(1);

            var tableNested2Cell = new PdfPCell(new Phrase("Issue: ", defaultFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            tableNested2Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            tableNested2Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            tableNested2Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            tableNested2Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            tableNested2Cell = new PdfPCell(new Phrase("1", whiteFont))
            {
                Border = Rectangle.NO_BORDER
            };
            tableNested2.AddCell(tableNested2Cell);

            var cellNested2 = new PdfPCell(tableNested2);

            table.AddCell(cellNested2);

            var cellCallIssue = new PdfPCell(new Phrase(trnCallAction.CallIssue, defaultFont))
            {
                Colspan = 3,
                Rowspan = 6
            };

            table.AddCell(cellCallIssue);

            var cellRemarks = new PdfPCell(new Phrase("Remarks: ", defaultFont))
            {
                Colspan = 4,
                Rowspan = 2
            };
            table.AddCell(cellRemarks);

            var tableNested3 = new PdfPTable(4);
            var tableNested3Cell = new PdfPCell(new Phrase("Product: ", defaultFont));
            tableNested3.AddCell(tableNested3Cell);

            tableNested3Cell = new PdfPCell(new Phrase(trnCallAction.ProductName, defaultFont))
            {
                Colspan = 3
            };
            tableNested3.AddCell(tableNested3Cell);

            tableNested3Cell = new PdfPCell(new Phrase("Cost: ", defaultFont));
            tableNested3.AddCell(tableNested3Cell);

            tableNested3Cell = new PdfPCell(new Phrase(trnCallAction.CallActionCost.ToString(), defaultFont))
            {
                Colspan = 3
            };
            tableNested3.AddCell(tableNested3Cell);

            var cellNested3 = new PdfPCell(tableNested3)
            {
                Colspan = 4
            };

            table.AddCell(cellNested3); 

            var tableFooter = new PdfPTable(8)
            {
                TotalWidth = 500f,
                LockedWidth = true,
                HorizontalAlignment = 0
            };

            var cell = new PdfPCell(new Phrase("Date: ", defaultFont))
            {
                Colspan = 2
            };
            tableFooter.AddCell(cell);

            cell = new PdfPCell(new Phrase("Time In: ", defaultFont))
            {
                Colspan = 2
            };
            tableFooter.AddCell(cell);

            cell = new PdfPCell(new Phrase("Time Out: ", defaultFont))
            {
                Colspan = 2
            };
            tableFooter.AddCell(cell);

            cell = new PdfPCell(new Phrase("Verified By:", defaultFont))
            {
                Colspan = 2
            };
            tableFooter.AddCell(cell);

            cell = new PdfPCell(new Phrase("1",whiteFont))
            {
                Colspan = 2
            };
            tableFooter.AddCell(cell);
            tableFooter.AddCell(cell);
            tableFooter.AddCell(cell);
            tableFooter.AddCell(cell);

            document.AddTitle("Call Ticket");
            document.Add(tableHeadings);
            document.Add(table);
            document.Add(tableFooter);

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }

        #endregion

        #region TrnCall Views List and Detail

        public ActionResult TrnCallListView()
        {
            return View();
        }

        [HttpGet]
        public ActionResult TrnCallDetailView()
        {
            return View();
        }

        #endregion
    }
}