using iTextSharp.text;
using iTextSharp.text.pdf;
using LinqKit;
using smpl_crm.Data;
using smpl_crm.Helpers;
using smpl_crm.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace smpl_crm.Controllers
{
    public class UserController : Controller
    {
        #region MstUser/MstUserAction CRUD Functions
        [HttpGet]
        public JsonResult MstUserList(jQueryDataTableParamModel param)
        {
            var crmDc = new smpl_crmDataContext();

            var mstUserList = from user in crmDc.MstUsers
                                  select new Models.MstUser { Id = user.Id, Username = user.Username, FullName = user.FullName };

            List<Models.MstUser> filteredMstUserList;

            if (!string.IsNullOrEmpty(param.sSearch))
            {

                var isUsernameSearchable = Convert.ToBoolean(Request["bSearchable_2"]);
                var isFullNameSearchable = Convert.ToBoolean(Request["bSearchable_3"]);

                filteredMstUserList = mstUserList.Where(c => isUsernameSearchable && c.Username.ToLower().Contains(param.sSearch.ToLower()) ||
                               isFullNameSearchable && c.FullName.ToLower().Contains(param.sSearch.ToLower())).ToList();

            }
            else
            {
                filteredMstUserList = mstUserList.ToList();
            }

            var isUsernameSortable = Convert.ToBoolean(Request["bSortable_2"]);
            var isFullNameSortable = Convert.ToBoolean(Request["bSortable_3"]);
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

            Func<Models.MstUser, string> orderingFunction = (c => sortColumnIndex == 2 && isUsernameSortable ? c.Username :
                                                                  sortColumnIndex == 3 && isFullNameSortable ? c.FullName :
                                                                  "");

            var sortDirection = Request["sSortDir_0"]; // asc or desc
            filteredMstUserList = sortDirection == "asc" ? filteredMstUserList.OrderBy(orderingFunction).ToList() : filteredMstUserList.OrderByDescending(orderingFunction).ToList();

            var displayedMstUserList = filteredMstUserList.Skip(param.iDisplayStart).Take(param.iDisplayLength);
            var result = from c in displayedMstUserList
                         select new Models.MstUser { Id = c.Id, Username = c.Username, FullName = c.FullName };

            return Json(new
            {
                param.sEcho,
                iTotalRecords = mstUserList.Count(),
                iTotalDisplayRecords = filteredMstUserList.Count(),
                aaData = result
            },
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MstUserDetail(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();
            var user = new Models.MstUser();

            var mstUserDetail = crmDc.MstUsers.First(c => c.Id == id);

            user.Id = mstUserDetail.Id;
            user.Username = mstUserDetail.Username;
            user.FullName = mstUserDetail.FullName;

            return Json(user, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MstUserDetailSave(Models.MstUser user)
        {
            var crmDc = new smpl_crmDataContext();

            if (user.Id != 0)
            {
                var mstUserDetail = crmDc.MstUsers.Single(c => c.Id == user.Id);

                mstUserDetail.Id = user.Id;
                mstUserDetail.Username = user.Username;
                mstUserDetail.FullName = user.FullName;
                crmDc.SubmitChanges();
            }
            else
            {
                var mstUserDetail = new Data.MstUser
                {
                    Username = user.Username,
                    FullName = user.FullName
                };

                crmDc.MstUsers.InsertOnSubmit(mstUserDetail);
                crmDc.SubmitChanges();
            }

            return Json(user, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MstUserListDelete(Int64 id)
        {
            var crmDc = new smpl_crmDataContext();

            var mstUser = crmDc.MstUsers.First(c => c.Id == id);

            if (mstUser != null) crmDc.MstUsers.DeleteOnSubmit(mstUser);
            crmDc.SubmitChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult MstUserListView()
        {
            return View();
        }

        [Authorize]
        public ActionResult MstUserDetailView()
        {
            return View();
        }
        #endregion
    }
}