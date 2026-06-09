
var mvarArbolMenu = [];

var mvarButtonsColor = "#0078d4";

var mvarButtonsHover = "#005A9E";

var mvarHomeNormalColor = "black";

var mvarHomeHoverColor = "white";

var mvarHomeHoverBackground = "black";

var mvarToolbarHighlight = "#edebe9";

var mvarToolbarBackground = "#ffffff";

var mvarToolbarShadow = "rgb(136 136 136) 0px 10px 33px";

var mvarContextBackground = "#f1f1f1";

var mvarButtonId = "";

var mvarEsHomePage = false;

var mvarConfigCargada = false;

var mvarColorEtiquetas = "";

var mvarSesionHash = "";

$(document).ready(function () {

    inicializarApariencia();

    inicializarHash();

    $("#menutoolbar").kendoContextMenu({
        target: "#toolbar",
        filter: ".toolbutton",
        select: function (e) {

            let opcion = e.item.id;

            if (opcion == "opc1") {

                let onclick = getHtmlPart('onclick=', e.target.outerHTML);

                let str = onclick.substr("InvokeURL(".length);
                str = str.substr(0, str.length - 1);

                var realUrl = str.replace("'", "").replace("'", "");

                InvokeURLinTab(realUrl);
            }
            else if (opcion == "opc2") {

                let id = getHtmlPart('id=', e.target.outerHTML);

                let elem = document.getElementById(id);

                if (elem) {

                    let onclick = getHtmlPart('onclick=', e.target.outerHTML);

                    let str = onclick.substr("InvokeURL(".length);
                    str = str.substr(0, str.length - 1);

                    var url = str.replace("'", "").replace("'", "");

                    var param = {
                        "href": url
                    };

                    MSExecuteOnServer('/Areas/Permisos/EliminarFavorito', param);

                    elem.remove();
                }
            }
        },
        close: function (e) {
            if (!mvarEsHomePage) {
                $("#gohome").hide();
                $("#sortable").hide();
                $("#toolbar").css("background-color", "transparent");
                $("#toolbar").css("box-shadow", "none");
            }
        }
    });

    $('#dismiss, .overlay').on('click', function () {
        $('#sidebar').removeClass('active');
        $('.overlay').fadeOut();
    });

    $('#abrirmenu').on('click', function () {
        abrirMenu();
    });

    $('#mobileabrirmenu').on('click', function () {
        abrirMenu();
    });

    activarTooltip('#abrirmenu');

    $("#toolbar").mouseover(function () {

        if (!mvarEsHomePage) {

            $("#toolbar").css("background-color", mvarToolbarBackground);
            $("#toolbar").css("box-shadow", mvarToolbarShadow);
            $("#toolbar").css('-moz-box-shadow', mvarToolbarShadow);
            $("#toolbar").css('-webkit-box-shadow', mvarToolbarShadow);
            $("#gohome").show();
            $("#sortable").show();
        }
    });

    $("#toolbar").mouseout(function () {

        if (!mvarEsHomePage && $("#menutoolbar").css('display') == 'none') {

            $("#gohome").hide();
            $("#sortable").hide();
            $("#toolbar").css("background-color", "transparent");
            $("#toolbar").css("box-shadow", "none");
        }
    });

    $('#linkLogo').on('click', function () {
        GoHome();
    });

    $('#mobilegohome').on('click', function () {
        GoHome();
    });

    $('#viewconfiginfo').on('click', function () {

        if (mvarConfigCargada) {
            viewConfigInfoClick('#viewconfiginfo');
        }
        else {
            configInfoClick('#viewconfiginfo');
            mvarConfigCargada = true;
        }
    });

    $('#mobileviewconfiginfo').on('click', function () {

        if (mvarConfigCargada) {
            viewConfigInfoClick('#mobileviewconfiginfo');
        }
        else {
            configInfoClick('#mobileviewconfiginfo');
            mvarConfigCargada = true;
        }
    });

    activarTooltip('#viewconfiginfo');

    $('#closeconfiginfo').on('click', function () {
        cerrarTodo();
    });

    var colores = getColores();

    $("#listView").kendoListView({
        dataSource: {
            data: colores
        },
        selectable: "single",
        template: kendo.template($("#template").html()),
    });

    $("#modooscuro").kendoSwitch({
        messages: {
            checked: "SI",
            unchecked: "NO"
        }
    });

    var estilosClaros = getEstilosClaros();

    $("#estiloclaro").kendoDropDownList({
        dataSource: {
            data: estilosClaros
        },
        dataTextField: "Descripcion",
        dataValueField: "Nombre",
        valuePrimitive: true,
    });

    var estilosOscuros = getEstilosOscuros();

    $("#estilooscuro").kendoDropDownList({
        dataSource: {
            data: estilosOscuros
        },
        dataTextField: "Descripcion",
        dataValueField: "Nombre",
        valuePrimitive: true,
    });

    $("#imagenfondo").kendoDropDownList();

    $("#coloretiq").kendoDropDownList({
        dataSource: {
            data: ["Blanco", "Negro"]
        },
    });

    $("#configpredeterminada").kendoButton({
        spriteCssClass: "fa fa-paint-brush"
    });

    $("#configpredeterminada").click(function () {
        SetDefaultConfig();
    });

    $("#configaceptar").kendoButton({
        //spriteCssClass: "fa fa-check"
    });

    $("#configaceptar").click(function () {
        AplicarEstilo();
        UserInfoColor();
    });

    $('#viewsesioninfo').on('click', function () {
        sesionInfoClick('#viewsesioninfo');
    });

    $('#mobileviewsesioninfo').on('click', function () {
        sesionInfoClick('#mobileviewsesioninfo');
    });

    activarTooltip('#viewsesioninfo');

    $("#cerrarsesionaceptar").kendoButton({
        spriteCssClass: "fa fa-sign-out"
    });

    $('#closesesioninfo').on('click', function () {
        cerrarTodo();
    });

    $('#mobileviewtoolbar').on('click', function () {
        mobileToolbarClick();
    });

    $('#closemobiletoolbar').on('click', function () {
        cerrarTodo();
    });

    $('#sesionCambiarPassword').on('click', function () {

        var url = MSGetUrl('/Areas/Procesos/Ejecutar/cambiopassword');

        window.location.href = url;
    });

    var urlBusq = MSGetUrl('/Areas/Busquedas/Buscar');

    $("#headersearch").kendoAutoComplete({
        dataTextField: "opcname",
        dataValueField: "opclink",
        noDataTemplate: 'No hay datos !!!',
        placeholder: "Buscar...",
        clearButton: true,
        minLength: 2,
        height: 500,
        filter: "contains",
        template: '<span style="margin-right: 10px;"><i class="#: data.spriteCssClass #" aria-hidden="true" style="width: 16px;"></i></span>' +
            '<span><b>#: data.opcname #</b><p><h7 class="searchpath">#: data.opcruta #</h7></p></span>',
        footerTemplate: 'Total #: instance.dataSource.total() # encontrados',
        dataSource: {
            dataType: 'json',
            severFiltering: true,
            serverPaging: true,
            transport: {
                read: urlBusq,
                parameterMap: function (data, type) {
                    return { filter: $("#headersearch").val() };
                }
            },
            group: { field: "group" }
        },
        select: function (e) {
            var realUrl = ProcesarURL(e.dataItem.opclink);
            InvokeURL(realUrl);
        }
    });

    $("#headermobilesearch").kendoAutoComplete({
        dataTextField: "opcname",
        dataValueField: "opclink",
        noDataTemplate: 'No hay datos !!!',
        placeholder: "Buscar...",
        clearButton: true,
        minLength: 2,
        height: 500,
        filter: "contains",
        template: '<span style="margin-right: 10px;"><i class="#: data.spriteCssClass #" aria-hidden="true" style="width: 16px;"></i></span>' +
            '<span><b>#: data.opcname #</b><p><h7 class="searchpath">#: data.opcruta #</h7></p></span>',
        footerTemplate: 'Total #: instance.dataSource.total() # encontrados',
        dataSource: {
            dataType: 'json',
            severFiltering: true,
            serverPaging: true,
            transport: {
                read: urlBusq,
                parameterMap: function (data, type) {
                    return { filter: $("#headermobilesearch").val() };
                }
            },
            group: { field: "group" }
        },
        select: function (e) {
            var realUrl = ProcesarURL(e.dataItem.opclink);
            InvokeURL(realUrl);
        }
    });

    $('#mobilegosearch').on('click', function () {

        $('#mobileheader').hide();
        $('#mobilesearch').show();
    });

    $('#cerrarmobilesearch').on('click', function () {

        $('#mobilesearch').hide();
        $('#mobileheader').show();
    });

    $("#menutreeview").kendoTreeView({
        dataSource: [],
        autoScroll: true,
        select: function (e) {
            e.preventDefault();
            var tree = $("#menutreeview").data('kendoTreeView');
            var dataItem = tree.dataItem(e.node);
            if (typeof (dataItem.href) != 'undefined') {
                var realUrl = ProcesarURL(dataItem.href);
                InvokeURL(realUrl);
            }
            else {
                tree.toggle(e.node);
            }
        }
    });

    $("#menu").kendoContextMenu({
        target: "#menutreeview",
        filter: ".k-in",
        select: function (e) {
            var node = $(e.target);
            var dataItem = $("#menutreeview").data("kendoTreeView").dataItem(node);
            if (typeof (dataItem.href) != 'undefined') {
                var realUrl = ProcesarURL(dataItem.href);
                InvokeURLinTab(realUrl);
                $('#sidebar').removeClass('active');
                $('.overlay').fadeOut();
            }
        }
    });

    var arbol = MSExecuteURLOnServer('/Areas/Permisos/TraerArbolMenu');

    var treeview = $("#menutreeview").data("kendoTreeView");

    treeview.setDataSource(new kendo.data.HierarchicalDataSource({
        data: arbol
    }));

    ajustarAlto();

    treeview.collapse(".k-item");

    window.addEventListener("resize", function () {
        ajustarAlto();
    });

    mvarArbolMenu = arbol;

    $('#expandnodes').on('click', function () {
        var tv = $("#menutreeview").data("kendoTreeView");
        tv.expand(".k-item");
    });

    $('#colapsenodes').on('click', function () {
        var tv = $("#menutreeview").data("kendoTreeView");
        tv.collapse(".k-item");
    });

    $("#searchopc").on("input", function () {
        var query = this.value.toLowerCase();
        var dataSource = treeview.dataSource;
        TreeFilter(dataSource, query);
    });

    $(".flatbutton").hover(function () {

        let id = "#" + $(this)[0].id;

        if (id != mvarButtonId) {
            $(this).css("background", mvarButtonsHover);
        }

    }, function () {

        let id = "#" + $(this)[0].id;

        if (id != mvarButtonId) {
            $(this).css("background", mvarButtonsColor);
        }
    });

    window.onclick = function (event) {
        if (!event.target.matches('.contextflatbutton') &&
            event.target.tagName != 'I') {

            let dropdowns = document.getElementsByClassName("dropdown-content");

            for (let i = 0; i < dropdowns.length; i++) {
                let openDropdown = dropdowns[i];
                if (openDropdown.classList.contains('show')) {
                    openDropdown.classList.remove('show');
                }
            }

            $(".contextflatbutton").hide();
        }
    }

    SetDefaultConfig();

    UserInfoColor();

    CreateToolbar();

    $("#gohome").hide();
    $("#sortable").hide();
    $("#toolbar").css("background-color", "transparent");
    $("#toolbar").css("box-shadow", "none");

    ColorFiltro();
});


