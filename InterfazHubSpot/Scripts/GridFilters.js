
//---------------------------------------------------------
//  Variables
//---------------------------------------------------------

var BusFiltrosfnReturn;

var DatosDeFiltros;

//---------------------------------------------------------
//  Funciones 
//---------------------------------------------------------

function AssignBusFiltrosEvents(funcReturn, pivot) {

    BusFiltrosfnReturn = funcReturn;

    $("#busFiltros").on('show.bs.modal', function () {

        DatosDeFiltros = pivot.getAllFilterData();

        var filters = pivot.getFiltersFields();

        var ddlCampo = $("#BusFiltrosCampo").data("kendoDropDownList");

        if (ddlCampo) {
            ddlCampo.setDataSource(filters);
        }
        else {

            $("#BusFiltrosCampo").kendoDropDownList({
                dataSource: filters,
                dataTextField: "title",
                dataValueField: "name",
                valuePrimitive: true,
                noDataTemplate: 'No hay datos...',
                template: kendo.template($("#tempCamposFiltro").html()),
                change: function (e) {
                    CargarGrillaFiltros(this.value());
                }
            });
        }

        var butMarcarTodo = $("#butMarcarTodoBusFiltros").data("kendoButton");

        if (!butMarcarTodo) {

            $("#butMarcarTodoBusFiltros").kendoButton({
                spriteCssClass: "fa fa-check-square"
            });

            $("#butMarcarTodoBusFiltros").click(function () {
                MarcarTodoFiltros();
            });
        }

        var butInvertirMarcas = $("#butInvertirMarcasBusFiltros").data("kendoButton");

        if (!butInvertirMarcas) {

            $("#butInvertirMarcasBusFiltros").kendoButton({
                spriteCssClass: "fa fa-sync"
            });

            $("#butInvertirMarcasBusFiltros").click(function () {
                InvertirMarcasFiltros();
            });
        }

        var gridFiltros = $("#gridFiltros").data("kendoGrid");

        if (!gridFiltros) {

            $("#gridFiltros").kendoGrid({

                selectable: "row",

                columns: [
                    { field: "activo", title: " ", width: "40px", template: '<input class="chkbx" type="checkbox" #= activo ? \'checked="checked"\' : "" # />' },
                    { field: "descrip", title: "Descripción" },
                ],

            });

            $("#gridFiltros .k-grid-content").on("change", "input.chkbx", function (e) {

                var grid = $("#gridFiltros").data("kendoGrid");

                var dataItem = grid.dataItem($(e.target).closest("tr"));

                dataItem.set("activo", this.checked);

                var index = grid.dataSource.indexOf(dataItem);

                var ddlCampo = $("#BusFiltrosCampo").data("kendoDropDownList");

                var datos = GetFilterData(ddlCampo.value());

                datos[index].activo = this.checked;
            });
        }

        $("#busFiltros").on('shown.bs.modal', function () {

            var ddlCampo = $("#BusFiltrosCampo").data("kendoDropDownList");

            if (ddlCampo) {
                CargarGrillaFiltros(ddlCampo.value());
            }
        });

        var butAceptar = $("#butFiltrosAceptar").data("kendoButton");

        if (!butAceptar) {

            $("#butFiltrosAceptar").kendoButton({
                spriteCssClass: "fa fa-check"
            });

            $("#butFiltrosAceptar").click(function () {
                BusFiltrosAceptar();
            });
        }

    });

}


function CargarGrillaFiltros(campo) {

    var gridFiltros = $("#gridFiltros").data("kendoGrid");

    if (gridFiltros) {

        var datos = GetFilterData(campo);

        var ds = GetDataSourceFiltros(datos);

        gridFiltros.setDataSource(ds);

        gridFiltros.refresh();
    }
}


function GetFilterData(campo) {

    var datos = [];

    for (i = 0; i < DatosDeFiltros.length; i++) {

        var elem = DatosDeFiltros[i];

        if (elem.field == campo) {
            datos = elem.data;
            break;
        }
    }

    return datos;
}


function GetDataSourceFiltros(datos) {

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


function MarcarTodoFiltros() {

    var uid = null;

    var gridFiltros = $("#gridFiltros").data("kendoGrid");

    var row = gridFiltros.select();

    var data = gridFiltros.dataItem(row);

    if (data != null) {
        uid = data.uid;
    }

    var ddlCampo = $("#BusFiltrosCampo").data("kendoDropDownList");

    var result = GetFilterData(ddlCampo.value());

    for (i = 0; i < result.length; i++) {
        result[i].activo = true;
    }

    var ds = GetDataSourceFiltros(result);

    gridFiltros.setDataSource(ds);

    gridFiltros.refresh();

    if (uid != null) {

        row = gridFiltros.table.find("[data-uid=" + uid + "]");

        if (row != null) {
            gridFiltros.select(row);
        }
    }
}


function InvertirMarcasFiltros() {

    var uid = null;

    var gridFiltros = $("#gridFiltros").data("kendoGrid");

    var row = gridFiltros.select();

    var data = gridFiltros.dataItem(row);

    if (data != null) {
        uid = data.uid;
    }

    var ddlCampo = $("#BusFiltrosCampo").data("kendoDropDownList");

    var result = GetFilterData(ddlCampo.value());

    for (i = 0; i < result.length; i++) {

        if (result[i].activo) {
            result[i].activo = false;
        }
        else {
            result[i].activo = true;
        }
    }

    var ds = GetDataSourceFiltros(result);

    gridFiltros.setDataSource(ds);

    gridFiltros.refresh();

    if (uid != null) {

        row = gridFiltros.table.find("[data-uid=" + uid + "]");

        if (row != null) {
            gridFiltros.select(row);
        }
    }
}


function BusFiltrosAceptar() {

    BusFiltrosfnReturn(DatosDeFiltros);

    $("#busFiltros").modal("hide");

}


