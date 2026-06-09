//--------------------------------------------------
//  Variables Globales
//--------------------------------------------------

var gblFechas = ["Hoy", "Semana Actual", "Mes Actual", "Año Actual", "Mes Anterior", "Año Anterior", "Personalizado", "Vacío"]
var gblPersDesc = "Personalizado";
var gblPersIndex = 6;

//--------------------------------------------------
//  Funciones JavaScript Genericas de Mastersoft
//--------------------------------------------------

function MSGetAppName() {

    return "";
}


function MSGetHelpUrl(url) {

    return "http://186.122.148.237:9004" + url;
}


function MSGetUrl(url) {

    var urlok = url;
    var appname = MSGetAppName();

    if (appname.length > 0) {
        urlok = '/' + appname + url
    }

    return urlok;
}


function MSShowLoading(htmlloading) {

    if (htmlloading != '') {
        $(htmlloading).showLoading();
    }
}


function MSHideLoading(htmlloading) {

    if (htmlloading != '') {
        $(htmlloading).hideLoading();
    }
}


function MSExecuteOnServer(url, datos) {
    
    var respuesta = null;

    //alert(MSGetUrl(url));

    $.ajax({
        async: false,
        url: MSGetUrl(url),
        type: 'POST',
        data: kendo.stringify(datos),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            respuesta = data;
        },
        error: function (error) {
            MensErr("No se pudieron enviar los datos al servidor");
        }
    });

    return respuesta;
}


function MSExecuteOnServerAsync(url, datos, fncallback, iswait) {

    //alert(MSGetUrl(url));

    if (iswait) {
        $('#myPleaseWait').modal('show');
    }

    $.ajax({
        async: true,
        url: MSGetUrl(url),
        type: 'POST',
        data: kendo.stringify(datos),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var owait = $('#myPleaseWait');
            if (owait != null) {
                owait.modal('hide');
            }
            fncallback(data);
        },
        error: function (error) {
            var owait = $('#myPleaseWait');
            if (owait != null) {
                owait.modal('hide');
            }
            MensErr("No se pudieron enviar los datos al servidor");
            //alert(kendo.stringify(error));
        }
    });
}


function MSExecuteURLOnServer(url) {

    var respuesta = null;

    //alert(MSGetUrl(url));

    $.ajax({
        async: false,
        url: MSGetUrl(url),
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            respuesta = data;
        },
        error: function (error) {
            MensErr("No se pudieron enviar los datos al servidor");
        }
    });

    return respuesta;
}


function MSExecuteURLOnServerAsync(url, fncallback, htmlloading, timeoutMs) {

    //alert(MSGetUrl(url));

    MSShowLoading(htmlloading);

    $.ajax({
        async: true,
        url: MSGetUrl(url),
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        timeout: (timeoutMs === undefined || timeoutMs === null) ? 0 : timeoutMs,
        success: function (data) {
            MSHideLoading(htmlloading);
            fncallback(data);
        },
        error: function (error) {
            MSHideLoading(htmlloading);
            MensErr("No se pudieron enviar los datos al servidor");
            //alert(kendo.stringify(error));
        }
    });
}


function WebApiPostOnServerAsync(url, datos, fncallback, iswait) {

    if (iswait) {
        $('#myPleaseWait').modal('show');
    }

    var headers = {};
    var token = GetToken();

    if (token) {
        headers.Authorization = 'Bearer ' + token;
    }

    var param = {};

    if (datos != null) {
        param = kendo.stringify(datos);
    }
    
    $.ajax({
        async: true,
        url: MSGetUrl(url),
        type: 'POST',
        headers: headers,
        data: param,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            var owait = $('#myPleaseWait');
            if (owait != null) {
                owait.modal('hide');
            }
            fncallback(data);
        },
        error: function (error) {
            var owait = $('#myPleaseWait');
            if (owait != null) {
                owait.modal('hide');
            }
            MensErr("No se pudieron enviar los datos al servidor");
        }
    });
}


function WebApiPostOnServer(url, datos) {

    var respuesta = null;
    
    var headers = {};
    var token = GetToken();

    if (token) {
        headers.Authorization = 'Bearer ' + token;
    }

    var param = {};

    if (datos != null) {
        param = kendo.stringify(datos);
    }
    
    $.ajax({
        async: false,
        url: MSGetUrl(url),
        type: 'POST',
        headers: headers,
        data: param,
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            respuesta = data;
        },
        error: function (error) {
            MensErr("No se pudieron enviar los datos al servidor");
        }
    });

    return respuesta;
}


