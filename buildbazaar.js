function showPopup() {
	document.getElementById("signup-form").style.display = "block";
}

function hidePopup() {
	document.getElementById("signup-form").style.display = "none";
}

$(document).ready(function() {
    $('#signup-form').submit(function(event) {
        // Prevent the default form submission
        event.preventDefault();
  
        // Get the form data
        var formData = $(this).serialize();
  
        // Send the AJAX request
        $.ajax({
        type: 'POST',
        url: 'signup.php',
        data: formData,
        success: function(response) {
          // Handle the successful response
          alert('User created successfully!');
        },
        error: function(xhr, status, error) {
          // Handle the error response
          alert('An error occurred while creating the user: ' + error);
        }
        });
    });
});
  