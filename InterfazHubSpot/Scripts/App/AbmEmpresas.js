var objectstate = 0;

var viewModel;

var CurrentProcess = "";

var idReg = 0;

var datosIniAbmEmpresas;

$(document).ready(function () {

    $('#rootwizard').bootstrapWizard({
        'withVisible': false
    });

    InicializarElementos();

    CreateGridEmpresas();

    CrearViewModel();

    AsignarBotones();

    InicializarCombos();

    AsignarValidaciones();

    AjustarAlto();
});

function InicializarElementos() {

    kendo.culture("es-AR");

    $("#EjercicioID").kendoNumericTextBox({
        format: "0",
        spinners: false
    });

    $("#Provinciaid").kendoDropDownList({
        dataTextField: "Descripcion",
        dataValueField: "ProvinciaID",
        valuePrimitive: true,
        noDataTemplate: 'No hay datos...',
    });

    $("#Provinciaid").closest('.k-dropdown.k-widget').keydown(function (e) {
        if (e.keyCode == 46) {
            $("#Provinciaid").data("kendoDropDownList").text("");
        }
    });

    $("#CondFiscalID").kendoDropDownList({
        dataTextField: "Descripcion",
        dataValueField: "CondFiscID",
        valuePrimitive: true,
        noDataTemplate: 'No hay datos...',
    });

    $("#CondFiscalID").closest('.k-dropdown.k-widget').keydown(function (e) {
        if (e.keyCode == 46) {
            $("#CondFiscalID").data("kendoDropDownList").text("");
        }
    });

    $("#Cuit").kendoMaskedTextBox({
        mask: "00-00000000-0",
    });
    
    $("#UsaLogo").kendoSwitch({
        messages: {
            checked: "SI",
            unchecked: "NO"
        },
        change: function (e) {
            if (e.checked) {
                $("#divFiles").show();
                $("#divLogoPreview").show();
            }
            else {
                $("#divFiles").hide();
                $("#divLogoPreview").hide();
            }
        }
    });

    $("#files").kendoUpload({
        async: {
            autoUpload: false,
            saveUrl: MSGetUrl("/Empresas/Upload"),
        },
        multiple: false,
        localization: {
            select: "Seleccionar",
            statusUploaded: "OK",
            statusUploading: "Procesando...",
            headerStatusUploading: "Procesando...",
            headerStatusUploaded: "Listo !!!",
            invalidFileExtension: "Tipo de Archivo Incorrecto",
            invalidFiles: "Archivo Incorrecto"
        },
        validation: {
            allowedExtensions: [".png", ".jpg"]
        },
        upload: function (e) {
            e.data = { ID: idReg };
        },
        success: onSuccess,
        error: onError
    });

    $("#butModificar").kendoButton({
        spriteCssClass: "fa fa-pencil"
    });

    $("#butAceptar").kendoButton({
        spriteCssClass: "fa fa-check"
    });

    $("#butCancelar").kendoButton({
        spriteCssClass: "fa fa-times"
    });

    window.addEventListener("resize", function () {
        AjustarAlto();
    });

}

function onSuccess(e) {

    if (e.operation == "upload") {
        
    }
}

function onError(e) {

    if (e.operation == "upload") {
        MensErr("No se pudo subir el archivo");
    }
}

function AsignarValidaciones() {

}

function AjustarAlto() {

    var h = window.innerHeight;

    h = h - 260;

    if (h < 190) {
        h = 190;
    }

    if (h > 850) {
        h = 850;
    }

    $("#gridIniEmpresas").height(h);

    $("#gridIniEmpresas").data("kendoGrid").refresh();
}

function CrearResultadosDataSource(datos) {

    var ds = new kendo.data.DataSource({
        pageSize: 50,
        data: datos,
        schema: {
            model: {
                fields: {
                    CodEmpre: { type: "number", editable: false },
                    Descrip: { type: "string", editable: false },
                }
            }
        },
        change: function (e) {

            var mens = "";
            var tot = this.data().length;
            var cant = this._total;

            viewModel.set("recordMessage", "");

            if (tot >= 16384) {
               mens = "Es posible que existan más resultados. Ajuste los parámetros para refinar su búsqueda.";
            }

            if (tot > 0) {

                if (cant == 1) {
                  viewModel.set("recordMessage", cant.toString() + " Registro. " + mens);
                }
                else {
                  viewModel.set("recordMessage", cant.toString() + " Registros. "+ mens);
                }

                if (mens.length == 0) {
                    $('#statusMessage').attr('class', '');
                }
                else {
                    $('#statusMessage').attr('class', 'ms-redlabel');
                }
            }
        },
    });

    return ds;
}

