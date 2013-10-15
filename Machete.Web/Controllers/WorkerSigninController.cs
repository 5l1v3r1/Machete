﻿#region COPYRIGHT
// File:     WorkerSigninController.cs
// Author:   Savage Learning, LLC.
// Created:  2012/06/17 
// License:  GPL v3
// Project:  Machete.Web
// Contact:  savagelearning
// 
// Copyright 2011 Savage Learning, LLC., all rights reserved.
// 
// This source file is free software, under either the GPL v3 license or a
// BSD style license, as supplied with this software.
// 
// This source file is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
//  
// For details please refer to: 
// http://www.savagelearning.com/ 
//    or
// http://www.github.com/jcii/machete/
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Machete.Service;
using Machete.Web.Helpers;
using NLog;
using Machete.Domain;
using Machete.Data;
using Machete.Web.Models;
using System.Web.Routing;
using AutoMapper;

namespace Machete.Web.Controllers
{
   [ElmahHandleError]
    public class WorkerSigninController : MacheteController
    {
        private readonly IWorkerSigninService _serv;
        private readonly IWorkerService _wServ;
        private System.Globalization.CultureInfo CI;        
        public WorkerSigninController(IWorkerSigninService workerSigninService, IWorkerService workerService)
        {
            this._serv = workerSigninService;
            this._wServ = workerService;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            CI = (System.Globalization.CultureInfo)Session["Culture"];
        }
        //
        // GET: /WorkerSignin/
        // Initial page creation
        [Authorize(Roles = "Manager, Administrator, Check-in")]
        public ActionResult Index()
        {        
            return View();
        }
        //
        // POST: /WorkerSignin/Index -- records a signin
        [HttpPost]
        [Authorize(Roles = "Manager, Administrator, Check-in")]
        public ActionResult Index(int dwccardnum, DateTime dateforsignin)
        {
            Worker worker = _wServ.GetWorkerByNum(dwccardnum);
            if (worker == null) throw new NullReferenceException("card ID doesn't match a worker");
            var _signin = new WorkerSignin();
            // The card just swiped
            _signin.dwccardnum = dwccardnum;
            _signin.dateforsignin = dateforsignin;
            _signin.memberStatus = worker.memberStatus;
            //
            //
            _serv.CreateSignin(_signin, this.User.Identity.Name);
            //Get picture from checkin, show with next view
            Image checkin_image = _serv.getImage(dwccardnum);           
            string imageRef = "/Content/images/NO-IMAGE-AVAILABLE.jpg";
            if (checkin_image != null)
            {
                imageRef = "/Image/GetImage/" + checkin_image.ID;
            }

            return Json(new
            {
                memberExpired = worker.isExpired,
                memberInactive = worker.isInactive,
                memberSanctioned = worker.isSanctioned,
                memberExpelled = worker.isExpelled,
                imageRef = imageRef,
                expirationDate = worker.memberexpirationdate
            },
            JsonRequestBehavior.AllowGet);
        }

        //
        // Lottery post
        //
        // POST: /WorkOrder/Delete/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult lotterySignin(int lotterycardnum, string lotterysignindate, string userName)
        {
            DateTime signinDate = DateTime.MinValue;
            //var workOrder = woServ.GetWorkOrder(id);
            if (System.String.IsNullOrEmpty(lotterysignindate)) throw new ArgumentNullException("Lottery Sign-in date null or empty");
            signinDate = DateTime.Parse(lotterysignindate);

            string rtnstatus;
            var wsi = _serv.GetSignin(lotterycardnum, signinDate);
            if (wsi == null) throw new ArgumentNullException("Lottery cannot find Sign-in record to update");

            if (wsi.lottery_timestamp == null)
            {
                wsi.lottery_sequence = _serv.GetNextLotterySequence(signinDate);
                wsi.lottery_timestamp = DateTime.Now;
                _serv.Save(wsi, userName);
            }                
            rtnstatus = "OK";
            var _signin = new WorkerSignin();
            return Json(new
            {
                status = rtnstatus,
                memberID = lotterycardnum
            },
            JsonRequestBehavior.AllowGet);
        }

