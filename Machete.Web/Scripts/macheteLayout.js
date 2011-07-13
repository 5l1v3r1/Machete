﻿//////////////////////////////////////////////////////////////////
///
///
M_emp_uitab_patt = /employer/i;
M_ord_uitab_patt = /workorder/i;
M_ass_uitab_patt = /workassign/i;

var eventDebug = 1;
var eventsequence = 0;
var eventDate = new Date();
var M_employerAccordion;
var M_employerTable;
var M_employerTabs;
var M_employerCreateForm;
var M_workOrderTable;
var M_workOrderTabs;
var M_workOrderCreateForm;
var M_last_employerID_loaded = -1;
var M_last_orderID_loaded = -1;
var M_last_assignmentID_loaded = -1;
////////////////////////////////////////////////////////////////
//
// console logging function
//
function M_conlog(typeStr, event, page, idtag, msg) {
    d = new Date();
    var timelapse = (d.getTime() - eventDate.getTime());    
    if (typeStr == "EVENT") {
        eventsequence = eventsequence + 1;
    }
    debug.log("%s:%d:%fms:%s:%s:%s:%s:  %s", spaceFill(typeStr, 5),
                                 spaceFill(eventsequence, 3),
                                 spaceFill(timelapse, 7),
                                 spaceFill(event, 10),
                                 spaceFill(page, 12),
                                 spaceFill(getGlobalID(idtag), 4),
                                 idtag,
                                 msg);
    eventDate = d;
}
////////////////////////////////////////////////////////////////
//
// pad log variables
//
function spaceFill(number, width) {
    width -= number.toString().length;
    if (width > 0) {
        return new Array(width + (/\./.test(number) ? 2 : 1)).join('_') + number;
    } 
    return number;
}
////////////////////////////////////////////////////////////////
//
// set global ID, based on tag
//
function setGlobalIDs(idtag, recordid, func) {
    M_conlog("ID===", "SET", func, idtag, "BEFORE");
    if (recordid == null) return;
    if (M_emp_uitab_patt.test(idtag)) {
        M_last_employerID_loaded = recordid;
    }
    if (M_ord_uitab_patt.test(idtag)) {
        M_last_orderID_loaded = recordid;
    }
    if (M_ass_uitab_patt.test(idtag)) {
        M_last_assignmentID_loaded = recordid;
    }
    M_conlog("ID===", "SET", "", idtag, "AFTER");
}
////////////////////////////////////////////////////////////////
//
// get global ID, based on tag
//
function getGlobalID(idtag) {
    var rtnval = 0; 
    if (M_emp_uitab_patt.test(idtag)) {
        rtnval = M_last_employerID_loaded;
    }
    if (M_ord_uitab_patt.test(idtag)) {
        rtnval = M_last_orderID_loaded;
    }
    if (M_ass_uitab_patt.test(idtag)) {
        rtnval = M_last_assignmentID_loaded;
    }
    return rtnval;    
}