function inicializarHash() {

    mvarSesionHash = "";

    var funcReturn = function (data) {

        mvarSesionHash = data.Hash;
    }

    MSExecuteOnServerAsync('/Account/ObtenerSesionHash', null, funcReturn, false);

    $(window).focus(function () {

        var nvaFuncReturn = function (data) {

            if (mvarSesionHash.length > 0 && data.Hash != mvarSesionHash) {

                $.blockUI.defaults.css.border = '5px solid darkgrey';
                $.blockUI.defaults.css.zIndex = 1200;
                $.blockUI.defaults.css.cursor = "default";

                $.blockUI.defaults.overlayCSS.zIndex = 1100;
                $.blockUI.defaults.overlayCSS.cursor = "default";

                $.blockUI({ message: $('#sessionCheck'), css: { width: '300px' } });

                $('#butSessionGoHome').click(function () {
                    $.unblockUI();
                    GoHome();
                });
            }
        }

        MSExecuteOnServerAsync('/Account/ObtenerSesionHash', null, nvaFuncReturn, false);
    });
}


function inicializarApariencia() {

    var imagenFondo = $("#currentImagenFondo").val();

    if (imagenFondo.length > 0) {

        if (imagenFondo != "(Ninguna)") {

            var url =  "url('" + MSGetUrl("/Content/Images/Fondos/" + imagenFondo) + "')";

            document.body.style.backgroundImage = url;

            document.body.style.backgroundColor = "#808080";
        }
    }

    var colorEtiquetas = $("#currentColorEtiquetas").val();

    mvarColorEtiquetas = colorEtiquetas;

    var kendoTheme = $("#currentTheme").val();

    var headerColor = $("#currentHeaderColor").val();

    var hoverColor = getHoverColor(headerColor);

    mvarButtonsColor = headerColor;

    mvarButtonsHover = hoverColor;
    
    if (esModoOscuro(kendoTheme)) {

        mvarHomeNormalColor = "white";

        mvarHomeHoverColor = "black";

        mvarHomeHoverBackground = "white"

        mvarToolbarHighlight = "#323130"

        mvarToolbarBackground = "#483c3c";

        mvarToolbarShadow = "rgb(15 15 15) 0px 10px 33px";

        mvarContextBackground = "#0c0c0c";

        $("#sidebar").css("color", "white");
        $("#sidebar").css("background", "#292323");

        $("#menutreeview").css("color", "white");
        $("#menutreeview").css("background", "#292323");
    }
    else {
       
        mvarHomeNormalColor = "black";

        mvarHomeHoverColor = "white";

        mvarHomeHoverBackground = "black"

        mvarToolbarHighlight = "#edebe9"

        mvarToolbarBackground = "lightgray";

        mvarToolbarShadow = "rgb(136 136 136) 0px 10px 33px";

        mvarContextBackground = "#f1f1f1";

        $("#sidebar").css("color", "black");
        $("#sidebar").css("background", "#f3f2f1");

        $("#menutreeview").css("color", "black");
        $("#menutreeview").css("background", "#f3f2f1");
    }
}