function CreateGridEmpresas() {

    $("#gridIniEmpresas").kendoGrid({
        columns: [
            { field: "Descrip", title: "Razón Social", minResizableWidth: 120 },
        ],

        scrollable: {
            virtual: true
        },

        reorderable: true,
        resizable: true,
        sortable: true,
        selectable: "row",

        change: onChangeGridInicial,

        filterable: {
            extra: false,
            messages: {
                info: "Filtros:",
                filter: "Filtrar",
                clear: "Limpiar",
                isTrue: "SI",
                isFalse: "NO",
                and: "Y",
                or: "O"
            },
            operators: {
               string: {
                    eq: "Igual",
                    neq: "Distinto",
                    startswith: "Comienza con",
                    contains: "Contiene",
                    endswith: "Finaliza con"
               },
               date: {
                    eq: "Igual",
                    neq: "Distinto",
                    gte: "Después o igual a",
                    gt: "Después",
                    lte: "Antes o igual a",
                    lt: "Antes",
              },
               number: {
                    eq: "Igual a",
                    neq: "Distinto a",
                    gte: "Mayor que o igual a",
                    gt: "Mayor que",
                    lte: "Menor que o igual a",
                    lt: "Menor que"
              }
           }
        }
    });

    AjustarGrilla($("#gridIniEmpresas").data("kendoGrid"));

}

function AjustarGrilla(grid) {

    grid.refresh();

    if (IsMobile()) {

        var newOptions = $.extend({}, grid.getOptions());

        for (i = 0; i < newOptions.columns.length; i++) {

            if (!newOptions.columns[i].width) {
                newOptions.columns[i].width = "220px";
            }
        }

        grid.setOptions(newOptions);

        grid.refresh();
    }
}

function onChangeGridInicial() {

    var row = this.select();

    var data = this.dataItem(row);

    if (data == null) {
        viewModel.set("isDeleteDisabled", true);
    }
    else {
        viewModel.set("isDeleteDisabled", false);
    }
}

function CrearViewModel() {

    var ResultadosDataSource = CrearResultadosDataSource([]);

    viewModel = kendo.observable({

        Resultados: ResultadosDataSource,

        isReadOnly: true,
        isFilterDisabled: true,
        isControlDisabled: false,
        isModifyDisabled: false,
        isAddNewDisabled: true,
        isDeleteDisabled: true,

        ProvinciasCombo: [],
        CondFiscalCombo: [],

        Empresas: null,

    });

    kendo.bind($("#Abm"), viewModel);

}

function InicializarCombos() {

    var funcReturn = function (data) {

        if (ExistsErrorMessages(data.Errores)) {
            ShowErrorMessages(data.Errores);
        }
        else {
            datosIniAbmEmpresas = data;
            AsignarCombos();
            InicializarBusquedaInicial();
        }
    }

    MSExecuteOnServerAsync('/Empresas/Inicializar', null, funcReturn, false);
}

function AsignarCombos() {

    viewModel.set("ProvinciasCombo", datosIniAbmEmpresas.Datos.Provincias);
    viewModel.set("CondFiscalCombo", datosIniAbmEmpresas.Datos.CondFiscal);
}

function InicializarBusquedaInicial() {

    var funcReturn = function (data) {

        if (ExistsErrorMessages(data.Errores)) {
            ShowErrorMessages(data.Errores);
        }
        else {
            viewModel.set("Resultados", CrearResultadosDataSource(data.Datos));
            HabilitarCancelar();
        }
    }

    MSExecuteOnServerAsync('/Empresas/Buscar', null, funcReturn, false);
}

function AsignarBotones() {

    $("#butModificar").click(function () {
        Modificar();
    });

    $("#gridIniEmpresas").on("dblclick", "tr.k-state-selected", function () {
        Modificar();
    });

    $("#butAceptar").click(function () {
        Grabar();
    });

    $("#butCancelar").click(function () {
        Cancelar();
    });
}