function GetToken() {

    var token = sessionStorage.getItem('accessToken');

    if (!token) {

        token = GetNewToken();

        if (token) {
            sessionStorage.setItem('accessToken', token);
        }
    }

    return token;
}


function GetNewToken() {

    var token = null;

    var result = MSExecuteOnServer('/Account/GetSesionStatus', null);

    if (!result) {
        return null;
    }

    var status = result.Status;

    var headers = {
        "Content-Type": "application/x-www-form-urlencoded",
        "Status" : status
    };

    var param = {
        "grant_type": "password",
    };

    var tokenUrl = MSGetUrl('/Token');

    $.ajax({
        async: false,
        type: 'POST',
        url: tokenUrl,
        headers: headers,
        data: param,
        success: function (data) {
            if (typeof (data.access_token) != 'undefined') {
                token = data.access_token;
            }
        },
    });

    return token;
}


function MensErr(mensaje) {

    let tit = 'Error !!!'; 

    let pos = mensaje.indexOf("(Suceso ID:");

    if (pos > 0) {

        let suc = mensaje.substring(pos).replace("(", "").replace(")", "");

        tit += " - " + suc; 

        mensaje = mensaje.substring(0, pos);
    }

    BootstrapDialog.show({
        title: tit,
        message: mensaje,
        draggable: true, 
        buttons: [{
            label: 'Cerrar',
            cssClass: 'k-button',
            action: function (dialogItself) {
                dialogItself.close();
            }
        }]
    });

}


function MensInfo(mensaje) {

    BootstrapDialog.show({
        title: 'Mensaje...',
        message: mensaje,
        draggable: true,
        buttons: [{
            label: 'Cerrar',
            cssClass: 'k-button',
            action: function (dialogItself) {
                dialogItself.close();
            }
        }]
    });
}


function Confirma(mensaje, fncallback) {
       
    BootstrapDialog.show({
        title: 'Confirmación...',
        message: mensaje,
        draggable: true,
        buttons: [{
            label: 'Aceptar',
            cssClass: 'k-button',
            action: fncallback
        }, {
            label: 'Cancelar',
            cssClass: 'k-button',
            action: function (dialogItself) {
                dialogItself.close();
            }
        }]
    });

}


function parseJsonDate(jsonDate) {

    if (!jsonDate) {
        return null;
    }

    var offset = new Date().getTimezoneOffset() * 60000;
    var parts = /\/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*/.exec(jsonDate);

    if (parts[2] == undefined)
        parts[2] = 0;

    if (parts[3] == undefined)
        parts[3] = 0;

    return new Date(+parts[1] + offset + parts[2] * 3600000 + parts[3] * 60000);
}


function padTo2Digits(num) {

    return num.toString().padStart(2, '0');
}
  
  
function formatDate(date) {
  
    return [
      padTo2Digits(date.getDate()),
      padTo2Digits(date.getMonth() + 1),
      date.getFullYear(),
    ].join('/');
}
  

function GridSetEnabled(enabled) {

    if (enabled) {
        $(".k-add-button").removeClass("k-state-disabled").addClass("k-grid-add");
    }
    else {
        $(".k-add-button").addClass("k-state-disabled").removeClass("k-grid-add");
    }
}



function ExistsErrorMessages(arrayDeErrores) {

    if (arrayDeErrores.length == 0) {
        return false;
    }
    else if (arrayDeErrores.length == 1) {

        if (arrayDeErrores[0].Source == "readonly") {
            return false;
        }
        else {
            return true;
        }
    }
    else {
        return true;
    }
}


function GetMessageReadOnly(arrayDeErrores) {

    var mens = "";

    if (arrayDeErrores.length == 1) {

        if (arrayDeErrores[0].Source == "readonly") {
            mens = arrayDeErrores[0].Message;
        }
    }

    return mens;
}


function ShowErrorMessages(arrayDeErrores) {

    var mensaje = "";

    for (var i = 0; i < arrayDeErrores.length; i++) {
        mensaje = mensaje + arrayDeErrores[i].Message
    }

    if (mensaje.length > 0) {
        MensErr(mensaje);
    }
}