function getHoverColor(color) {

    var result = "";

    var colores = getColores();

    for (var i = 0; i < colores.length; i++) {

        var item = colores[i];

        if (item.color == color) {
            result = item.hover;
            break;
        }
    }

    return result;
}


function esModoOscuro(kendoTheme) {

    var result = false;

    var estilosOscuros = getEstilosOscuros();

    for (var i = 0; i < estilosOscuros.length; i++) {

        var item = estilosOscuros[i];

        if (item.Nombre == kendoTheme) {
            result = true;
            break;
        }
    }

    return result;
}


function getColores() {

    var result = [
        { id: 1, title: "Azul Dafault", color: "#0078d4", hover: "#005A9E" },
        { id: 2, title: "Azul Contraste", color: "#0001ff", hover: "#0000D7" },
        { id: 3, title: "Negro", color: "#212121", hover: "#484141" },
        { id: 4, title: "Mora", color: "#17234e", hover: "#3d4a79" },
        { id: 5, title: "Cordoban", color: "#570000", hover: "#340102" },
        { id: 6, title: "Cordoban Oscuro", color: "#380000", hover: "#561f1f" },
        { id: 7, title: "Naranja Oscuro", color: "#d24726", hover: "#BA2A1D" },
        { id: 8, title: "Uva", color: "#432158", hover: "#2D163B" },
        { id: 9, title: "Azul Claro", color: "#5db2ff", hover: "#4095FC" },
        { id: 10, title: "Verde Claro", color: "#82ba00", hover: "#5E9B1A" },
        { id: 11, title: "Azul Oscuro", color: "#004b8b", hover: "#032D63" },
        { id: 12, title: "Naranja", color: "#e64524", hover: "#CC3716" },
        { id: 13, title: "Rosa", color: "#dc4fad", hover: "#C8338A" },
        { id: 14, title: "Rosa Oscuro", color: "#dd0f94", hover: "#C8338A" },
        { id: 15, title: "Granada", color: "#911844", hover: "#6E1334" },
        { id: 16, title: "Frambuesa", color: "#8d398f", hover: "#712E72" },
        { id: 17, title: "Cerceta", color: "#008299", hover: "#0A5C73" },
        { id: 18, title: "Sandia", color: "#007239", hover: "#004A26" },
    ];

    return result;
}


