
//---------------------------------------------------------
//  Variables
//---------------------------------------------------------

var CargarLayoutfnReturn;

var ClaveLayout;

//---------------------------------------------------------
//  Funciones de Cargar Layout
//---------------------------------------------------------

function AssignCargarLayoutEvents(funcReturn, clave) {

    CargarLayoutfnReturn = funcReturn;

    ClaveLayout = clave;

    $("#cargarLayout").on('show.bs.modal', function () {

        var loadLayoutList = $("#LoadLayoutDescripcion").data("kendoDropDownList");

        if (!loadLayoutList) {

            $("#LoadLayoutDescripcion").kendoDropDownList({
                dataTextField: "Descripcion",
                dataValueField: "Descripcion",
                valuePrimitive: true,
                noDataTemplate: 'No hay datos...',
                change: function(e) {
                    var dataItem = this.dataItem();
                    var swtDefault = $("#LoadLayoutEsDefault").data("kendoMobileSwitch");
                    if (swtDefault) {
                        swtDefault.check(dataItem.EsDefault);
                    }
                }
            });

            loadLayoutList = $("#LoadLayoutDescripcion").data("kendoDropDownList");
        }

        var switchEsDefault = $("#LoadLayoutEsDefault").data("kendoMobileSwitch");

        if (!switchEsDefault) {

            $("#LoadLayoutEsDefault").kendoMobileSwitch({
                onLabel: "si",
                offLabel: "no"
            });
        }

        var butAceptar = $("#butLoadLayoutAceptar").data("kendoButton");

        if (!butAceptar) {

            $("#butLoadLayoutAceptar").kendoButton({
                spriteCssClass: "fa fa-check"
            });

            $("#butLoadLayoutAceptar").click(function () {
                LoadLayoutAceptar();
            });
        }

        var datos = {
            "Clave": ClaveLayout + CurrentProcess
,        };

        var result = MSExecuteOnServer('/GridLayouts/Buscar', datos);

        if (result != null) {

            if (ExistsErrorMessages(result.Errores)) {
                ShowTooltipMessages("err", result.Errores);
            }
            else {
                loadLayoutList.setDataSource(result.Datos);
            }
        }
    });
}

function LoadLayoutAceptar() {

    var loadLayoutList = $("#LoadLayoutDescripcion").data("kendoDropDownList");

    var switchEsDefault = $("#LoadLayoutEsDefault").data("kendoMobileSwitch");

    var param = {
        descrip: loadLayoutList.value(),
        esDefault: switchEsDefault.check(),
    };

    if (param.descrip.length == 0) {
        ShowTooltipError("LoadLayoutDescripcion", "Este campo no debe estar vacío");
        MensErr("Datos incorrectos, verifique el mensaje de error en cada campo");
    }
    else {
        $("#SaveLayoutDescripcion").val(param.descrip);
        CargarLayoutfnReturn(param);
        $("#cargarLayout").modal("hide");
    }
}

//---------------------------------------------------------
//  Funciones de Grabar Layout
//---------------------------------------------------------

function AssignGrabarLayoutEvents(funcReturn) {

    GrabarLayoutfnReturn = funcReturn;

    $("#grabarLayout").on('show.bs.modal', function () {

        $("#errSaveLayoutDescripcion").css("display", "none");

        var switchEsDefault = $("#SaveLayoutEsDefault").data("kendoMobileSwitch");

        if (!switchEsDefault) {

            $("#SaveLayoutEsDefault").kendoMobileSwitch({
                onLabel: "si",
                offLabel: "no"
            });
        }

        var butAceptar = $("#butSaveLayoutAceptar").data("kendoButton");

        if (!butAceptar) {

            $("#butSaveLayoutAceptar").kendoButton({
                spriteCssClass: "fa fa-check"
            });

            $("#butSaveLayoutAceptar").click(function () {
                SaveLayoutAceptar();
            });
        }
    });
}