        [UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, Check-in")]
        public ActionResult ListDuplicate(DateTime todaysdate, string userName)
        {
            _serv.listDuplicate(todaysdate, userName);
            return Json(new
            {
                jobSuccess = true,
                status = "OK",
                date = todaysdate
            },
            JsonRequestBehavior.AllowGet);
        }
       
       
       [UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, Check-in")]
       //to do: rename this method to clearList
        public ActionResult clearLottery(int id, string userName)
        {
            _serv.clearLottery(id, userName);
            return Json(new
            {
                jobSuccess = true,
                status = "OK",
                deletedID = id
            },
            JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This method invokes IWorkerSigninService.moveDown,
        /// which moves a worker down in numerical order in the daily 
        /// ('lottery') list,
        /// and moves the proceeding (next) set member into their spot.
        /// </summary>
        /// <param name="id">The Worker ID of the person to be moved down.</param>
        /// <param name="userName">The username of the person making the request.</param>
        /// <returns>Json (bool jobSuccess, string status)</returns>
        [UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, Check-in")]
        public ActionResult moveDown(int id, string userName)
        {
            _serv.moveDown(id, userName);
            return Json(new
            {
                jobSuccess = true,
                status = "OK", 
                workerID = id
            },
            JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// This method invokes IWorkerSigninService.moveUp,
        /// which moves a worker up in numerical order in the 
        /// daily ('lottery') list,
        /// and moves the preceeding set member into their spot.
        /// </summary>
        /// <param name="id">The Worker ID of the person to be moved down.</param>
        /// <param name="userName">The username of the person making the request.</param>
        /// <returns>Json (bool jobSuccess, string status)</returns>
        [UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, Check-in")]
        public ActionResult moveUp(int id, string userName)
        {
            _serv.moveUp(id, userName);
            return Json(new
            {
                jobSuccess = true,
                status = "OK",
                workerID = id
            },
            JsonRequestBehavior.AllowGet);
        }

        // GET: /WorkerSignin/Delete/5
       /// <summary>
       /// This method deletes a signin from the master Worker Signins list for the day.
       /// </summary>
       /// <param name="id">The Worker ID of the worker.</param>
       /// <param name="userName">The user performing the action.</param>
       /// <returns>Json (bool jobSuccess, string status, int deletedID)</returns>
        [UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, Check-in")]
        public ActionResult Delete(int id, string userName)
        {
            _serv.Delete(id, userName);            
            return Json(new
            {
                jobSuccess = true,
                status = "OK",
                deletedID = id
            },
            JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "Administrator, Manager, Check-in")]
        public ActionResult AjaxHandler(jQueryDataTableParam param)
        {
            var vo = Mapper.Map<jQueryDataTableParam, viewOptions>(param);
            vo.CI = CI;
            dataTableResult<wsiView> was = _serv.GetIndexView(vo);
            //return what's left to datatables
            var result = from p in was.query select new 
            {  
                WSIID = p.ID,
                recordid = p.ID.ToString(),
                dwccardnum = p.dwccardnum,
                fullname = p.fullname,
                firstname1 = p.firstname1,
                firstname2 = p.firstname2,
                lastname1 = p.lastname1,
                lastname2 = p.lastname2, 
                dateforsignin = p.dateforsignin,
                dateforsigninstring = p.dateforsignin.ToShortDateString(),
                WAID = p.waid ?? 0,
                memberStatus = LookupCache.byID(p.memberStatus, CI.TwoLetterISOLanguageName),
                memberInactive = p.w.isInactive,
                memberSanctioned = p.w.isSanctioned,
                memberExpired = p.w.isExpired,
                memberExpelled = p.w.isExpelled,
                imageID = p.imageID,
                lotterySequence = p.lotterySequence,
                expirationDate = p.expirationDate.ToShortDateString(),
                skills = _getSkillCodes(p.englishlevel, p.skill1, p.skill2, p.skill3),
                program = p.typeOfWorkID == Worker.iDWC ? "DWC" : "HHH"
                };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = was.totalCount,
                iTotalDisplayRecords = was.filteredCount,
                aaData = result
            },
            JsonRequestBehavior.AllowGet);
        }

        //TODO: rework into model 
        private string _getSkillCodes(int eng, int? sk1, int? sk2, int? sk3)
        {
            string rtnstr = "E" + eng + " ";
            if (sk1 != null)
            {
                var lookup = LookupCache.getByID((int)sk1);
                rtnstr = rtnstr + lookup.ltrCode + lookup.level + " ";
            }
            if (sk2 != null)
            {
                var lookup = LookupCache.getByID((int)sk2);
                rtnstr = rtnstr + lookup.ltrCode + lookup.level + " ";
            }
            if (sk3 != null)
            {
                var lookup = LookupCache.getByID((int)sk3);
                rtnstr = rtnstr + lookup.ltrCode + lookup.level;
            }
            return rtnstr;
        }

    }
}