function jqrfyTabs(myTab, myPrefix, defaultTab) {
    if (defaultTab == null) defaultTab = 0;
    //
    // ## create the tab bar
    //http://forum.jquery.com/topic/ajaxoptions-is-null-problem
    var jTabs = $(myTab).tabs({
        selected: defaultTab,
        ajaxOptions: {
            error: function (xhr, status, index, anchor) {
                $(anchor.hash).html("Couldn't load this tab.");
            },
            data: {},
            success: function (data, textStatus) { }
        },
        ////////////////////////////////////////////////////////////////
        //
        // select event
        //
        select: function (event, ui) {
            $(ui.panel).hide();
            //
            //if ListTab, hide table and redraw it
            if ($(ui.tab).hasClass('ListTab')) {
                //
                //clear out old recordID 
                if (eventDebug) M_conlog("EVENT", "SELECT", "jqrfyTabs", ui.panel.id, "ListTab");
                setGlobalIDs(ui.panel.id, 0, "jqrfyTabs");
                //
                // hide list and redraw it                
                var myTable = $(ui.panel).find('.display');
                myTable.dataTable().fnDraw();

            } else {
                // Get record from global variable
                if (eventDebug) M_conlog("EVENT", "SELECT", "jqrfyTabs", ui.panel.id, "RecordTab");
                var hiddenID = $(ui.panel).find('.hiddenRecID').attr('value');
                setGlobalIDs(ui.panel.id, hiddenID, "jqrfyTabs");
                //
            }
        },
        ////////////////////////////////////////////////////////////////
        //
        // load event (This event doesn't happen for the list tab)
        //
        load: function (event, ui) {
            if (eventDebug) M_conlog("EVENT", "LOAD", "jqrfyTabs", ui.panel.id, $(ui.tab).attr('href'));
            // Get record from edit tab
            var hiddenID = $(ui.panel).find('.hiddenRecID').attr('value');

            // Set record ID in tabs custom attribute
            setGlobalIDs(ui.panel.id, hiddenID, "jqrfyTabs");
            $(ui.panel).fadeIn();
        },
        ////////////////////////////////////////////////////////////////
        //
        // show event
        //
        show: function (event, ui) {
            if (eventDebug) M_conlog("EVENT", "SHOW", "jqrfyTabs", ui.panel.id, $(ui.tab).attr('href'));

            if ($(ui.tab).hasClass('ListTab') || $(ui.tab).hasClass('GeneralTab')) {
                $(ui.panel).fadeIn();
            }
        },
        ////////////////////////////////////////////////////////////////
        //
        // remove  event (This event doesn't happen for the list tab)
        //
        remove: function (event) {
            if (eventDebug) M_conlog("EVENT", "REMOVE", "jqrfyTabs", event.target.id, "");
        },
        idPrefix: myPrefix,
        //template to put the ui-icon-close in the tab
        tabTemplate: "<li><a href='#{href}'>#{label}</a> <span class='ui-icon ui-icon-close'>Remove Tab</span></li>"
    });
    ///
    /// close tab event
    ///
    $(jTabs).find("span.ui-icon-close").live("click", function (e) {
        var trgTabnav = $(e.target).closest('.ui-tabs');
        var index = trgTabnav.children('.ui-tabs-nav').index($(this).parent());
        trgTabnav.tabs("remove", index);
        trgTabnav.tabs("select", 0);            //select list tab
    });
    return jTabs;
}
//////////////////////////////////////////////////////////////////
///
/// Add_rectab to tabbar
///
function add_rectab(theref, label, tabObj, exclusiveTab, recID) {
    //search for tab label--if it's already open, select instead of adding duplcate (datatables error)
    //TODO: de-duplicate based on ID, not label (label can change)

    var foundtab = $(tabObj).children('.ui-tabs-nav').find('li').find('a[recordID=' + recID + ']');
    var tabsize = foundtab.size();
    if (tabsize > 0) {
        var index1 = $("li", tabObj).index($(foundtab).parent());
        if (eventDebug) M_conlog("", "SELECT TAB", "add_rectab", "", label);
        tabObj.tabs("select", index1);
        setGlobalIDs(tabObj.id, recID, "add_rectab");
        return;
    }
    if (exclusiveTab) {  //>0==inside an accordion
        
        //$(myAccordion).accordion("activate", false);
        //Delete previous tab--only one when using for selection inside accordion
        var index2 = $(tabObj).children('.ui-tabs-nav').find('li').size() - 1;
        if (index2 > 1) {
            if (eventDebug) M_conlog("INIT+", "ADD TAB", "add_rectab", "", "exclusive on; removing tab");
            //Don't blast tab 0 or tab 1 (list and create)

            tabObj.tabs("remove", index2);
        }
    }

    // create tab on tabbar
    
    tabObj.tabs("add", theref, label);


    var tabIndex = tabObj.tabs('length');
    // select the newly created tab
    tabObj.tabs("select", tabIndex - 1);
    $(tabObj).find('.ui-tabs-selected').find('a').attr('recordID', recID);
    //setGlobalIDs(tabObj.id, recID, "add_rectab");

    //float the close icon --ui 1.8.6 is overflow: hidden on ui-icon
    $(tabObj).find("span.ui-icon-close").attr('style', 'float: right');
    //Close accordion and get to the work order

    return false;
}
//////////////////////////////////////////////////////////////////
///
///##Create dataTable
///
//function jqrfyTable(myTable, ajaxSource, colDef, rowCallback, myTab, sorting, initFilter) {
function jqrfyTable(myTable, myTab, myOptions) {
    var oTable;
    oTable = $(myTable).dataTable(myOptions).fnSetFilteringDelay(250);
    if (eventDebug) M_conlog("INIT+", "ADD TABLE","jqrfyTable", "", "");
    ////////////////////////////////////////////////////////////////
    //
    // table click event -- highlight row
    //
    $(myTable).find('tbody').click(function (event) {
        $(oTable.fnSettings().aoData).each(function () {
            $(this.nTr).removeClass('row_selected');
        });
        $(event.target.parentNode).addClass('row_selected');
    });
    ////////////////////////////////////////////////////////////////
    //
    // table doubleclick event 
    //
    $(myTable).find('tbody').dblclick(function (event) {

        var exclusiveTab = $(event.target).closest('.ui-tabs').hasClass('ExclusiveTab');
        var myTr = event.target.parentNode;
        var idPatt = /\d+\b/;
        var myID = $(myTr).attr('edittabref').match(idPatt);
        if (eventDebug) M_conlog("EVENT", "DBLCLICK", "jqrfyTable", "", "========================");
        //
        // add new tab
        //TODO: where the hell is myTab coming from? when will it get clobbered?
        add_rectab($(myTr).attr('edittabref'),
                   $(myTr).attr('edittablabel'),
                   myTab,
                   exclusiveTab,
                   myID);
    });
    return oTable;
}
//////////////////////////////////////////////////////////////////
///
///##open google map, default to casa latina origin
///
function openGoogleMap(destAddr, origAddr) {
    if (origAddr == null) {
        origAddr = "317+17th+Avenue+South,+Seattle,+WA+98144";
    }
    var myStr = "http://maps.google.com/maps?f=d&source=s_d&saddr=" + origAddr + "&daddr=" + destAddr + "&hl=en&ie=UTF8";
    window.open(myStr);
}
////////////////////////////////////////////
//
//
//
$.fn.addItems = function (data) {
    var elSel = this[0];    
    if (data == null) {
        return;
    }
    for (var i = elSel.length; i >= 0; i--) {
        elSel.remove(i);
    }
    for (var i = 0; i < data.length; i++) {
        var text = data[i].Text;
        var value = data[i].Value;
 
        var elOptNew = document.createElement('option');
        elOptNew.text = text;
        elOptNew.value = value;
        elSel.add(elOptNew, null);
    }
};

