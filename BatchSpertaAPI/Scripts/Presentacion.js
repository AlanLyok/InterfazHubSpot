

$(document).ready(function () {

    AjustarAlto();

    window.addEventListener("resize", function () {
        AjustarAlto();
    });
});



function AjustarAlto() {

    if (IsMobile()) {
        $("#containerCarousel").css("display", "none");
    }
    else {

        var h = window.innerHeight;

        h = h - 85;
    
        if (h < 540) {
            h = 540;
        }
    
        if (h > 850) {
            h = 850;
        }

        $("#containerCarousel").css("display", "table");
    
        $("#containerCarousel").height(h);
    }

  
}
