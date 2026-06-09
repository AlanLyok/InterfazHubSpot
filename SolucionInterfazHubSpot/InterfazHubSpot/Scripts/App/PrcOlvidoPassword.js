var viewModel;
var datosIniPrcActivation;

$(document).ready(function () {

    InicializarElementos();

    CrearViewModel();
});

function InicializarElementos() {

    kendo.culture("es-AR");

    $("#butAceptar").kendoButton({
        spriteCssClass: "fa fa-check"
    });

    $("#butAceptar").click(function () {
        ProcesoAceptar();
    });

}


function CrearViewModel() {
    
    var param = {
        "Usuario": $("#hiddenUsuario").val(),
        "Email": $("#hiddenEmail").val(),
        "Password": "",
        "Confirm": "",
    };

    viewModel = kendo.observable({

        Parametros: param,

    });

    kendo.bind($("#PrcActivation"), viewModel);
}

function LimpiarValidaciones() {

    $("#errPassword").css("display", "none");
    $("#errConfirm").css("display", "none");
}

function ProcesoAceptar() {

    LimpiarValidaciones();

    var param = {
        "Email": viewModel.get("Parametros.Email"),
        "Password": viewModel.get("Parametros.Password"),
        "Confirm": viewModel.get("Parametros.Confirm"),
    };

    var result = MSExecuteOnServer('/PrcOlvidoPassword/Procesar', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            var url = MSGetUrl('/');
            window.location = url;
        }
    }
}