function getEstilosClaros() {

    var result = [
        { Nombre: "blueopal", Descripcion: "Blue Opal" },
        { Nombre: "bootstrap", Descripcion: "Bootstrap" },
        { Nombre: "default", Descripcion: "Default" },
        { Nombre: "fiori", Descripcion: "Fiori" },
        { Nombre: "material", Descripcion: "Material" },
        { Nombre: "metro", Descripcion: "Metro" },
        { Nombre: "nova", Descripcion: "Nova" },
        { Nombre: "office365", Descripcion: "Office 365" },
        { Nombre: "silver", Descripcion: "Silver" },
        { Nombre: "uniform", Descripcion: "Uniform" }
    ];

    return result;
}


function getEstilosOscuros() {

    var result = [
        { Nombre: "black", Descripcion: "Black" },
        { Nombre: "flat", Descripcion: "Flat" },
        { Nombre: "highcontrast", Descripcion: "High Contrast" },
        { Nombre: "materialblack", Descripcion: "Material Black" },
        { Nombre: "metroblack", Descripcion: "Metro Black" },
        { Nombre: "moonlight", Descripcion: "Moonlight" }
    ];

    return result;
}


function getHeadercolorindex(color) {

    var index = 0;

    var colores = getColores();

    for (var i = 0; i < colores.length; i++) {

        var item = colores[i];

        if (item.color == color) {
            index = i;
            break;
        }
    }

    return index;
}


function getEstiloclaroindex(theme) {

    var index = 0;

    var estilosClaros = getEstilosClaros();

    for (var i = 0; i < estilosClaros.length; i++) {

        var item = estilosClaros[i];

        if (item.Nombre == theme) {
            index = i;
            break;
        }
    }

    return index;
}


function getEstilooscuroindex(theme) {

    var index = 0;

    var estilosOscuros = getEstilosOscuros();

    for (var i = 0; i < estilosOscuros.length; i++) {

        var item = estilosOscuros[i];

        if (item.Nombre == theme) {
            index = i;
            break;
        }
    }

    return index;
}


function getImagenfondoindex(imagenfondo, fondos) {

    var index = 0;

    for (var i = 0; i < fondos.length; i++) {

        if (fondos[i] == imagenfondo) {
            index = i;
            break;
        }
    }

    return index;
}


function getColoretiquetasindex(coloretiquetas) {

    var index = 0;

    if (coloretiquetas == "Blanco") {
        index = 0;
    }
    else if (coloretiquetas == "Negro") {
        index = 1;
    }

    return index;
}


function abrirMenu() {

    if ($('#sidebar').hasClass('active')) {
        $('#sidebar').removeClass('active');
        $('.overlay').fadeOut();
    }
    else {
        $('#sidebar').addClass('active');
        $('.overlay').fadeIn();
        $('.collapse.in').toggleClass('in');
        $('a[aria-expanded=true]').attr('aria-expanded', 'false');
    }

    cerrarTodo();
}


function configInfoClick(butonId) {

    if ($('#configinfo').css('display') == 'none') {

        var funcReturn = function (data) {

            cerrarTodo();

            var headercolorindex = getHeadercolorindex(data.color);

            var estiloclaroindex = getEstiloclaroindex(data.estiloclaro);

            var estilooscuroindex = getEstilooscuroindex(data.estilooscuro);

            var imagenfondoindex = getImagenfondoindex(data.imagenfondo, data.fondos);

            var coloretiquetasindex = getColoretiquetasindex(data.coloretiquetas);

            var listView = $("#listView").data("kendoListView");

            listView.select(listView.content.children()[headercolorindex]);

            var modooscuro = $("#modooscuro").data("kendoSwitch");

            modooscuro.check(data.modooscuro);

            $("#estiloclaro").data("kendoDropDownList").select(estiloclaroindex);

            $("#estilooscuro").data("kendoDropDownList").select(estilooscuroindex);

            var cmbimagen = $("#imagenfondo").data("kendoDropDownList");

            cmbimagen.setDataSource(data.fondos);

            cmbimagen.select(imagenfondoindex);

            $("#coloretiq").data("kendoDropDownList").select(coloretiquetasindex);

            mvarButtonId = butonId;

            $(butonId).css("color", "black");
            $(butonId).css("background", "#faf9f8");

            document.getElementById('configinfo').style.top = "-580px";

            $('#configinfo').show();
            $('#configinfo').animate({ top: '46px' });
        }

        MSExecuteOnServerAsync('/Account/ObtenerEstilos', null, funcReturn, false);
    }
    else {
        cerrarTodo();
    }
}


function viewConfigInfoClick(butonId) {

    if ($('#configinfo').css('display') == 'none') {

        cerrarTodo();

        mvarButtonId = butonId;

        $(butonId).css("color", "black");
        $(butonId).css("background", "#faf9f8");

        document.getElementById('configinfo').style.top = "-580px";

        $('#configinfo').show();
        $('#configinfo').animate({ top: '46px' });
    }
    else {
        cerrarTodo();
    }
}