function ShowValidationMessages(viewModel, arrayDeErrores) {

    var mensaje = "";

    for (var i = 0; i < arrayDeErrores.length; i++) {

        if (arrayDeErrores[i].Source.length > 0) {
            viewModel.set("msg" + arrayDeErrores[i].Source, arrayDeErrores[i].Message);
        }
        else {
            mensaje = arrayDeErrores[i].Message
        }
    }

    if (mensaje.length > 0) {
        MensErr(mensaje);
    }
}


function GetPeriodo(fecha) {

    var result = ""

    if (fecha) {

        var anio = fecha.getFullYear();

        var mes = fecha.getMonth() + 1;

        result = anio.toString() + right('00' + mes.toString(), 2);
    }

    return result;
}


function right(str, chr) {
    return str.slice(str.length-chr,str.length);
}


function AsigStrinValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");
        
        var valor = $("#" + id).val();
        
        if (valor.trim().length == 0) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigPeriodValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");
        
        var valor = $("#" + id).val();
        
        if (valor.trim().length == 0) {
            ShowTooltipError(id, message);
        }
        else if (valor.length != 6) {
            ShowTooltipError(id, message);
        }
        else {

            var parte1 = valor.substring(0, 4);
            var parte2 = valor.substring(4, 6);

            var anio = parseInt(parte1);
            var mes = parseInt(parte2);

            if (anio && mes) {

                if (!(anio >= 1900 && anio <= 3000 &&
                    mes >= 1 && mes <= 12)) {
                    ShowTooltipError(id, message); 
                }
            }
            else {
                ShowTooltipError(id, message); 
            }
        }
    });
}



function AsigComboValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");

        var combo = $("#" + id).data("kendoComboBox");

        var dataItem = combo.dataItem();
            
        if (typeof (dataItem) == 'undefined' || dataItem == null) {
            ShowTooltipError(id, message);
        }
        else if (typeof (dataItem.isDeleted) != 'undefined') {

            if (dataItem.isDeleted) {
                ShowTooltipError(id, "Este elemento no puede elegirse, porque fue deshabilitado");
            }
        }
    });
}


function AsigDropDValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");
        
        var dropdownlist = $("#" + id).data("kendoDropDownList");

        var dataItem = dropdownlist.dataItem();

        if (typeof (dataItem) == 'undefined' || dataItem == null) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigDatePValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");
        
        var datepicker = $("#" + id).data("kendoDatePicker");

        var valor = datepicker.value();
              
        if (valor == null) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigNumericValidNoVacio(id, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");

        var numerictextbox = $("#" + id).data("kendoNumericTextBox");

        var valor = numerictextbox.value();

        if (valor == null) {
            ShowTooltipError(id, message);
        }
        else if (valor == 0) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigNumericValidRango(id, desde, hasta, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");

        var numerictextbox = $("#" + id).data("kendoNumericTextBox");

        var valor = numerictextbox.value();

        if (valor == null) {
            ShowTooltipError(id, message);
        }
        else if (!(valor >= desde && valor <= hasta)) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigNumericValidCompa(id, signo, valor1, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");

        var numerictextbox = $("#" + id).data("kendoNumericTextBox");

        var valor = numerictextbox.value();

        if (valor == null) {
            ShowTooltipError(id, message);
        }
        else if (signo == ">" && !(valor > valor1)) {
            ShowTooltipError(id, message);
        }
        else if (signo == ">=" && !(valor >= valor1)) {
            ShowTooltipError(id, message);
        }
        else if (signo == "<" && !(valor < valor1)) {
            ShowTooltipError(id, message);
        }
        else if (signo == "<=" && !(valor <= valor1)) {
            ShowTooltipError(id, message);
        }
        else if (signo == "<>" && !(valor != valor1)) {
            ShowTooltipError(id, message);
        }
    });
}


function AsigStrinValidMinCar(id, minCar, message) {

    $("#" + id).blur(function () {

        $("#err" + id).css("display", "none");

        var valor = $("#" + id).val();

        if (valor.trim().length > 0 && valor.trim().length < minCar) {
            ShowTooltipError(id, message);
        }
    });
}


