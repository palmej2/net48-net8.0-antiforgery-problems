$(document).ready(function () {
    $.ajax({
        url: '/home/testPost',  // The URL to send the request to
        type: 'POST',           // The HTTP method (POST in this case)
        headers: {
            'xss-token': $('input[name="__RequestVerificationToken"]').val() // Custom header with value "blah"
        },
        success: function (response) {

            document.getElementById("title").innerHTML = "WORKED!";
        },
        error: function (xhr, status, error) {

            document.getElementById("title").innerHTML = "FAILED!";

        }
    });
});