
//---------------------------------------------------------
//  Variables
//---------------------------------------------------------

var BusSelectColumnsViewModel;

var BusSelectColumnsfnLoad;

var BusSelectColumnsfnReturn;

//---------------------------------------------------------
//  Funciones 
//---------------------------------------------------------

function AssignBusSelectColumnsEvents(funcLoad, funcReturn) {

    BusSelectColumnsfnLoad = funcLoad;

    BusSelectColumnsfnReturn = funcReturn;

    $("#busSelectColumns").on('show.bs.modal', function () {
        
        var butMarcarTodo = $("#butMarcarTodoSelectColumns").data("kendoButton");

        if (!butMarcarTodo) {

            $("#butMarcarTodoSelectColumns").kendoButton({
                spriteCssClass: "fa fa-check-square"
            });

            $("#butMarcarTodoSelectColumns").click(function () {
                MarcarTodoSelectColumns();
            });
        }

        var butInvertirMarcas = $("#butInvertirMarcasSelectColumns").data("kendoButton");

        if (!butInvertirMarcas) {

            $("#butInvertirMarcasSelectColumns").kendoButton({
                spriteCssClass: "fa fa-sync"
            });

            $("#butInvertirMarcasSelectColumns").click(function () {
                InvertirMarcasSelectColumns();
            });
        }

        var BooleanoEditor = function (container, options) {
            $('<input type="checkbox" data-bind="checked: ' + options.field + '"/>')
                .appendTo(container)
        };

        var grid = $("#gridBusSelectColumns" ).data("kendoGrid");

        if (!grid) {

            $("#gridBusSelectColumns").kendoGrid({
            
                selectable: "row",
    
                columns: [
                    { field: "visible", title: " ", width: "40px", editor: BooleanoEditor, template: '<input type="checkbox" data-bind="checked: visible"/>' },
                    { field: "title", title: "Columna" },
                ],
    
            });
        }
       
        var resultadosDataSource = CrearDataSourceSelectColumns([]);
        
        BusSelectColumnsViewModel = kendo.observable({

            Resultados: resultadosDataSource
        });

        kendo.bind($("#busSelectColumns"), BusSelectColumnsViewModel);

    });

    $("#busSelectColumns").on('shown.bs.modal', function () {
        BusSelectColumnsLlenarGrilla();
    });

    $("#butSelectColumnsAceptar").kendoButton({
        spriteCssClass: "fa fa-check"
    });

    $("#butSelectColumnsAceptar").click(function () {
        BusSelectColumnsSeleccionar();
    });
}


function BusSelectColumnsLlenarGrilla() {
   
    var columnsInfo = BusSelectColumnsfnLoad();

    var resultadosDataSource = CrearDataSourceSelectColumns(columnsInfo);

    BusSelectColumnsViewModel.set("Resultados", resultadosDataSource);
}


function CrearDataSourceSelectColumns(datos) {

    var ds = new kendo.data.DataSource({
        data: datos,
        schema: {
            model: {
                fields: {
                    visible: { type: "boolean" },
                    title: { type: "string", editable: false },
                    field: { type: "string", editable: false },
                }
            }
        }
    });

    return ds;
}


function MarcarTodoSelectColumns() {

    var uid = null;

    var grid = $("#gridBusSelectColumns" ).data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    if (data != null) {
        uid = data.uid;
    }

    var columnsInfo = BusSelectColumnsViewModel.get("Resultados").data();

    for (i = 0; i < columnsInfo.length; i++) {
        columnsInfo[i].visible = true;
    }

    var resultadosDataSource = CrearDataSourceSelectColumns(columnsInfo);

    BusSelectColumnsViewModel.set("Resultados", resultadosDataSource);

    if (uid != null) {

        row = grid.table.find("[data-uid=" + uid + "]");

        if (row != null) {
            grid.select(row);
        }
    }
}


function InvertirMarcasSelectColumns() {

    var uid = null;

    var grid = $("#gridBusSelectColumns" ).data("kendoGrid");

    var row = grid.select();

    var data = grid.dataItem(row);

    if (data != null) {
        uid = data.uid;
    }

    var columnsInfo = BusSelectColumnsViewModel.get("Resultados").data();

    for (i = 0; i < columnsInfo.length; i++) {

        if (columnsInfo[i].visible) {
            columnsInfo[i].visible = false;
        }
        else {
            columnsInfo[i].visible = true;
        }
    }

    var resultadosDataSource = CrearDataSourceSelectColumns(columnsInfo);

    BusSelectColumnsViewModel.set("Resultados", resultadosDataSource);

    if (uid != null) {

        row = grid.table.find("[data-uid=" + uid + "]");

        if (row != null) {
            grid.select(row);
        }
    }
}


function BusSelectColumnsSeleccionar() {

    var grid = $("#gridBusSelectColumns" ).data("kendoGrid");
       
    var columnsInfo = BusSelectColumnsViewModel.get("Resultados").data();

    var hayColVisible = false;

    for (i = 0; i < columnsInfo.length; i++) {

        if (columnsInfo[i].visible) {
            hayColVisible = true;
            break;
        }
    }

    if (hayColVisible) {
        BusSelectColumnsfnReturn(columnsInfo);
        $("#busSelectColumns").modal("hide");
    }
    else {
        MensInfo("Se debe seleccionar al menos una columna");
    }
}

