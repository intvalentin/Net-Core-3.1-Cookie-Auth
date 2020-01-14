$(document).ready(function () {
    $(document).on("keyup", "#passwordConfirm", function (e) {
        if ($("#password").val() != $(this).val() || /\s/.test($("#password").val()) || $("#password").val() == "" || (/[\s]/g).test($(this).val())) {
            $(this).removeClass("is-valid").addClass("is-invalid");
            $("#submitButton").attr("type", "button");
        } else {
            $(this).removeClass("is-invalid").addClass("is-valid");
            $("#password").removeClass("is-invalid").addClass("is-valid");
           
            $("#submitButton").attr("type", "Sumbit");
        }
    });
    $(document).on("keyup", "#password", function (e) {
        if ($("#passwordConfirm").val() != $(this).val() || /\s/.test($("#passwordConfirm").val()) || $("#passwordConfirm").val() == "" || (/[\s]/g).test($(this).val())) {
            $(this).removeClass("is-valid").addClass("is-invalid");
            $("#submitButton").attr("type", "button");
        } else {
            $(this).removeClass("is-invalid").addClass("is-valid");
            $("#passwordConfirm").removeClass("is-invalid").addClass("is-valid");
           
            $("#submitButton").attr("type", "Sumbit");
        }
    });
});