function ShowTooltipError(id, message) {
    
    $("#err" + id).css("display", "inline");

    var myTooltip = $("#err" + id).data("kendoTooltip");

    if (myTooltip == null || typeof (myTooltip) == 'undefined') {

        $("#err" + id).kendoTooltip({
            content: message,
            position: "bottom",
            showOn: "click mouseenter"
        });

        myTooltip = $("#err" + id).data("kendoTooltip");
    }
    else {
        myTooltip.options.content = message;
        myTooltip.refresh();
    }

    //myTooltip.show($("#err" + id));
}


function ShowTooltipMessages(prefix, arrayDeErrores) {

    var mensaje = "";
    var aviso = "";

    for (var i = 0; i < arrayDeErrores.length; i++) {

        if (arrayDeErrores[i].Source.length > 0 && arrayDeErrores[i].Source != "aviso") {

            $("#" + prefix + arrayDeErrores[i].Source).css("display", "inline");

            var myTooltip = $("#" + prefix + arrayDeErrores[i].Source).data("kendoTooltip");

            if (myTooltip == null || typeof (myTooltip) == 'undefined') {

                $("#" + prefix + arrayDeErrores[i].Source).kendoTooltip({
                    content: arrayDeErrores[i].Message,
                    position: "bottom",
                    showOn: "click mouseenter"
                });
            }
            else {
                myTooltip.options.content = arrayDeErrores[i].Message;
                myTooltip.refresh();
            }

        }
        else if(arrayDeErrores[i].Source == "aviso") {
            aviso = arrayDeErrores[i].Message;
        }
        else {
            mensaje = arrayDeErrores[i].Message
        }
    }

    ShowTabStyles(prefix, arrayDeErrores);
    
    if (mensaje.length > 0) {
        MensErr(mensaje);
    }
    else if (aviso.length > 0) {
        MensInfo(aviso);
    }
}


function ClearTabStyles() {

    var tab = $(".nav.nav-tabs");

    if (tab != null) {

        var lista = tab.children();

        for (i = 0; i < lista.length; i++) {
            lista[i].children[0].style.color = "";
        }
    }
}


function ShowTabStyles(prefix, arrayDeErrores) {

    var tab = $(".nav.nav-tabs");

    if (tab != null) {
    
        var lista = tab.children();

        for (i = 0; i < lista.length; i++) {

            var href = lista[i].children[0].href;

            var partes = href.split("#");

            var menuid = partes[1];

            var enco = false;

            var elementos = document.getElementById(menuid).querySelectorAll('IMG');

            for (j = 0; j < elementos.length; j++) {

                var imgid = elementos[j].id.substring(prefix.length);

                for (k = 0; k < arrayDeErrores.length; k++) {

                    var source = arrayDeErrores[k].Source;

                    if (source.length > 0) {

                        if (source.trim().toLowerCase() == imgid.trim().toLowerCase()) {
                            enco = true;
                            break;
                        }
                    }
                }

                if (enco) {
                    break;
                }
            }

            if (enco) {
                lista[i].children[0].style.color = "red";
            }
        }
    }
}


function ShowGridTabStyles(tabid, datosgrid) {

    try {
        
        var enco = false;

        for (i = 0; i < datosgrid.length; i++) {

            var msg = datosgrid[i].ItemError;

            if (msg != null) {

                if (msg.length > 0) {
                    enco = true;
                    break;
                }
            }
        }

        var tab = $(".nav.nav-tabs");

        if (tab != null) {

            var lista = tab.children();

            for (i = 0; i < lista.length; i++) {

                var href = lista[i].children[0].href;

                var partes = href.split("#");

                var menuid = partes[1];

                if (menuid == tabid) {

                    if (enco) {
                        lista[i].children[0].style.color = "red";
                    }
                    
                    break;
                }
            }
        }
    }
    catch (err) {
        
    }
}


function EmptyValue(data) {

    if (typeof (data) == 'number') {

        if (data == 0) {
            return true;
        }
        else {
            return false;
        }
    }

    if (typeof (data) == 'boolean') {
        return false;
    }
    
    if (typeof (data) == 'undefined' || data === null) {
        return true;
    }

    if (typeof (data.length) != 'undefined') {

        if (/^[\s]*$/.test(data.toString())) {
            return true;
        }

        return data.length == 0;
    }

    return false;
}


function GetGridCurrentDataItem(id) {

    var grid = $("#" + id).data("kendoGrid");

    var editRow = grid.tbody.find("tr:has(.k-edit-cell)");

    return grid.dataItem(editRow);
}