function UpdateViewModel(model) {

    objectstate = model.Empresas.ObjectState;

    if (model.Empresas.ObjectState == 0) {
        viewModel.set("isDeleteDisabled", true);
    }
    else {
        viewModel.set("isDeleteDisabled", false);
    }

    var empresas = {
        "CodEmpre": model.Empresas.CodEmpre,
        "Descrip": model.Empresas.Descrip,
        "EjercicioID": model.Empresas.EjercicioID,
        "Direccion": model.Empresas.Direccion,
        "Localidad": model.Empresas.Localidad,
        "Provinciaid": model.Empresas.Provinciaid,
        "CondFiscalID": model.Empresas.CondFiscalID,
        "Cuit": model.Empresas.Cuit,
        "NroIngBrutos": model.Empresas.NroIngBrutos,
        "Telefono": model.Empresas.Telefono,
        "Mail": model.Empresas.Mail,
        "UsaLogo": model.Empresas.UsaLogo,
    };

    viewModel.set("Empresas", empresas);

    if (empresas.UsaLogo) {
        $("#divFiles").show();
        $("#divLogoPreview").show();
    }
    else {
        $("#divFiles").hide();
        $("#divLogoPreview").hide();
    }

    $("#logoPreview").attr("src", "");

    if (model.Empresas.ImagenLogo) {

        var urlLogo = MSGetUrl("/Empresas/GetFile?key=") + model.LogoKey;

        $("#logoPreview").attr("src", urlLogo);

        $("#logoPreview").show();
    }
    else {
        $("#logoPreview").hide();
    }
}

function LimpiarValidaciones() {

    $("#errDescrip").css("display", "none");
    $("#errEjercicioID").css("display", "none");
    $("#errDireccion").css("display", "none");
    $("#errLocalidad").css("display", "none");
    $("#errProvinciaid").css("display", "none");
    $("#errCondFiscalID").css("display", "none");
    $("#errCuit").css("display", "none");
    $("#errNroIngBrutos").css("display", "none");
    $("#errTelefono").css("display", "none");
    $("#errMail").css("display", "none");
}

function HabilitarInicio() {

    $('#rootwizard').bootstrapWizard('show', 'tab1');
}

function DesHabilitarTodo() {

    viewModel.set("isFilterDisabled", false);
    viewModel.set("isAddNewDisabled", true);
    viewModel.set("isDeleteDisabled", true);
}

function HabilitarEdicion() {

    var upload = $("#files").data("kendoUpload");

    upload.clearAllFiles();

    $('#rootwizard').bootstrapWizard('show', 'tab2');
}

function HabilitarCancelar() {
    viewModel.set("isFilterDisabled", false);
    viewModel.set("isAddNewDisabled", false);
    viewModel.set("isDeleteDisabled", true);
}

function SetIsModifyDisabled(disabled) {

    viewModel.set("isModifyDisabled", disabled);
}

function Modificar() {

    var grid = $("#gridIniEmpresas").data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    if (data == null) {
       MensInfo("Se debe selecionar algun registro");
       return;
    }

    var param = {
        "CodEmpre": data.CodEmpre,
        "Descrip": data.Descrip,
    };

    var result = MSExecuteOnServer('/Empresas/Aplicar', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            SetIsModifyDisabled(true);
            HabilitarEdicion();
            LimpiarValidaciones();
            UpdateViewModel(result);
        }
    }
}

function Grabar() {

    LimpiarValidaciones();

    var errores = [];

    ValidDropDown(errores, "Provinciaid");
    ValidDropDown(errores, "CondFiscalID");

    if (errores.length > 0) {
        AddIncorectMessage(errores);
        ShowTooltipMessages("err", errores);
        return;
    }

    var upload = $("#files").data("kendoUpload");

    var datos = {
        "ObjectState": objectstate,
        "CodEmpre": viewModel.get("Empresas.CodEmpre"),
        "Descrip": viewModel.get("Empresas.Descrip"),
        "EjercicioID": viewModel.get("Empresas.EjercicioID"),
        "Direccion": viewModel.get("Empresas.Direccion"),
        "Localidad": viewModel.get("Empresas.Localidad"),
        "Provinciaid": viewModel.get("Empresas.Provinciaid"),
        "CondFiscalID": viewModel.get("Empresas.CondFiscalID"),
        "Cuit": viewModel.get("Empresas.Cuit"),
        "NroIngBrutos": viewModel.get("Empresas.NroIngBrutos"),
        "Telefono": viewModel.get("Empresas.Telefono"),
        "Mail": viewModel.get("Empresas.Mail"),
        "UsaLogo": viewModel.get("Empresas.UsaLogo"),
    };

    var result = MSExecuteOnServer('/Empresas/Grabar', datos);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            idReg = datos.CodEmpre;
            upload.upload();
            UpdateViewModel(result);
            InicializarBusquedaInicial();
            MensInfo("Grabación Realizada Correctamente");
            HabilitarInicio();
        }
    }
}

function Cancelar() {

    $('#rootwizard').bootstrapWizard('show', 'tab1');
}