function sesionInfoClick(butonId) {

    if ($('#sesioninfo').css('display') == 'none') {

        cerrarTodo();

        var datos = MSExecuteURLOnServer('/Account/ObtenerSesionInfo');

        $("#sesionIniciales").text(datos.Iniciales);
        $("#sesionUsuario").text(datos.NombreCompleto);
        $("#sesionEmail").text(datos.UsuarioEmail);

        $("#sesionEmpresa").text(datos.Empresa);
        $("#sesionEjercicio").text(datos.Ejercicio);
        $("#sesionContableEjercicio").text(datos.ContableEjercicio);
        $("#sesionSistema").text(datos.Sistema + " " + datos.Version);

        mvarButtonId = butonId;

        $(butonId).css("color", "black");
        $(butonId).css("background", "#faf9f8");

        $(".circulo").css("color", "black");
        $(".circulo").css("border-color", "black");
        $(".circulo").css("background", "#faf9f8");

        document.getElementById('sesioninfo').style.top = "-400px";

        $('#sesioninfo').show();
        $('#sesioninfo').animate({ top: '46px' });
    }
    else {
        cerrarTodo();
    }
}

function mobileToolbarClick() {

    if ($('#mobiletoolbarinfo').css('display') == 'none') {

        cerrarTodo();

        mvarButtonId = '#mobileviewtoolbar';

        actualizarMobileToolbar();

        $('mobileviewtoolbar').css("color", "black");
        $('mobileviewtoolbar').css("background", "#faf9f8");

        document.getElementById('mobiletoolbarinfo').style.top = "-300px";

        $('#mobiletoolbarinfo').show();
        $('#mobiletoolbarinfo').animate({ top: '46px' });
    }
    else {
        cerrarTodo();
    }
}


function actualizarMobileToolbar() {

    var datos = getListaFavoritos();

    var html = "<table>\r\n";

    for (var i = 0; i < datos.length; i++) {

        var item = datos[i];

        html += "<tr>\r\n";
        html += '<td><button class="k-button" type="button" data-role="button" style="height: 30px;width:36px;" onclick="' + item.onclick + '"><span><i class="' + item.icono + '" aria-hidden="true"></i></span></button></td>\r\n'
        html += '<td style="padding: 6px;"><a href="#" onclick="' + item.onclick + '">' + item.title + '</a></td>\r\n'

        html += "</tr>\r\n";
    }

    html += "</table>\r\n"

    var container = document.getElementById("mobiletoolbar");

    container.innerHTML = html;
}


function getListaFavoritos() {

    var datos = [];

    var elem = $("#sortable")

    for (var i = 0; i < elem[0].childElementCount; i++) {

        var item = elem[0].children[i];

        var html = item.outerHTML;

        var title = getHtmlPart('data-title=', html);
        var onclick = getHtmlPart('onclick=', html);
        var icono = getHtmlPart('<i class=', html);

        datos.push({ title: title, onclick: onclick, icono: icono });
    }

    return datos;
}


function getHtmlPart(searchexp, html) {

    var result = "";

    var ini = html.search(searchexp);

    if (ini >= 0) {

        var str = html.substr(ini + searchexp.length + 1);

        var fin = str.indexOf('"');

        if (fin >= 0) {

            result = str.substr(0, fin);
        }
    }

    return result;
}

function cerrarTodo() {

    mvarButtonId = "";

    $('#viewconfiginfo').css("color", "white");
    $('#viewconfiginfo').css("background", mvarButtonsColor);

    $('#mobileviewconfiginfo').css("color", "white");
    $('#mobileviewconfiginfo').css("background", mvarButtonsColor);

    if ($('#configinfo').css('display') != 'none') {
        $('#configinfo').hide();
    }

    $('#viewsesioninfo').css("color", "white");
    $('#viewsesioninfo').css("background", mvarButtonsColor);

    $('#viewsesioninfocirculo').css("color", "white");
    $('#viewsesioninfocirculo').css("border-color", "white");
    $('#viewsesioninfocirculo').css("background", "transparent");

    $('#mobileviewsesioninfocirculo').css("color", "white");
    $('#mobileviewsesioninfocirculo').css("border-color", "white");
    $('#mobileviewsesioninfocirculo').css("background", "transparent");

    $('#mobileviewsesioninfo').css("color", "white");
    $('#mobileviewsesioninfo').css("background", mvarButtonsColor);

    if ($('#sesioninfo').css('display') != 'none') {
        $('#sesioninfo').hide();
    }

    $('#mobileviewtoolbar').css("color", "white");
    $('#mobileviewtoolbar').css("background", mvarButtonsColor);

    if ($('#mobiletoolbarinfo').css('display') != 'none') {
        $('#mobiletoolbarinfo').hide();
    }
}


function activarTooltip(id) {

    $(id).kendoTooltip({
        position: "bottom",
        showAfter: 1000,
        animation: {
            open: {
                effects: "zoom",
                duration: 150
            }
        }
    });
}


function TreeFilter(dataSource, query) {
    var hasVisibleChildren = false;
    var data = dataSource instanceof kendo.data.HierarchicalDataSource && dataSource.data();

    for (var i = 0; i < data.length; i++) {
        var item = data[i];
        var text = item.text.toLowerCase();
        var itemVisible =
            query === true
            || query === ""
            || text.indexOf(query) >= 0;

        var anyVisibleChildren = TreeFilter(item.children, itemVisible || query);

        hasVisibleChildren = hasVisibleChildren || anyVisibleChildren || itemVisible;

        item.hidden = !itemVisible && !anyVisibleChildren;
    }

    if (data) {
        dataSource.filter({ field: "hidden", operator: "neq", value: true });
    }

    return hasVisibleChildren;
}


