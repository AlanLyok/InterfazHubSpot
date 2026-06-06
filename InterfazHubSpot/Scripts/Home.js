

$(document).ready(function () {

    mvarEsHomePage = true;

    ViewHome();
});


function ViewHome() {

    $('#homepage').show();

    $("#toolbar").css("background-color", mvarToolbarBackground);

    $("#gohome").show();
    $("#sortable").show();

    var html = CreateHtmlTile(mvarArbolMenu, -1, "Opciones");

    showTiles(html);

    cerrarTodo();

    ajustarAlto();
}


function CreateHtmlTile(items, parentId, titulo) {

    let ismobile = IsMobile();

    let colorEtiquetas = "black";

    if (mvarColorEtiquetas == "Blanco") {
        colorEtiquetas = "white";
    }

    let html = "";

    html += '<h3 class="tilesTitle" style="color: ' + colorEtiquetas + ';">' + titulo + '</h3>\r\n';
    html += '<br />\r\n';
    html += '<div class="row" style="display: block;">\r\n';
    html += '<div class="col-md-12">\r\n';

    for (let i = 0; i < items.length; i++) {

        var unHtml = CreateUnTile(items[i], ismobile, colorEtiquetas);

        html += unHtml;
    }

    html += '</div>\r\n';
    html += '</div>\r\n';
    html += '</div>\r\n';

    if (parentId >= 0) {
        html += '<div class="row">\r\n';
        html += '<div class="col-md-12">\r\n';
        html += '<button id="butVolver" class="k-primary" type="button" data-role="button" style="margin-left: 40px;margin-top: 20px;"  onclick="BackFromTile(' + parentId.toString() + ')">Volver</button>\r\n';
        html += '</div>\r\n';
        html += '</div>\r\n';
        html += '<br />\r\n';
    }

    return html;
}


function CreateUnTile(elem, ismobile, colorEtiquetas) {

    let html = "";

    if (elem.href && !ismobile) {

        html += '<div class="tileBlock">';
        html += '<div class="tileElem">';
        html += '<div id="tb_' + elem.id.toString() + '" class="tileButton">';
        html += '<button class="k-button has-tooltip" type="button" data-role="button" onclick="InvokeFromTile(' + elem.id.toString() + ')" style="width: 88px;height: 66;" title="' + elem.text + '">';
        html += '<span>';
        html += '<i class="' + elem.spriteCssClass + ' fa-4x link-tile-color" style="color: ' + colorEtiquetas + ';" aria-hidden="true"></i>';
        html += '</span>';
        html += '<div class="tileLink">';
        html += '<a href="#" class="link-tile-color" style="color: ' + colorEtiquetas + ';" onclick="InvokeFromTile(' + elem.id.toString() + ')">' + elem.text + '</a>';
        html += '</div>';
        html += '</button>';
        html += '<span class="tooltip-home"><p>' + elem.text + '</p></span>';
        html += '<div class="dropdowntile">';
        html += '<div id="tb_but_' + elem.id.toString() + '" class="contextflatbutton" style="display: none;" title="Abrir menu contextual" onclick="OpenTileMenu(' + elem.id.toString() + ')">';
        html += '<i class="fa fa-align-justify" aria-hidden="true"  style="color: ' + colorEtiquetas + ';"></i>';
        html += '</div>';
        html += '<div id="tb_drp_' + elem.id.toString() + '" class="dropdown-content">';
        html += '<span class="menuitem" onclick="InvokeFromTileinTab(' + elem.id.toString() + ')">Abrir en otra pestaña</span>';
        html += '<span class="menuitem" onclick="AnclarFavorito(' + elem.id.toString() + ')">Anclar como favorito</span>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        html += '</div>\r\n';
    }
    else {

        html += '<div class="tileBlock">';
        html += '<div class="tileElem">';
        html += '<div class="tileButton">';
        html += '<button class="k-button" type="button" data-role="button" onclick="InvokeFromTile(' + elem.id.toString() + ')" style="width: 88px;height: 66;">';
        html += '<span>';
        html += '<i class="' + elem.spriteCssClass + ' fa-4x link-tile-color" style="color: ' + colorEtiquetas + ';" aria-hidden="true"></i>';
        html += '</span>';
        html += '<div class="tileLink">';
        html += '<a href="#" class="link-tile-color" style="color: ' + colorEtiquetas + ';" onclick="InvokeFromTile(' + elem.id.toString() + ')">' + elem.text + '</a>';
        html += '</div>';
        html += '</button>';
        html += '<span class="tooltip-home"><p>' + elem.text + '</p></span>';
        html += '</div>';
        html += '</div>';
        html += '</div>\r\n';
    }

    return html;
}


function InvokeFromTile(id) {

    var result = GetCurrentTileItem(id);
    var titulo = result.titulo;
    var parentId = result.parentId;
    var item = result.item;

    if (typeof (item.items) != 'undefined' && item.items.length > 0) {
        var html = CreateHtmlTile(item.items, parentId, titulo);
        showTiles(html);
        SetButtonIcon("#butVolver", "fa fa-arrow-left");
    }
    else if (item.href.length > 0) {
        var realUrl = ProcesarURL(item.href);
        InvokeURL(realUrl);
    }
    else {
        MensErr("No hay nada para ejecutar");
    }
}


function InvokeFromTileinTab(id) {

    var result = GetCurrentTileItem(id);
    var titulo = result.titulo;
    var parentId = result.parentId;
    var item = result.item;

    if (typeof (item.items) != 'undefined' && item.items.length > 0) {
        var html = CreateHtmlTile(item.items, parentId, titulo);
        showTiles(html);
        SetButtonIcon("#butVolver", "fa fa-arrow-left");
    }
    else if (item.href.length > 0) {
        var realUrl = ProcesarURL(item.href);
        InvokeURLinTab(realUrl);
    }
    else {
        MensErr("No hay nada para ejecutar");
    }
}


