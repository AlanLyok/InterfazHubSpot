var viewModel;

var datosIniAbmUsuariosWeb;

var OperacionABM = "";

var updateFields = false;

var updMustraQR = false;

$(document).ready(function () {

    $('#rootwizard').bootstrapWizard({
        'withVisible': false
    });

    InicializarElementos();

    CreateGridUsuariosWeb();

    CrearViewModel();

    AsignarBotones();

    InicializarCombos();

    AsignarValidaciones();

    AjustarAlto();
});

function InicializarElementos() {

    kendo.culture("es-AR");

    $("#UsaDobleFactor").kendoSwitch({
        messages: {
            checked: "Si",
            unchecked: "No"
        },
        change: function (e) {
            var value = e.checked;
            if (OperacionABM == "M" && updMustraQR) {
                if (value) {
                    $("#MuestraQR").data("kendoSwitch").enable(true);
                    viewModel.set("UsuariosWeb.MuestraQR", true)
                }
                else {
                    viewModel.set("UsuariosWeb.MuestraQR", false)
                    $("#MuestraQR").data("kendoSwitch").enable(false);
                }
            }
        }
    });

    $("#MuestraQR").kendoSwitch({
        messages: {
            checked: "Si",
            unchecked: "No"
        }
    });

    $("#PerfilId").kendoDropDownList({
        dataTextField: "Descripcion",
        dataValueField: "Id",
        valuePrimitive: true,
        noDataTemplate: 'No hay datos...',
    });

    $("#PerfilId").closest('.k-dropdown.k-widget').keydown(function (e) {
        if (e.keyCode == 46) {
            $("#PerfilId").data("kendoDropDownList").text("");
        }
    });

    $("#butAgregar").kendoButton({
        spriteCssClass: "fa fa-plus"
    });

    $("#butModificar").kendoButton({
        spriteCssClass: "fa fa-pencil"
    });

    $("#butEliminar").kendoButton({
        spriteCssClass: "fa fa-trash-alt"
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

function AsignarValidaciones() {

    AsigStrinValidNoVacio("Usuario", "El campo 'Usuario' no debe estar vacio");
    AsigStrinValidNoVacio("NombreCompleto", "El campo 'Nombre Completo' no debe estar vacio");
    AsigDropDValidNoVacio("PerfilId", "El campo 'Perfil' debe estar informado");
}

function AjustarAlto() {

    var h = window.innerHeight;

    h = h - 226;

    if (h < 190) {
        h = 190;
    }

    if (h > 850) {
        h = 850;
    }

    $("#gridIniUsuariosWeb").height(h);

    $("#gridIniUsuariosWeb").data("kendoGrid").refresh();
}

function CrearResultadosDataSource(datos) {

    var ds = new kendo.data.DataSource({
        pageSize: 50,
        data: datos,
        schema: {
            model: {
                fields: {
                    Id: { type: "number", editable: false },
                    Usuario: { type: "string", editable: false },
                    NombreCompleto: { type: "string", editable: false },
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
                    viewModel.set("recordMessage", cant.toString() + " Registros. " + mens);
                }

                if (mens.length == 0) {
                    $('#statusMessage').attr('class', '');
                }
                else {
                    $('#statusMessage').attr('class', 'ms-redlabel');
                }
            }
        }
    });

    return ds;
}

function CreateGridUsuariosWeb() {

    $("#gridIniUsuariosWeb").kendoGrid({
        columns: [
            { field: "Usuario", title: "Usuario" },
            { field: "NombreCompleto", title: "Nombre Completo" },
        ],

        scrollable: {
            virtual: true
        },

        reorderable: true,
        sortable: true,
        resizable: true,

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

        PerfilesWebCombo: [],

        UsuariosWeb: null,
    });

    kendo.bind($("#Abm"), viewModel);

}

function InicializarCombos() {

    var funcReturn = function (data) {

        if (ExistsErrorMessages(data.Errores)) {
            ShowErrorMessages(data.Errores);
        }
        else {
            datosIniAbmUsuariosWeb = data;
            AsignarCombos();
            InicializarBusquedaInicial();
        }
    }

    MSExecuteURLOnServerAsync('/UsuariosWeb/Inicializar', funcReturn, '');
}

function AsignarCombos() {

    viewModel.set("PerfilesWebCombo", datosIniAbmUsuariosWeb.Datos.PerfilesWeb);
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

    MSExecuteURLOnServerAsync('/UsuariosWeb/Buscar', funcReturn, '');
}

function AsignarBotones() {

    $("#butAgregar").click(function () {
        Agregar();
    });

    $("#butModificar").click(function () {
        Modificar();
    });

    $("#gridIniUsuariosWeb").on("dblclick", "tr.k-state-selected", function () {
        Modificar();
    });

    $("#butEliminar").click(function () {
        Eliminar();
    });

    $("#butAceptar").click(function () {
        Grabar();
    });

    $("#butCancelar").click(function () {
        Cancelar();
    });
}

function UpdateViewModel(model) {

    updateFields = false;

    if (OperacionABM == "A") {
        $("#divMuestraQR").hide();
    }
    else if (OperacionABM == "M") {
        $("#divMuestraQR").show();
    }

    if (model.UsuariosWeb.ObjectState == 0) {
        viewModel.set("isDeleteDisabled", true);
    }
    else {
        viewModel.set("isDeleteDisabled", false);
    }

    var usuariosweb = {
        "Id": model.UsuariosWeb.Id,
        "EmpresaId": model.UsuariosWeb.EmpresaId,
        "Usuario": model.UsuariosWeb.Usuario,
        "NombreCompleto": model.UsuariosWeb.NombreCompleto,
        "PerfilId": model.UsuariosWeb.PerfilId,
        "UsaDobleFactor": model.UsuariosWeb.UsaDobleFactor,
        "MuestraQR": model.UsuariosWeb.MuestraQR
    };

    viewModel.set("UsuariosWeb", usuariosweb);

    if (model.UsuariosWeb.UsaDobleFactor) {
        $("#MuestraQR").data("kendoSwitch").enable(true);
    }
    else {
        $("#MuestraQR").data("kendoSwitch").enable(false);
    }

    updateFields = true;
}

function LimpiarValidaciones() {

    $("#errUsuario").css("display", "none");
    $("#errNombreCompleto").css("display", "none");
    $("#errPassword").css("display", "none");
    $("#errPerfilId").css("display", "none");
}

function HabilitarInicio() {

    $('#rootwizard').bootstrapWizard('show', 'tab1');
}

function HabilitarAgregar() {

    var grid = $("#gridIniUsuariosWeb").data("kendoGrid");

    grid.clearSelection();

    $('#rootwizard').bootstrapWizard('show', 'tab2');
}

function HabilitarEdicion() {

    $('#rootwizard').bootstrapWizard('show', 'tab2');
}

function HabilitarCancelar() {
    viewModel.set("isFilterDisabled", false);
    viewModel.set("isAddNewDisabled", false);
    viewModel.set("isDeleteDisabled", true);
}

function Agregar() {

    var result = MSExecuteURLOnServer('/UsuariosWeb/Cancelar');

    if (result != null) {
        OperacionABM = "A";
        viewModel.set("isModifyDisabled", false);
        HabilitarAgregar();
        UpdateViewModel(result);
        LimpiarValidaciones();
    }
}

function Modificar() {

    updMustraQR = false;

    var grid = $("#gridIniUsuariosWeb").data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    if (data == null) {
        MensInfo("Se debe selecionar algun registro");
        return;
    }

    var param = {
        "Id": data.Id,
    };

    var result = MSExecuteOnServer('/UsuariosWeb/Aplicar', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            OperacionABM = "M";
            viewModel.set("isModifyDisabled", true);
            HabilitarEdicion();
            LimpiarValidaciones();
            UpdateViewModel(result);
            updMustraQR = true;
        }
    }
}

function Eliminar() {

    Confirma('¿ Confirma la eliminación de este registro ?',
               function (dialogItself) {
                   EjecutarEliminar();
                   dialogItself.close();
               });
}

function EjecutarEliminar() {

    var grid = $("#gridIniUsuariosWeb").data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    if (data == null) {
        MensInfo("Se debe selecionar algun registro");
        return;
    }

    var param = {
        "Id": data.Id,
    };

    var result = MSExecuteOnServer('/UsuariosWeb/Eliminar', param);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
            UpdateViewModel(result);
            LimpiarValidaciones();
            HabilitarInicio();
            InicializarBusquedaInicial();
        }
    }
}

function Grabar() {

    var grid = $("#gridIniUsuariosWeb").data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    var objectstate = 0;

    if (data != null) {
        objectstate = 2;
    }

    LimpiarValidaciones();

    var datos = {
        "ObjectState": objectstate,
        "Id": viewModel.get("UsuariosWeb.Id"),
        "EmpresaId": viewModel.get("UsuariosWeb.EmpresaId"),
        "Usuario": viewModel.get("UsuariosWeb.Usuario"),
        "NombreCompleto": viewModel.get("UsuariosWeb.NombreCompleto"),
        "Password": viewModel.get("UsuariosWeb.Password"),
        "PerfilId": viewModel.get("UsuariosWeb.PerfilId"),
        "UsaDobleFactor": viewModel.get("UsuariosWeb.UsaDobleFactor"),
        "MuestraQR": viewModel.get("UsuariosWeb.MuestraQR"),
    };

    var result = MSExecuteOnServer('/UsuariosWeb/Grabar', datos);

    if (result != null) {

        if (ExistsErrorMessages(result.Errores)) {
            ShowTooltipMessages("err", result.Errores);
        }
        else {
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