function ajustarAlto() {

    let h = window.innerHeight;

    h = h - 143;

    if (h > 100) {
        $('#menutreecontainer').height(h);
    }

    h = window.innerHeight;

    h = h - 52;

    if (h > 100) {
        $('#toolbar').height(h);
    }

    AjustarHeader();
}


function AjustarHeader() {

    let w = window.innerWidth;

    if (w >= 716) {
        $('#mobileheader').hide();
        $('#mobilesearch').hide();
    }
    else {
        $('#mobileheader').show();
        $('#mobilesearch').hide();
    }

    cerrarTodo();
}


function ProcesarURL(link) {

    var appName = MSGetAppName();

    if (appName.length > 0) {
        link = "/" + appName + link;
    }

    return link;
}


function InvokeURL(realUrl) {

    window.location.href = realUrl;
}


function InvokeURLinTab(realUrl) {

    window.open(realUrl, "_blank");
}


function GoHome() {

    var url = MSGetUrl('/');

    window.location.href = url;
}


function CreateToolbar() {

    $("#toolbar").css("background-color", mvarToolbarBackground);
    $("#toolbar").css("box-shadow", mvarToolbarShadow);
    $("#toolbar").css('-moz-box-shadow', mvarToolbarShadow);
    $("#toolbar").css('-webkit-box-shadow', mvarToolbarShadow);

    let htmltoolbar = "";

    htmltoolbar += '<div id="gohome" class="flathome" title="Ir al Inicio" style="display: block;margin-top: 8px;" onclick="GoHome()">\r\n';
    htmltoolbar += '<i class="fa fa-home fa-lg" aria-hidden="true" style="vertical-align: middle;"></i>\r\n';
    htmltoolbar += '</div>\r\n';

    htmltoolbar += '<ul id="sortable">\r\n';

    var result = MSExecuteURLOnServer('/Areas/Permisos/TraerFavoritos');

    for (let i = 0; i < result.Datos.length; i++) {

        let elem = result.Datos[i];

        let id = getMenuItemID(elem.href);

        if (id > 0) {

            let botonhtml = '<li id="tb_elem_' + id.toString() + '" class="toolbutton" type="button" data-role="button" title="' + elem.texto.trim() + '" data-title="' + elem.texto.trim() + '" onclick="' + "InvokeURL('" + ProcesarURL(elem.href.trim()) + "')" + '"><span><i class="' + elem.cssclass + ' fa-lg' + '" aria-hidden="true"></i></span></li>';

            htmltoolbar += botonhtml + '\r\n';
        }

    }

    htmltoolbar += '</ul>\r\n';

    var toolbarcontainer = document.getElementById("toolbar");

    toolbarcontainer.innerHTML = htmltoolbar;

    $("#sortable").sortable({
        update: function (event, ui) {

            var listaBotones = [];

            var botones = $("#sortable").children();

            for (let i = 0; i < botones.length; i++) {

                var html = botones[i].outerHTML;

                var title = getHtmlPart('data-title=', html);
                var onclick = getHtmlPart('onclick=', html);
                var icono = getHtmlPart('<i class=', html);

                var href = onclick.replace("InvokeURL('", "");

                href = href.replace("')", "");

                icono = icono.replace(" fa-lg", "");

                listaBotones.push({ text: title, href: href, cssclass: icono });
            }

            var param = {
                "botones": listaBotones
            }

            MSExecuteOnServer('/Areas/Permisos/GuardarFavoritos', param);
        },
        // change: function(e) {
        //     console.log("change");
        // }
    });

    $("#sortable").disableSelection();

    let tooltip = $("#toolbar").data("kendoTooltip");

    if (!tooltip) {

        $("#toolbar").kendoTooltip({
            filter: "li",
            position: "right",
            showAfter: 500,
            animation: {
                open: {
                    effects: "zoom",
                    duration: 200
                }
            }
        });
    }

    $("#gohome").kendoTooltip({
        position: "right",
        showAfter: 500,
        animation: {
            open: {
                effects: "zoom",
                duration: 200
            }
        }
    });

    $("#gohome").css("color", mvarHomeNormalColor);
    $("#gohome").css("background", "transparent");
    $("#gohome").css("box-shadow", "none");

    $("#gohome").hover(function () {

        $(this).css("color", mvarHomeHoverColor);
        $(this).css("background", mvarHomeHoverBackground);

    }, function () {

        $(this).css("color", mvarHomeNormalColor);
        $(this).css("background", "transparent");
    });

    $(".toolbutton").hover(function () {

        $(this).css("background", mvarToolbarHighlight);

    }, function () {

        $(this).css("background", "transparent");
    });
}


function getMenuItemID(href) {

    let param = {
        "id": -1,
    };

    getMenuItemID_Nodos(mvarArbolMenu, href.trim().toLowerCase(), param)

    return param.id;
}


function getMenuItemID_Nodos(items, href, param) {

    for (let i = 0; i < items.length; i++) {

        let elem = items[i];

        if ((elem.href) && elem.href.trim().toLowerCase() == href) {
            param.id = elem.id;
            break;
        }
        else if (typeof (elem.items) != 'undefined' && elem.items.length > 0) {
            getMenuItemID_Nodos(elem.items, href, param);
        }
    }
}