////////////////////////////////////////////
//
// parse skills drop down
//  for the create/edit assignment page. 
//  bound to change event
//
function parseSkillsDD(myForm) {
    var myDD = $(myForm).find('#skillID');
    var myWage = $(myForm).find('#hourlyWage');
    var myHour = $(myForm).find('#hours');
    //
    var myOption = $(myDD).find('option:selected');
    if ($(myOption).attr('wage') != null) { $(myWage).val($(myOption).attr('wage')); }
    if ($(myOption).attr('minHour') != null) { $(myHour).val($(myOption).attr('minHour')); }
    if ($(myOption).attr('fixedjob') == "True") {
        //\
        $(myWage).attr('disabled', 'disabled');
        $(myHour).attr('disabled', 'disabled');
    } else {
        $(myWage).removeAttr('disabled', 'disabled');
        $(myHour).removeAttr('disabled', 'disabled');
    }
}

function toggleDropDown(myDD, showVal, myRow, init) {
    //
    if ($(myDD).find(':selected').text() == showVal) {
        if (init) $(myRow).show();
        else $(myRow).fadeIn();
    } else {
        if (init) $(myRow).hide();
        else $(myRow).fadeOut();
    }
}

function dateTimeWidget(myObj) {
    $(myObj).datetimepicker({
        stepMinute: 15,
        ampm: true,
        hourMin: 7,
        hourMax: 17,
        minuteMin: 0,
        minuteMax: 45,
        minuteGrid: 15,
        hourGrid: 2
    });
}

datatable_lang_en = {
    "oPaginate": {
        "sFirst": "First page",
        "sLast": "Last page",
        "sNext": "Next page",
        "sPrevious": "Previous page" 
    },
    "sEmptyTable": "No data available in table",
    "sInfo": "Showing (_START_ to _END_) out of _TOTAL_ entries",
    "sInfoEmpty": "No entries to show",
    "sInfoFiltered": " - filtered from _MAX_ total entries",
    "sInfoPostFix": "",
    "sLengthMenu": "Show _MENU_ entries",
    "sProcessing": "Processing request...",
    "sSearch": "Search:"
};

datatable_lang_es = {
    "oPaginate": {
        "sFirst": "Primera página",
        "sLast": "Última página",
        "sNext": "Página siguiente",
        "sPrevious": "Página Anterior"
    },
    "sEmptyTable": "No hay datos disponibles en el cuadro",
    "sInfo": "Mostrando a cabo (_START_ a _END_) de _TOTAL_ entradas",
    "sInfoEmpty": "No hay entradas para mostrar",
    "sInfoFiltered": " - filtrado de _MAX_ registros",
    "sInfoPostFix": "",
    "sLengthMenu": "Mostrar _MENU_ registros",
    "sProcessing": "Procesamiento de solicitudes...",
    "sSearch": "Filtrar los registros:"
};