function BackFromTile(id) {

    var result = GetCurrentTileItem(id);
    var titulo = result.titulo;
    var parentId = 0;
    var items = [];

    if (result.item.items) {
        items = result.item.items;
        parentId = result.parentId;
    }
    else {
        items = result.item;
        parentId = -1;
    }

    if (typeof (items) != 'undefined' && items.length > 0) {
        var html = CreateHtmlTile(items, parentId, titulo);
        showTiles(html);
        SetButtonIcon("#butVolver", "fa fa-arrow-left");
    }
}


function SetButtonIcon(id, icon) {

    if ($(id).length) {

        var button = $(id).data("kendoButton");

        if (!button) {

            $(id).kendoButton({
                spriteCssClass: icon
            });
        }
    }
}


function GetCurrentTileItem(id) {

    let param = {
        "item": null,
        "parentId": 0,
        "titulo": "Opciones"
    };

    if (id == 0) {
        param.item = mvarArbolMenu;
    }
    else {
        BuscarItemPorId_Nodos(mvarArbolMenu, id, param, "", 0);
    }

    return param;
}


function BuscarItemPorId_Nodos(items, id, param, currentPath, parentId) {

    for (let i = 0; i < items.length; i++) {

        let elem = items[i];

        if (elem.id == id) {

            if (currentPath.length > 0) {
                currentPath += " - ";
            }

            param.parentId = parentId;

            currentPath += elem.text.trim();

            param.titulo = currentPath;

            param.item = elem;

            break;
        }
        else if (typeof (elem.items) != 'undefined' && elem.items.length > 0) {

            let pathAnt = currentPath;

            if (currentPath.length > 0) {
                currentPath += " - ";
            }

            currentPath += elem.text.trim();

            BuscarItemPorId_Nodos(elem.items, id, param, currentPath, elem.id);

            currentPath = pathAnt;

            if (param.item != null) {
                break;
            }
        }
    }
}


function showTiles(html) {

    var container = document.getElementById("tilescontainer");

    container.innerHTML = html;

    AsignarTileEvents();
}


function AsignarTileEvents() {

    $(".tileButton").hover(function () {

        let tbid = $(this)[0].id;

        if (tbid.length > 0) {

            let partes = tbid.split("_");

            let nroid = partes[1];

            let butid = "#tb_but_" + nroid;

            if ($(butid).length) {
                $(butid).show();
            }
        }

    }, function () {

        let tbid = $(this)[0].id;

        if (tbid.length > 0) {

            let partes = tbid.split("_");

            let nroid = partes[1];

            let butid = "#tb_but_" + nroid;

            let drpid = "tb_drp_" + nroid;

            let elem = document.getElementById(drpid);

            if (elem) {

                let drpclassList = elem.classList;

                if (drpclassList) {

                    if (!drpclassList.contains('show')) {
                        $(butid).hide();
                    }
                }
            }
        }
    });

    $(".menuitem").hover(function () {

        $(this).css("background-color", "darkgray");

    }, function () {

        $(this).css("background-color", mvarContextBackground);
    });
}


function OpenTileMenu(id) {

    var dropdowns = document.getElementsByClassName("dropdown-content");

    for (i = 0; i < dropdowns.length; i++) {
        var openDropdown = dropdowns[i];
        if (openDropdown.classList.contains('show')) {
            openDropdown.classList.remove('show');
        }
    }

    let butid = "tb_but_" + id.toString();

    $(".contextflatbutton").each(function () {

        let elemId = $(this)[0].id;

        if (elemId != butid) {
            $(this).hide();
        }
    });

    let drpid = "tb_drp_" + id.toString();

    document.getElementById(drpid).classList.toggle("show");
}


function AnclarFavorito(id) {

    var elements = $("#sortable").children();

    if (elements.length >= 15) {
        MensInfo("Como maximo puede haber 15 favoritos");
        return;
    }

    var enco = false;

    var searchid = 'tb_elem_' + id.toString();

    for (let i = 0; i < elements.length; i++) {

        var elemid = elements[i].id;

        if (elemid == searchid) {
            enco = true;
            break;
        }
    }

    if (enco) {
        MensInfo("Esta opcion ya existe como favorito");
    }
    else {
        AgregarFavorito(id);
    }
}


function AgregarFavorito(id) {

    var result = GetCurrentTileItem(id);

    var item = result.item;

    var param = {
        "text": item.text.trim(),
        "href": item.href.trim(),
        "cssclass": item.spriteCssClass.trim()
    }

    MSExecuteOnServer('/Areas/Permisos/AgregarFavorito', param);

    var newElement = document.createElement("li");

    newElement.setAttribute('id', 'tb_elem_' + item.id.toString());

    newElement.setAttribute('class', "toolbutton ui-sortable-handle");

    newElement.setAttribute('type', "button");

    newElement.setAttribute('data-role', "button");

    newElement.setAttribute('title', item.text.trim());

    newElement.setAttribute('data-title', item.text.trim());

    newElement.setAttribute('onclick', "InvokeURL('" + ProcesarURL(item.href.trim()) + "')");

    newElement.innerHTML = '<span><i class="' + item.spriteCssClass.trim() + ' fa-lg" aria-hidden="true"></i></span>';

    var parent = document.getElementById("sortable");

    parent.appendChild(newElement);

    $('#tb_elem_' + item.id.toString()).hover(function () {

        $(this).css("background", mvarToolbarHighlight);

    }, function () {

        $(this).css("background", "transparent");
    });

}