function SetDefaultConfig() {

    var listView = $("#listView").data("kendoListView");

    listView.select(listView.content.children().first());

    var modooscuro = $("#modooscuro").data("kendoSwitch");

    modooscuro.check(false);

    $("#estiloclaro").data("kendoDropDownList").select(1);

    $("#estilooscuro").data("kendoDropDownList").select(0);

    var cmbimagen = $("#imagenfondo").data("kendoDropDownList");

    var fondos = cmbimagen.dataSource.data();

    var imagenfondoindex = getImagenfondoindex("FondoNumbit.png", fondos);

    cmbimagen.select(imagenfondoindex);

    $("#coloretiq").data("kendoDropDownList").select(0);
}


function AplicarEstilo() {

    var color = "";

    var hover = "";

    var listView = $("#listView").data("kendoListView");

    var colorItem = listView.select();

    var dataItem = listView.dataItem(colorItem);

    if (dataItem) {

        color = dataItem.color;

        hover = dataItem.hover;
    }

    var modooscuro = $("#modooscuro").data("kendoSwitch").value();

    var estiloclaro = $("#estiloclaro").data("kendoDropDownList").value();

    var estilooscuro = $("#estilooscuro").data("kendoDropDownList").value();

    var imagenfondo = $("#imagenfondo").data("kendoDropDownList").value();

    var coloretiquetas = $("#coloretiq").data("kendoDropDownList").value();

    $("#paramcolor").val(color);
    $("#paramhover").val(hover);
    $("#parammodooscuro").val((modooscuro) ? "true" : "false");
    $("#paramestiloclaro").val(estiloclaro);
    $("#paramestilooscuro").val(estilooscuro);
    $("#paramimagenfondo").val(imagenfondo);
    $("#paramcoloretiquetas").val(coloretiquetas);

    document.forms["estilosForm"].submit();
}

function UserInfoColor() {
    var colorHeader = $("#navheader").css("background");
    $(".userinfo").first().css("background", colorHeader);
    $(".circulosesion").first().css("background", colorHeader);
    $(".circulosesion").css("filter", "brightness(80%)");
}

class Color {
    constructor(r, g, b) {
        this.set(r, g, b);
    }

    toString() {
        return `rgb(${Math.round(this.r)}, ${Math.round(this.g)}, ${Math.round(this.b)})`;
    }

    set(r, g, b) {
        this.r = this.clamp(r);
        this.g = this.clamp(g);
        this.b = this.clamp(b);
    }

    hueRotate(angle = 0) {
        angle = angle / 180 * Math.PI;
        const sin = Math.sin(angle);
        const cos = Math.cos(angle);

        this.multiply([
            0.213 + cos * 0.787 - sin * 0.213,
            0.715 - cos * 0.715 - sin * 0.715,
            0.072 - cos * 0.072 + sin * 0.928,
            0.213 - cos * 0.213 + sin * 0.143,
            0.715 + cos * 0.285 + sin * 0.140,
            0.072 - cos * 0.072 - sin * 0.283,
            0.213 - cos * 0.213 - sin * 0.787,
            0.715 - cos * 0.715 + sin * 0.715,
            0.072 + cos * 0.928 + sin * 0.072,
        ]);
    }

    grayscale(value = 1) {
        this.multiply([
            0.2126 + 0.7874 * (1 - value),
            0.7152 - 0.7152 * (1 - value),
            0.0722 - 0.0722 * (1 - value),
            0.2126 - 0.2126 * (1 - value),
            0.7152 + 0.2848 * (1 - value),
            0.0722 - 0.0722 * (1 - value),
            0.2126 - 0.2126 * (1 - value),
            0.7152 - 0.7152 * (1 - value),
            0.0722 + 0.9278 * (1 - value),
        ]);
    }

    sepia(value = 1) {
        this.multiply([
            0.393 + 0.607 * (1 - value),
            0.769 - 0.769 * (1 - value),
            0.189 - 0.189 * (1 - value),
            0.349 - 0.349 * (1 - value),
            0.686 + 0.314 * (1 - value),
            0.168 - 0.168 * (1 - value),
            0.272 - 0.272 * (1 - value),
            0.534 - 0.534 * (1 - value),
            0.131 + 0.869 * (1 - value),
        ]);
    }

    saturate(value = 1) {
        this.multiply([
            0.213 + 0.787 * value,
            0.715 - 0.715 * value,
            0.072 - 0.072 * value,
            0.213 - 0.213 * value,
            0.715 + 0.285 * value,
            0.072 - 0.072 * value,
            0.213 - 0.213 * value,
            0.715 - 0.715 * value,
            0.072 + 0.928 * value,
        ]);
    }

    multiply(matrix) {
        const newR = this.clamp(this.r * matrix[0] + this.g * matrix[1] + this.b * matrix[2]);
        const newG = this.clamp(this.r * matrix[3] + this.g * matrix[4] + this.b * matrix[5]);
        const newB = this.clamp(this.r * matrix[6] + this.g * matrix[7] + this.b * matrix[8]);
        this.r = newR;
        this.g = newG;
        this.b = newB;
    }

    brightness(value = 1) {
        this.linear(value);
    }
    contrast(value = 1) {
        this.linear(value, -(0.5 * value) + 0.5);
    }

    linear(slope = 1, intercept = 0) {
        this.r = this.clamp(this.r * slope + intercept * 255);
        this.g = this.clamp(this.g * slope + intercept * 255);
        this.b = this.clamp(this.b * slope + intercept * 255);
    }

    invert(value = 1) {
        this.r = this.clamp((value + this.r / 255 * (1 - 2 * value)) * 255);
        this.g = this.clamp((value + this.g / 255 * (1 - 2 * value)) * 255);
        this.b = this.clamp((value + this.b / 255 * (1 - 2 * value)) * 255);
    }