function GetFechaDesde(opcion) {

    var fecha = new Date();

    fecha.setHours(0, 0, 0, 0);

    if (opcion == "Semana Actual") {
        fecha.setDate(fecha.getDate() + (1 - fecha.getDay()));
    }
    else if (opcion == "Mes Actual") {
        fecha = new Date(fecha.getFullYear(), fecha.getMonth(), 1, 0, 0, 0, 0);
    }
    else if (opcion == "Año Actual") {
        fecha = new Date(fecha.getFullYear(), 0, 1, 0, 0, 0, 0);
    }
    else if (opcion == "Mes Anterior") {
        fecha.setMonth(fecha.getMonth() - 1, 1)
    }
    else if (opcion == "Año Anterior") {
        fecha = new Date(fecha.getFullYear() - 1, 0, 1, 0, 0, 0, 0);
    }
    else if (opcion == "Vacío") {
        fecha = null;
    }

    return fecha;
}


function GetFechaHasta(opcion) {

    var fecha = new Date();

    fecha.setHours(23, 59, 59, 999);

    if (opcion == "Semana Actual") {
        fecha.setDate(fecha.getDate() + (1 - fecha.getDay()));
        fecha.setDate(fecha.getDate() + 6);
    }
    else if (opcion == "Mes Actual") {
        fecha = new Date(fecha.getFullYear(), fecha.getMonth() + 1, 0, 23, 59, 59, 999);

    }
    else if (opcion == "Año Actual") {
        fecha = new Date(fecha.getFullYear(), 11, 31, 23, 59, 59, 999);
    }
    else if (opcion == "Mes Anterior") {
        fecha.setMonth(fecha.getMonth(), 0)
    }
    else if (opcion == "Año Anterior") {
        fecha = new Date(fecha.getFullYear() - 1, 11, 31, 23, 59, 59, 999);
    }
    else if (opcion == "Vacío") {
        fecha = null;
    }

    return fecha;
}


function GetValueDropDownList(id, campo) {

    var dataitem = $("#" + id).data("kendoDropDownList").dataItem()

    if (dataitem) {
        return dataitem[campo]
    }
    else {
        return null
    }
}


function GetValueComboBox(id, campo) {

    var dataitem = $("#" + id).data("kendoComboBox").dataItem()

    if (dataitem) {
        return dataitem[campo]
    }
    else {
        return null
    }
}


function GetValueDatePicker(id) {

    var datepicker = $("#" + id).data("kendoDatePicker");

    var value = datepicker.value();

    return value;
}


function GetValueNumericTextBox(id) {

    var numerictextbox = $("#" + id).data("kendoNumericTextBox");

    var value = numerictextbox.value();

    return value;
}


function GetValueMaskedTextBox(id) {

    var maskedtextbox = $("#" + id).data("kendoMaskedTextBox");

    var value = maskedtextbox.value();

    return value;
}


function GetValueSwitch(id) {

    var switchElem = $("#" + id).data("kendoSwitch");

    var value = switchElem.check();

    return value;
}


function GetValueNullableBool(id) {

    var valor = $("#" + id).data("kendoDropDownList").value()

    if (valor.toLowerCase() == "si") {
        return true;
    }
    else if (valor.toLowerCase() == "no") {
        return false;
    }
    else {
        return null;
    }
}


function GetDropDownValue(viewModel, identificador) {

    var value = viewModel.get(identificador);

    if (typeof (value) == 'undefined') {

        var pos = identificador.lastIndexOf(".");

        var identificador = identificador.slice(0, pos);

        value = viewModel.get(identificador);
    }

    return value;
}


function CheckComboValue(viewModel, identificador, html_id) {

    var combo = $("#" + html_id).data("kendoComboBox");

    var dataItem = combo.dataItem();

    if (typeof (dataItem) == 'undefined' || dataItem == null) {
        viewModel.set(identificador, null);
    }
}


function AsignarRangoFecha(dropdownid, index, viewModel, identificadorDesde, identificadorHasta) {

    var dropdownlist = $(dropdownid).data("kendoDropDownList");

    dropdownlist.select(index);

    var opcion = dropdownlist.value();

    if (opcion != gblPersDesc) {
        viewModel.set(identificadorDesde, GetFechaDesde(opcion));
        viewModel.set(identificadorHasta, GetFechaHasta(opcion));
    }
}


