/// <reference path="http://msxrmtools.com/Xrm.Page/MSXRMTOOLS.Xrm.Page.2016.js" />


// JavaScript source code
function LoadSaveClick(econtext) {
    console.log("======================== ==============================================================");
    debugger;
    var eventArgs = econtext.getEventArgs();
    Xrm.Page.getControl().forEach(function (data, index) {
        console.log(data.getName());
    });
    if (Xrm.Page.getControl("accountContactsGrid").getGrid().getTotalRecordCount() >= 1) {
        return;
    } else {
        Xrm.Page.ui.setFormNotification("Add Atlest One Contact to save record","Error","ContactError01")
        eventArgs.preventDefault();
    }
    
    console.log("======================================================================================");
}