    hsl() {
        // Code taken from https://stackoverflow.com/a/9493060/2688027, licensed under CC BY-SA.
        const r = this.r / 255;
        const g = this.g / 255;
        const b = this.b / 255;
        const max = Math.max(r, g, b);
        const min = Math.min(r, g, b);
        let h, s, l = (max + min) / 2;

        if (max === min) {
            h = s = 0;
        } else {
            const d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch (max) {
                case r:
                    h = (g - b) / d + (g < b ? 6 : 0);
                    break;

                case g:
                    h = (b - r) / d + 2;
                    break;

                case b:
                    h = (r - g) / d + 4;
                    break;
            }
            h /= 6;
        }

        return {
            h: h * 100,
            s: s * 100,
            l: l * 100,
        };
    }

    clamp(value) {
        if (value > 255) {
            value = 255;
        } else if (value < 0) {
            value = 0;
        }
        return value;
    }
}

class Solver {
    constructor(target, baseColor) {
        this.target = target;
        this.targetHSL = target.hsl();
        this.reusedColor = new Color(0, 0, 0);
    }

    solve() {
        const result = this.solveNarrow(this.solveWide());
        return {
            values: result.values,
            loss: result.loss,
            filter: this.css(result.values),
        };
    }

    solveWide() {
        const A = 5;
        const c = 15;
        const a = [60, 180, 18000, 600, 1.2, 1.2];

        let best = { loss: Infinity };
        for (let i = 0; best.loss > 25 && i < 3; i++) {
            const initial = [50, 20, 3750, 50, 100, 100];
            const result = this.spsa(A, a, c, initial, 1000);
            if (result.loss < best.loss) {
                best = result;
            }
        }
        return best;
    }

    solveNarrow(wide) {
        const A = wide.loss;
        const c = 2;
        const A1 = A + 1;
        const a = [0.25 * A1, 0.25 * A1, A1, 0.25 * A1, 0.2 * A1, 0.2 * A1];
        return this.spsa(A, a, c, wide.values, 500);
    }

    spsa(A, a, c, values, iters) {
        const alpha = 1;
        const gamma = 0.16666666666666666;

        let best = null;
        let bestLoss = Infinity;
        const deltas = new Array(6);
        const highArgs = new Array(6);
        const lowArgs = new Array(6);

        for (let k = 0; k < iters; k++) {
            const ck = c / Math.pow(k + 1, gamma);
            for (let i = 0; i < 6; i++) {
                deltas[i] = Math.random() > 0.5 ? 1 : -1;
                highArgs[i] = values[i] + ck * deltas[i];
                lowArgs[i] = values[i] - ck * deltas[i];
            }

            const lossDiff = this.loss(highArgs) - this.loss(lowArgs);
            for (let i = 0; i < 6; i++) {
                const g = lossDiff / (2 * ck) * deltas[i];
                const ak = a[i] / Math.pow(A + k + 1, alpha);
                values[i] = fix(values[i] - ak * g, i);
            }

            const loss = this.loss(values);
            if (loss < bestLoss) {
                best = values.slice(0);
                bestLoss = loss;
            }
        }
        return { values: best, loss: bestLoss };

        function fix(value, idx) {
            let max = 100;
            if (idx === 2 /* saturate */) {
                max = 7500;
            } else if (idx === 4 /* brightness */ || idx === 5 /* contrast */) {
                max = 200;
            }

            if (idx === 3 /* hue-rotate */) {
                if (value > max) {
                    value %= max;
                } else if (value < 0) {
                    value = max + value % max;
                }
            } else if (value < 0) {
                value = 0;
            } else if (value > max) {
                value = max;
            }
            return value;
        }
    }

    loss(filters) {
        const color = this.reusedColor;
        color.set(0, 0, 0);

        color.invert(filters[0] / 100);
        color.sepia(filters[1] / 100);
        color.saturate(filters[2] / 100);
        color.hueRotate(filters[3] * 3.6);
        color.brightness(filters[4] / 100);
        color.contrast(filters[5] / 100);

        const colorHSL = color.hsl();
        return (
            Math.abs(color.r - this.target.r) +
            Math.abs(color.g - this.target.g) +
            Math.abs(color.b - this.target.b) +
            Math.abs(colorHSL.h - this.targetHSL.h) +
            Math.abs(colorHSL.s - this.targetHSL.s) +
            Math.abs(colorHSL.l - this.targetHSL.l)
        );
    }

    css(filters) {
        function fmt(idx, multiplier = 1) {
            return Math.round(filters[idx] * multiplier);
        }
        return `filter: invert(${fmt(0)}%) sepia(${fmt(1)}%) saturate(${fmt(2)}%) hue-rotate(${fmt(3, 3.6)}deg) brightness(${fmt(4)}%) contrast(${fmt(5)}%);`;
    }
}

function ColorFiltro() {
    const rgb2hex = (rgb) => `#${rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/).slice(1).map(n => parseInt(n, 10).toString(16).padStart(2, '0')).join('')}`;

    var fondoheadercolor = $('#navheader').css("background-color");
    var colorhex = rgb2hex(fondoheadercolor);

    const rgb = hexToRgb(colorhex);
    const color = new Color(rgb[0], rgb[1], rgb[2]);
    const solver = new Solver(color);
    const result = solver.solve();

    $('.user-background').attr('style', result.filter);
}

function hexToRgb(hex) {
    const shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
    hex = hex.replace(shorthandRegex, (m, r, g, b) => {
        return r + r + g + g + b + b;
    });

    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result
        ? [
            parseInt(result[1], 16),
            parseInt(result[2], 16),
            parseInt(result[3], 16),
        ]
        : null;
}