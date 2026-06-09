
var mstrNombreCompleto = "";
var mstrIniciales = "";
var mstrEmail = "";

var mbooUsaDobleFactor = false;
var mbooMuestraQR  = false;
var mstrQRCode = "";


$(document).ready(function () {

    document.body.style.backgroundColor = "#808080";
    
    $('#rootwizard').bootstrapWizard({
        'withVisible': false
    });
    
    InicializarElementos();

    InicializarDatos();
});


function InicializarElementos() {

    kendo.culture("es-AR");

    $("#recordar").kendoSwitch({
        messages: {
            checked: "SI",
            unchecked: "NO"
        },
    });
    
    $("#butAceptar").kendoButton({
        spriteCssClass: "fa fa-check"
    });
    
    $("#butAceptar").click(function () {
        ValidarUsuario();
    });
    
    $("#EmpresaId").kendoDropDownList({
        dataTextField: "Descrip",
        dataValueField: "CodEmpre",
        valuePrimitive: true,
        noDataTemplate: 'No hay datos...',
    });
    
    $("#recuperapsw").click(function () {
        RecuperarPassword();
    });
        
    $("#butCancelarQR").kendoButton({
        spriteCssClass: "fa fa-arrow-left"
    });

    $("#butCancelarQR").click(function () {
        Cancelar();
    });
        
    $("#butSiguiente").kendoButton({
        spriteCssClass: "fa fa-arrow-right"
    });

    $("#butSiguiente").click(function () {
        PedirPIN();
    });

    $("#butCancelar").kendoButton({
        spriteCssClass: "fa fa-arrow-left"
    });

    $("#butCancelar").click(function () {
        Cancelar();
    });
        
    $("#butLogin").kendoButton({
        spriteCssClass: "fa fa-sign-in"
    });

    $("#butLogin").click(function () {
        ValidarPIN();
    });
         
    $("#butCancelarRecupero").kendoButton({
        spriteCssClass: "fa fa-arrow-left"
    });

    $("#butCancelarRecupero").click(function () {
        Cancelar();
    });

    $("#butEviarRecupero").kendoButton({
        spriteCssClass: "fa fa-envelope"
    });

    $("#butEviarRecupero").click(function () {
        EviarRecupero();
    });
    
    $("input").bind("keydown", function(event) {
      var keycode = (event.keyCode ? event.keyCode : (event.which ? event.which : event.charCode));
      if (keycode == 13) { 
        document.getElementById('butLogin').click();
         return false;
      } else  {
         return true;
      }
    });
        
    $('#rootwizard').bootstrapWizard('show', 'tab1');
}


function InicializarDatos() {

    var funcReturn = function (data) {

        if (ExistsErrorMessages(data.Errores)) {
            ShowErrorMessages(data.Errores);
        }
        else {
            var comboEmpresa = $("#EmpresaId").data("kendoDropDownList");
            comboEmpresa.setDataSource(data.Datos.Empresas);

            if (data.Datos.Empresas.length > 0) {
                comboEmpresa.select(0);
                comboEmpresa.trigger("change");
            }
        }
    }

    MSExecuteURLOnServerAsync('/Account/Inicializar', funcReturn, '');
}


function LimpiarValidaciones() {

    $("#errUsuario").css("display", "none");
    $("#errPassword").css("display", "none");
}


function Cancelar() {

    $('#rootwizard').bootstrapWizard('show', 'tab1');
}


function ValidarUsuario() {

    LimpiarValidaciones();
    
    var param = {
        "Usuario": $("#Usuario").val(),
        "password": $("#Password").val(),
        "EmpresaId": $("#EmpresaId").val()
    };
   
    var result = MSExecuteOnServer('/Account/Validar', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {

            mstrNombreCompleto = result.NombreCompleto;
            mstrIniciales = result.Iniciales;
            mstrEmail = result.Email;
  
            mbooUsaDobleFactor = result.UsaDobleFactor;
            mbooMuestraQR  = result.MuestraQR;
            mstrQRCode = result.QRCode;

            if (mbooMuestraQR) {
                MostrarQRCode();
            }
            else if (mbooUsaDobleFactor) {
                PedirPIN();
            }
            else {
                IniciarSesion();
            }
 
        }
    }
}


function MostrarQRCode() {

    document.getElementById("QRCode").src = mstrQRCode;

    $('#rootwizard').bootstrapWizard('show', 'tab3');
}


function PedirPIN() {

    $("#sesionIniciales").text(mstrIniciales);
    $("#sesionUsuario").text(mstrNombreCompleto);
    $("#sesionEmail").text(mstrEmail);

    $('#rootwizard').bootstrapWizard('show', 'tab4');
}


function ValidarPIN() {

    LimpiarValidaciones();

    if (mbooUsaDobleFactor) {

        var pin = $("#PIN").val();

        if (!pin) {

            var errores = [];

            errores.push({ Message: "Este campo no debe quedar vacio, debe cargar el PIN que figura en su celular, sin espacios.", Source: "PIN" });

            errores.push({ Message: "Datos incorrectos, verifique el mensaje de error en cada campo", Source: "" });

            ShowTooltipMessages("err", errores);

            return;
        }
    }
    
    var param = {
        "Usuario": $("#Usuario").val(),
        "password": $("#Password").val(),
        "EmpresaId": $("#EmpresaId").val(),
        "pin": $("#PIN").val(),
    };
   
    var result = MSExecuteOnServer('/Account/ValidarPIN', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            IniciarSesion();
        }
    }
}


function IniciarSesion() {

    LimpiarValidaciones();

    var headers = {
        "Content-Type": "application/x-www-form-urlencoded",
        "CompanyId": $("#EmpresaId").val(),
    };

    var param = {
        "grant_type": "password",
        "username": $("#Usuario").val(),
        "password": $("#Password").val(),
    };

    var tokenUrl = MSGetUrl('/Token');

    $.ajax({
        type: 'POST',
        url: tokenUrl,
        headers: headers,
        data: param,
        success: function (data) {
            if (typeof (data.access_token) != 'undefined') {
                sessionStorage.setItem('accessToken', data.access_token);
                document.forms["myform"].submit();
            }
            else {
                MensErr("Esta operación no ha podido ejecutarse.");
            }
        },
        error: function (error) {
            if (typeof (error.responseJSON) != 'undefined') {
                if (typeof (error.responseJSON.error_description) != 'undefined') {
                    MensErr(error.responseJSON.error_description);
                }
                else {
                    MensErr("Esta operación no ha podido ejecutarse.");
                }
            }
            else {
                MensErr("Esta operación no ha podido ejecutarse.");
            }
        }
    });
}


function RecuperarPassword() {

    $("#EmailRecupero").val($("#Usuario").val());

    $('#rootwizard').bootstrapWizard('show', 'tab2');

}


function EviarRecupero() {
    
    $("#imgLoading").css("display", "inline");

    var funcReturn = function (data) {

        $("#imgLoading").css("display", "none");

        if (ExistsErrorMessages(data.Errores)) {
            ShowTooltipMessages("err", data.Errores);
        }
        else {
            $('#rootwizard').bootstrapWizard('show', 'tab1');
        }
    }

    var param = {
        "email": $("#EmailRecupero").val(),
    };

    MSExecuteOnServerAsync('/Account/RenovarPassword', param, funcReturn, false);
}

