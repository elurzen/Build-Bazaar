function showSignupForm() {
    $("#new-user-form")[0].reset();    
	document.getElementById("signup-form").style.display = "block";
}

function hideSignupForm() {
	document.getElementById("signup-form").style.display = "none";
}

$(document).ready(function () {
    var userToken = localStorage.getItem("token");
    if (userToken == null && window.location.pathname != "/") {
        window.location.href = "/";
    }
    // Check if on index page and for existing session cookie
    if (window.location.pathname === "/" && userToken != null) {
        $.ajax({
            type: 'POST',
            url: '/Home/WebValidateToken',
            headers: {
                Authorization: userToken
            },
            success: function (result) {
                if (result.success) {
                    // login successful, redirect to builds page
                    window.location.href = '/Builds';
                } else {
                    //it's fine
                }
            },
            error: function (xhr, status, error) {
                // ajax error, show error message
                //$('#login-error').text('An error occurred while logging in.');
                alert("Error: " + error);
            }
        });
    }

    $('#login-form').submit(function (event) {
        event.preventDefault(); // prevent form from submitting

        // get form data
        var formData = {
            username: $('#username').val(),
            password: $('#password').val()
        };

        // send ajax request
        $.ajax({
            type: 'POST',
            url: '/Home/Login',
            data: formData,
            success: function (result) {
                if (result.success) {
                    // login successful, redirect to builds page
                    localStorage.setItem('token', result.token);
                    window.location.href = '/Builds/';
                } else {
                    // login failed, show error message
                    //$('#login-error').text(result.message);
                    alert("Error: " + result.message);
                }
            },
            error: function (xhr, status, error) {
                // ajax error, show error message
                //$('#login-error').text('An error occurred while logging in.');
                alert("Error: " + error);
            }
        });
    });

    // Listen for "keypress" event on input fields
    $('input').on('keypress', function (event) {
        // Check if "enter" key was pressed
        if (event.which === 13) {
            // Check if sign-up button is visible
            if ($('#signup-form').is(':visible')) {
                // If so, trigger "click" event on sign-up button
                $('#signup-submit').trigger('click');
            } else {
                // Otherwise, trigger "click" event on login button
                $('#login-button').trigger('click');
            }
        }
    });

    $('#signup-form').submit(function (event) {
        event.preventDefault(); // prevent the form from submitting normally

        // get the form data
        var formData = {
            'username': $('#new-username').val(),
            'email': $('#new-email').val(),
            'password': $('#new-password').val()
        };

        // send an AJAX POST request to the CreateUser action method
        $.ajax({
            type: 'POST',
            url: '/Home/CreateUser',
            data: formData,
            dataType: 'json',
            encode: true
        })
            .done(function (data) {
                if (data.success) {
                    alert("User Created Successfully!")
                    hideSignupForm();
                }
                else {
                    alert("Error: " + data.message);
                }
            })
            .fail(function (xhr, status, error) {
                // handle the AJAX request failure
                alert("Error: " + error);
            });
    });
});