function SaveLayoutAceptar() {

    var switchEsDefault = $("#SaveLayoutEsDefault").data("kendoMobileSwitch");

    var param = {
        descrip: $("#SaveLayoutDescripcion").val().trim(),
        esDefault: switchEsDefault.check(),
    };

    if (param.descrip.length == 0) {
        ShowTooltipError("SaveLayoutDescripcion", "Este campo no debe estar vacío");
        MensErr("Datos incorrectos, verifique el mensaje de error en cada campo");
    }
    else {
        GrabarLayoutfnReturn(param);
        $("#grabarLayout").modal("hide");
    }
}

//---------------------------------------------------------
//  Funciones de Eliminar Layouts
//---------------------------------------------------------

function AssignDeleteLayoutEvents(funcReturn) {

    DeleteLayoutfnReturn = funcReturn;

    $("#eliminarLayout").on('show.bs.modal', function () {

        DatosDeLayouts = ObtenerLayouts();

        var gridDeleteLayout = $("#gridDeleteLayout").data("kendoGrid");

        if (!gridDeleteLayout) {

            $("#gridDeleteLayout").kendoGrid({

                selectable: "row",

                columns: [
                    { field: "activo", title: " ", width: "40px", template: '<input class="chkbx" type="checkbox" #= activo ? \'checked="checked"\' : "" # />' },
                    { field: "descrip", title: "Descripción" },
                ],

            });

            $("#gridDeleteLayout .k-grid-content").on("change", "input.chkbx", function(e) {

                var grid = $("#gridDeleteLayout").data("kendoGrid");

                var dataItem = grid.dataItem($(e.target).closest("tr"));

                dataItem.set("activo", this.checked);

                var index = grid.dataSource.indexOf(dataItem);

                DatosDeLayouts[index].activo = this.checked;
            });
        }

        $("#eliminarLayout").on('shown.bs.modal', function () {

            CargarGrillaDeleteLayout(DatosDeLayouts);
        });

        var butAceptar = $("#butDeleteLayoutAceptar").data("kendoButton");

        if (!butAceptar) {

            $("#butDeleteLayoutAceptar").kendoButton({
                spriteCssClass: "fa fa-trash-alt"
            });

            $("#butDeleteLayoutAceptar").click(function () {
                DeleteLayoutAceptar();
            });
        }

    });
}

function ObtenerLayouts() {

    var datos = [];

    var param = {
        "Clave": ClaveLayout + CurrentProcess
    };

    var result = MSExecuteOnServer('/GridLayouts/Buscar', param);

    if (result != null) {

        if (result.Datos) {

            for (i = 0; i < result.Datos.length; i++) {
                datos.push({ activo: false, descrip: result.Datos[i].Descripcion });
            }
        }
    }

    return datos;
}

function GetDataSourceDeleteLayout(datos) {

    var ds = new kendo.data.DataSource({
        data: datos,
        schema: {
            model: {
                fields: {
                    activo: { type: "boolean" },
                    descrip: { type: "string", editable: false },
                }
            }
        }
    });

    return ds;
}

function CargarGrillaDeleteLayout(datos) {

    var gridDeleteLayout = $("#gridDeleteLayout").data("kendoGrid");

    if (gridDeleteLayout) {

        var ds = GetDataSourceDeleteLayout(datos);

        gridDeleteLayout.setDataSource(ds);

        gridDeleteLayout.refresh();
    }
}

function DeleteLayoutAceptar() {

    var param = {
        "Clave": ClaveLayout + CurrentProcess,
        "Layouts": []
    };

    for (i = 0; i < DatosDeLayouts.length; i++) {

        if (DatosDeLayouts[i].activo) {
            param.Layouts.push({ descrip: DatosDeLayouts[i].descrip });
        }
    }

    if (param.Layouts.length > 0) {
        DeleteLayoutfnReturn(param);
        CargarGrillaDeleteLayout([]);
        $("#eliminarLayout").modal("hide");
    }
    else {
        MensErr("Se debe seleccionar algun diseño para eliminar");
    }
}
