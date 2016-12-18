﻿#region COPYRIGHT
// File:     WorkAssignmentController.cs
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
using AutoMapper;
using Machete.Domain;
using Machete.Service;
using Machete.Web.Helpers;
using Machete.Web.ViewModel;
using System;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace Machete.Web.Controllers
{
    [ElmahHandleError]
    public class WorkAssignmentController : MacheteController
    {
        private readonly IWorkAssignmentService waServ;
        private readonly IWorkerService wkrServ;
        private readonly IWorkOrderService woServ;
        private readonly IWorkerSigninService wsiServ;
        private readonly ILookupCache lcache;
        private readonly IMapper map;
        private readonly IDefaults def;
        private System.Globalization.CultureInfo CI;
        public WorkAssignmentController(IWorkAssignmentService workAssignmentService,
            IWorkerService workerService,
            IWorkOrderService workOrderService,
            IWorkerSigninService signinService,
            ILookupCache lc,
            IDefaults def,
            IMapper map)

        {
            this.waServ = workAssignmentService;
            this.wkrServ = workerService;
            this.woServ = workOrderService;
            this.wsiServ = signinService;
            this.lcache = lc;
            this.map = map;
            this.def = def;
        }
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            CI = (System.Globalization.CultureInfo)Session["Culture"];
        }
        #region Index
        //
        // GET: /WorkAssignment/
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk, Check-in")]
        public ActionResult Index()
        {
            WorkAssignmentIndex _model = new WorkAssignmentIndex();
            //_model.todaysdate = DateTime.Today.ToShortDateString();
            _model.todaysdate = System.String.Format("{0:MM/dd/yyyy}", DateTime.Today);
            return View(_model);
        }

        #endregion

        [Authorize(Roles = "Administrator, Manager, PhoneDesk, Check-in")]
        public ActionResult AjaxHandler(jQueryDataTableParam param)
        {
            //Get all the records            
            var vo = map.Map<jQueryDataTableParam, viewOptions>(param);
            vo.CI = CI;
            dataTableResult<Service.DTO.WorkAssignmentList> was = waServ.GetIndexView(vo);
            var result = was.query
            .Select(
                e => map.Map<Service.DTO.WorkAssignmentList, ViewModel.WorkAssignmentList>(e)
            ).AsEnumerable();
            //var result = from p in was.query select new { 
            //                tabref = _getTabRef(p),
            //                tablabel = _getTabLabel(p),
            //                WOID = Convert.ToString(p.workOrderID),
            //                WAID = Convert.ToString(p.ID),
            //                recordid = Convert.ToString(p.ID),
            //                pWAID = p.getFullPseudoID(), 
            //                employername = p.workOrder.Employer.name,
            //                englishlevel = Convert.ToString(p.englishLevelID),
            //                skill =  lcache.textByID(p.skillID, CI.TwoLetterISOLanguageName),
            //                hourlywage = System.String.Format("${0:f2}", p.hourlyWage),
            //                hours = Convert.ToString(p.hours),
            //                hourRange = p.hourRange > 0 ? Convert.ToString(p.hourRange) : "",
            //                days = Convert.ToString(p.days),
            //                description = p.description, // WA description now matches WO description unless they change it, see Create method
            //                datecreated = Convert.ToString(p.datecreated),
            //                dateupdated = Convert.ToString(p.dateupdated), 
            //                updatedby = p.updatedby,
            //                dateTimeofWork = p.workOrder.dateTimeofWork.AddHours(Convert.ToDouble(WebConfigurationManager.AppSettings["TimeZoneDifferenceFromPacific"])).ToString(),
            //                timeofwork = p.workOrder.dateTimeofWork.AddHours(Convert.ToDouble(WebConfigurationManager.AppSettings["TimeZoneDifferenceFromPacific"])).ToShortTimeString(),
            //                status = p.workOrder.status.ToString(),
            //                earnings = System.String.Format("${0:f2}",p.getMinEarnings),
            //                maxEarnings = System.String.Format("${0:f2}", p.getMaxEarnings),
            //                WSIID = p.workerSigninID ?? 0,
            //                WID = p.workerAssignedID ?? 0,
            //                assignedWorker = p.workerAssigned != null ? p.workerAssigned.dwccardnum + " " + p.workerAssigned.Person.fullName() : "",
            //                //requestedList = p.workOrder.workerRequests.Select(a => a.fullNameAndID).ToArray(),
            //                asmtStatus = _getStatus(p)
            //    };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = was.totalCount,
                iTotalDisplayRecords = was.filteredCount,
                aaData = result
            },
            JsonRequestBehavior.AllowGet);
        }
        //
        // _getStatus
        //private string _getStatus(Domain.WorkAssignment asmt)
        //{
        //    if (asmt.workerAssignedID > 0 && asmt.workerSigninID > 0) // green
        //        return "completed";
        //    if (asmt.workerAssignedID == null && asmt.workOrder.status == Domain.WorkOrder.iCompleted)
        //        return "incomplete";
        //    if (asmt.workerAssignedID > 0 && asmt.workerSigninID == null && asmt.workOrder.status == Domain.WorkOrder.iCompleted)
        //        return "orphaned";
        //    if (asmt.workOrder.status == Domain.WorkOrder.iCancelled)
        //        return "cancelled";
        //    if (asmt.workOrder.status == Domain.WorkOrder.iActive) // blue
        //        return "active";
        //    return null;
        //}
        //
        // _getTabRef
        //
        //private string _getTabRef(Domain.WorkAssignment wa)
        //{
        //    return "/WorkAssignment/Edit/" + Convert.ToString(wa.ID);
        //}

        //
        // _getTabLabel
        //
        //private string _getTabLabel(Domain.WorkAssignment wa)
        //{
        //    return Machete.Web.Resources.WorkAssignments.tabprefix + wa.getFullPseudoID();
        //}            
        //
        // GET: /WorkAssignment/Create
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Create
        public ActionResult Create(int WorkOrderID, string _description)
        {
            var wa = map.Map<Domain.WorkAssignment, ViewModel.WorkAssignment>(new Domain.WorkAssignment()
            {
                active = true,
                workOrderID = WorkOrderID,
                skillID = def.getDefaultID(LCategory.skill),
                hours = def.hoursDefault,
                days = def.daysDefault,
                hourlyWage = def.hourlyWageDefault,
                description = _description
            });
            wa.def = def;
        return PartialView("Create", wa);
    }

    //
    // POST: /WorkAssignment/Create
    //
    [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Create(Domain.WorkAssignment assignment, string userName)
        {
            UpdateModel(assignment);
            assignment.workOrder = woServ.Get(assignment.workOrderID);
            assignment.incrPseudoID();
            //assignment.workOrder.waPseudoIDCounter++;
            //assignment.pseudoID = assignment.workOrder.waPseudoIDCounter;
            Domain.WorkAssignment newAssignment = waServ.Create(assignment, userName);
            var result = map.Map<Domain.WorkAssignment, ViewModel.WorkAssignment>(newAssignment);
            return Json(new
            {
                sNewRef = result.tabref,
                sNewLabel = result.tablabel,
                iNewID = result.ID
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion

        
        //
        // POST: /WorkAssignment/Edit/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Duplicate
        public ActionResult Duplicate(int id, string userName)
        {
            //
            // TODO: Move duplication functionality to the service layer
            Domain.WorkAssignment _assignment = waServ.Get(id);
            Domain.WorkAssignment duplicate = _assignment;
            duplicate.incrPseudoID();
            //duplicate.workOrder.waPseudoIDCounter++;
            //duplicate.pseudoID = duplicate.workOrder.waPseudoIDCounter;
            duplicate.workerAssigned = null;
            duplicate.workerAssignedID = null;
            duplicate.workerSiginin = null;
            duplicate.workerSigninID = null;
            var saved = waServ.Create(duplicate, userName);
            var result = map.Map<Domain.WorkAssignment, ViewModel.WorkAssignment>(saved);
            return Json(new
            {
                sNewRef = result.tabref,
                sNewLabel = result.tablabel,
                iNewID = result.ID
            },
            JsonRequestBehavior.AllowGet);

        }
        #endregion

        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        #region Assign
        public ActionResult Assign(int waid, int wsiid, string userName)
        {
            Domain.WorkerSignin signin = wsiServ.Get(wsiid);
            Domain.WorkAssignment assignment = waServ.Get(waid);
            waServ.Assign(assignment, signin, userName);

            return Json(new
            {
                jobSuccess = true
            }, JsonRequestBehavior.AllowGet);            
        }

        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        public JsonResult Unassign(int? waid, int? wsiid, string userName)
        {
            waServ.Unassign(waid, wsiid, userName);
            return Json(new
            {
                jobSuccess = true,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //
        // GET: /WorkAssignment/Edit/5
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Edit
        public ActionResult Edit(int id)
        {
            Domain.WorkAssignment wa = waServ.Get(id);
            var m = map.Map<Domain.WorkAssignment, ViewModel.WorkAssignment>(wa);
            m.def = def;
            return PartialView("Edit", m);
        }
        //
        // POST: /WorkAssignment/Edit/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Edit(int id, int? workerAssignedID, string userName)
        {
            Domain.WorkAssignment asmt = waServ.Get(id);
            //check if workerAssigned changed; if so, Unassign
            int? origWorker = asmt.workerAssignedID;
            if (workerAssignedID != origWorker)
                waServ.Unassign(asmt.ID, asmt.workerSigninID, userName);     
            //Update from HTML attributes
            UpdateModel(asmt);
            //Save will link workerAssigned to Assignment record
            // if changed from orphan assignment
            waServ.Save(asmt, userName);
                
            return Json(new { jobSuccess = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion      
        //
        //GET: /WorkAssignment/View/5
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region View
        public ActionResult View(int id)
        {
            Domain.WorkAssignment workAssignment = waServ.Get(id);
            
            return View(workAssignment);
        }
        #endregion
        #region Delete
        //
        // POST: /WorkAssignment/Delete/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public JsonResult Delete(int id, FormCollection collection, string user)
        {
            waServ.Delete(id, user);

            return Json(new
            {
                status = "OK",
                jobSuccess = true,
                deletedID = id
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion
    }


}
