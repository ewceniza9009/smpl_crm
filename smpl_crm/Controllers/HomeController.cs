using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using smpl_crm.Data;
using smpl_crm.Models;

namespace smpl_crm.Controllers
{
    public class HomeController : Controller
    {
        public JsonResult MstCustomerList(jQueryDataTableParamModel param)
        {
            var crmDc = new smpl_crmDataContext();

            var mstCustomerList = from cust in crmDc.innosoft_mstCustomers
                                  select new MstCustomer{ Id = cust.Id,  Name = cust.Name, Address = cust.Address, ContactPerson = cust.ContactPerson };

            List<MstCustomer> filteredMstCustomerList;

            if (!string.IsNullOrEmpty(param.sSearch))
            {

                var isNameSearchable = Convert.ToBoolean(Request["bSearchable_2"]);
                var isAddressSearchable = Convert.ToBoolean(Request["bSearchable_3"]);
                var isContactPersonSearchable = Convert.ToBoolean(Request["bSearchable_4"]);

                filteredMstCustomerList = mstCustomerList.Where(c => isNameSearchable && c.Name.ToLower().Contains(param.sSearch.ToLower())
                               ||
                               isAddressSearchable && c.Address.ToLower().Contains(param.sSearch.ToLower())
                               ||
                               isContactPersonSearchable && c.ContactPerson.ToLower().Contains(param.sSearch.ToLower())).ToList();
                
            }
            else
            {
                filteredMstCustomerList = mstCustomerList.ToList();
            }

            var isNameSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var isAddressSortable = Convert.ToBoolean(Request["bSortable_3"]);
            var isContactPersonSortable = Convert.ToBoolean(Request["bSortable_4"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

            Func<MstCustomer, string> orderingFunction = (c => sortColumnIndex == 2 && isNameSortable ? c.Name :
                                                           sortColumnIndex == 3 && isAddressSortable ? c.Address :
                                                           sortColumnIndex == 4 && isContactPersonSortable ? c.ContactPerson :
                                                           "");

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            filteredMstCustomerList = sortDirection == "asc" ? filteredMstCustomerList.OrderBy(orderingFunction).ToList() : filteredMstCustomerList.OrderByDescending(orderingFunction).ToList();

            var displayedMstCustomerList = filteredMstCustomerList.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedMstCustomerList 
                         select new MstCustomer { Id = c.Id, Name =  c.Name, Address = c.Address, ContactPerson =  c.ContactPerson };

            return Json(new
            {
                param.sEcho,
                iTotalRecords = mstCustomerList.Count(),
                iTotalDisplayRecords = filteredMstCustomerList.Count(),
                aaData = result
            },
            JsonRequestBehavior.AllowGet);

        }

        public JsonResult MstCustomerListDelete(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();

            var mstCustomer = crmDc.innosoft_mstCustomers.First(c => c.Id == id);

            if (mstCustomer != null) crmDc.innosoft_mstCustomers.DeleteOnSubmit(mstCustomer);
            crmDc.SubmitChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MstCustomerDetail(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();
            var cust = new MstCustomer();

            var mstCustomerDetail = crmDc.innosoft_mstCustomers.First(c => c.Id == id);

            cust.Id = mstCustomerDetail.Id;
            cust.Name = mstCustomerDetail.Name;
            cust.Address = mstCustomerDetail.Address;
            cust.ContactPerson = mstCustomerDetail.ContactPerson;
            cust.Telephone = mstCustomerDetail.Telephone;
            cust.Fax = mstCustomerDetail.Fax;

            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MstCustomerDetailSave(MstCustomer cust)
        {
            var crmDc = new smpl_crmDataContext();

            if (cust.Id != 0)
            {
                var mstCustomerDetail = crmDc.innosoft_mstCustomers.Single(c => c.Id == cust.Id);

                mstCustomerDetail.Id = cust.Id;
                mstCustomerDetail.Name = cust.Name;
                mstCustomerDetail.Address = cust.Address;
                mstCustomerDetail.ContactPerson = cust.ContactPerson;
                mstCustomerDetail.Telephone = cust.Telephone;
                mstCustomerDetail.Fax = cust.Fax;

                crmDc.SubmitChanges();
            }
            else
            {
                var mstCustomerDetail = new innosoft_mstCustomer
                {
                    Name = cust.Name,
                    Address = cust.Address,
                    ContactPerson = cust.ContactPerson,
                    Telephone = cust.Telephone,
                    Fax = cust.Fax
                };

                crmDc.innosoft_mstCustomers.InsertOnSubmit(mstCustomerDetail);
                crmDc.SubmitChanges();
            }

            return Json(cust, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult MstCustomerListView()
        {
            return View();
        }

        [Authorize]
        public ActionResult MstCustomerDetailView()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}