function InitMaskedDatePicker() {

    var kendo = window.kendo,
          ui = kendo.ui,
          Widget = ui.Widget,
          proxy = $.proxy,
          CHANGE = "change",
          PROGRESS = "progress",
          ERROR = "error",
          NS = ".generalInfo";

    var MaskedDatePicker = Widget.extend({
        init: function (element, options) {
            var that = this;
            Widget.fn.init.call(this, element, options);

            $(element).kendoMaskedTextBox({ mask: that.options.dateOptions.mask || "00/00/0000" })
            .kendoDatePicker({
                format: that.options.dateOptions.format || "dd/MM/yyyy",
                parseFormats: that.options.dateOptions.parseFormats || ["dd/MM/yyyy", "dd/MM/yy"]
            })
            .closest(".k-datepicker")
            .add(element)
            .removeClass("k-textbox");

            that.element.data("kendoDatePicker").bind("change", function () {
                that.trigger(CHANGE);
            });
        },
        options: {
            name: "MaskedDatePicker",
            dateOptions: {}
        },
        events: [
          CHANGE
        ],
        destroy: function () {
            var that = this;
            Widget.fn.destroy.call(that);

            kendo.destroy(that.element);
        },
        value: function (value) {
            var datepicker = this.element.data("kendoDatePicker");

            if (value === undefined) {
                return datepicker.value();
            }

            datepicker.value(value);
        }
    });

    ui.plugin(MaskedDatePicker);

}


function ValidDate(errores, id) {

    if ($("#" + id).data("kendoMaskedTextBox").value().length != 0) {

        if ($("#" + id).data("kendoDatePicker").value() == null) {
            errores.push({ Message: "Este campo debe ser una fecha valida", Source: id });
        }
    }

}



function ValidDateNoEmpty(errores, id) {

    if ($("#" + id).data("kendoMaskedTextBox").value().length == 0) {
        errores.push({ Message: "Esta fecha no debe ser vacía", Source: id });
    }
    else if ($("#" + id).data("kendoDatePicker").value() == null) {
        errores.push({ Message: "Este campo debe ser una fecha valida", Source: id });
    }
}


function ValidCombo(errores, id) {

    var combo = $("#" + id).data("kendoComboBox");

    var dataItem = combo.dataItem();

    if (typeof (dataItem) != 'undefined' && dataItem != null) {
    
        if (typeof (dataItem.isDeleted) != 'undefined') {

            if (dataItem.isDeleted) {
                errores.push({ Message: "Este elemento no puede elegirse, porque fue deshabilitado", Source: id });
            }
        }
    }
}


function ValidDropDown(errores, id) {

    var combo = $("#" + id).data("kendoDropDownList");

    var dataItem = combo.dataItem();

    if (typeof (dataItem) != 'undefined' && dataItem != null) {

        if (typeof (dataItem.isDeleted) != 'undefined') {

            if (dataItem.isDeleted) {
                errores.push({ Message: "Este elemento no puede elegirse, porque fue deshabilitado", Source: id });
            }
        }
    }
}


function AddIncorectMessage(errores) {

    errores.push({ Message: "Datos incorrectos, verifique el mensaje de error en cada campo", Source: "" });
}


function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires="+d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}


function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for(var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}


function downloadAll(urls) {
    var link = document.createElement('a');

    link.setAttribute('download', null);
    link.style.display = 'none';

    document.body.appendChild(link);

    for (var i = 0; i < urls.length; i++) {
        link.setAttribute('href', urls[i]);
        link.click();
    }

    document.body.removeChild(link);
}


function IsMobile() {

    var result = false;

    if (navigator.userAgent.match(/Android/i)) {
        result = true;
    }
    else if (navigator.userAgent.match(/BlackBerry/i)) {
        result = true;
    }
    else if (navigator.userAgent.match(/iPhone|iPad|iPod/i)) {
        result = true;
    }
    else if (navigator.userAgent.match(/iPhone|iPad|iPod/i)) {
        result = true;
    }
    else if (navigator.userAgent.match(/Opera Mini/i)) {
        result = true;
    }
    else if (navigator.userAgent.match(/IEMobile/i) || navigator.userAgent.match(/WPDesktop/i)) {
        result = true;
    }

    return result
}
