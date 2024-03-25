function validateUserToken() {
    var userToken = localStorage.getItem("token");
    if (userToken == null) {
        window.location.href = "/";
    }
    if (userToken != null) {

        $.ajax({
            type: 'POST',
            url: '/Home/WebValidateToken',
            headers: {
                Authorization: userToken
            },
            success: function (result) {
                if (result.success) {

                    //populate build list sidebar
                    populateBuilds();

                    //populate notes section    
                    var firstLi = $('#sidebar li:first');
                    var buildID = firstLi.attr('data-buildID');
                    if (buildID != null) {
                        populateNote();
                        populateReferenceImages()
                        initNotesButtons();
                    }

                } else {
                    window.location.href = '/';
                }
            },
            error: function (xhr, status, error) {
                // ajax error, show error message
                //$('#login-error').text('An error occurred while logging in.');
                alert("Error: " + error);
            }
        });
    }
}

function initBuildForm() {
    $('#add-build-form').submit(function (event) {
        event.preventDefault(); // prevent form from submitting

        var userToken = localStorage.getItem("token");
        if (userToken == null) {
            window.location.href = "/";
        }
        if (userToken != null) {
            var name = $("#build-name").val();
            var image = $("#build-image")[0].files[0];
            var formData = new FormData();

            formData.append("buildName", name);
            formData.append("image", image);

            $.ajax({
                url: "/Home/CreateBuild",
                headers: {
                    Authorization: userToken
                },
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    // Hide the form and refresh the page to show the new build
                    hideAddBuildForm();
                    window.location.reload();
                },
                error: function (xhr, status, error) {
                    // Show an error message if the request failed
                    alert("Failed to create build: " + xhr.responseText);
                }
            });
        }
    });
}

function showAddBuildForm() {
    document.getElementById("add-build-form").style.display = "block";
}

function hideAddBuildForm() {
    document.getElementById("add-build-form").style.display = "none"
}

function initUploadReferenceImageForm() {
    $('#upload-reference-image-form').submit(function (event) {
        event.preventDefault(); // prevent form from submitting

        var userToken = localStorage.getItem("token");
        if (userToken == null) {
            window.location.href = "/";
        }
        if (userToken != null) {
            var image = $("#reference-image")[0].files[0];
            var formData = new FormData();

            formData.append("image", image);
            formData.append("buildID", localStorage.buildID);

            $.ajax({
                url: "/Home/UploadReferenceImage",
                headers: {
                    Authorization: userToken
                },
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    // Hide the form and refresh the page to show the new build
                    hideUploadReferenceImageForm();
                    window.location.reload();
                },
                error: function (xhr, status, error) {
                    // Show an error message if the request failed
                    alert("Failed to create build: " + xhr.responseText);
                }
            });
        }
    });
}

function showUploadReferenceImageForm() {
    document.getElementById("upload-reference-image-form").style.display = "block";
}

function hideUploadReferenceImageForm() {
    document.getElementById("upload-reference-image-form").style.display = "none"
}

function populateBuilds() {
    var userToken = localStorage.getItem("token");
    $.ajax({
        type: 'POST',
        url: '/Home/GetBuilds',
        async: false,
        headers: {
            Authorization: userToken
        },
        success: function (result) {
            // Get the list container element
            var list = $('#sidebar');

            // Clear the list 
            //list.empty();
            //list.append($('h2').text('Builds'));                            
            //This seems to be working with it commented out, but I'll leave it here
            //it was putting multiple "Builds" in the list, but I think it was doing it for every
            //h2 header element on the page, and this started happening after I added the columns

            // Iterate through the rows returned by the database
            for (let i = 0; i < result.builds.length; i++) {
                record = result.builds[i];
                // Create an <li> element for the row
                var li = $('<li>').addClass('build-item').attr('data-buildID', record.buildID);
                li.on('click', function () {
                    var buildID = $(this).attr('data-buildID');
                    localStorage.setItem('buildID', buildID);
                    ///alert("clicked buildID: " + buildID);
                    populateNote();
                    populateReferenceImages();
                });
                //var splits = record.imageName.split('.');
                //var extension = splits[splits.length - 1];
                //var imagePath = "../UploadedFiles/" + record.imageID + '.' + extension;
                var imagePath = record.filePath;
                // Create an <img> element for the image
                var img = $('<img>').attr('src', imagePath).addClass('thumbnail');



                // Create a <h3> element for the build name
                var h4 = $('<h4>').text(record.buildName);

                // Append the <img> and <h3> elements to the <li> element
                li.append(img).append(h4);

                // Append the <li> element to the list
                list.append(li);
            }
            var newBuildLink = $('<a>').attr('href', '#').text('+New Build').click(showAddBuildForm);
            list.append($('<li>').append(newBuildLink)).css("list-style-type", "none");
        },
        error: function (xhr, status, error) {
            // ajax error, show error message
            //$('#login-error').text('An error occurred while logging in.');
            alert("Error: " + error);
        }
    });
}

function populateNote() {
    var userToken = localStorage.getItem("token");
    if (localStorage.buildID != null) {
        $.ajax({
            type: 'POST',
            url: '/Home/GetNote',
            headers: {
                Authorization: userToken
            },
            data: { buildID: localStorage.buildID },
            success: function (result) {

                //alert(result.note.filePath);

                var xhr = new XMLHttpRequest();
                xhr.open("GET", result.note.filePath + "?_=" + new Date().getTime(), true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4 && xhr.status === 200) {
                        var fileContents = xhr.responseText;
                        document.getElementById("notes").textContent = fileContents;
                    }
                };
                xhr.send();
            },
            error: function (xhr, status, error) {
                // ajax error, show error message
                //$('#login-error').text('An error occurred while logging in.');
                alert("Error: " + error);
            }
        });
    }
}

function populateReferenceImages() {

    var userToken = localStorage.getItem("token");
    $.ajax({
        type: 'POST',
        url: '/Home/GetReferenceImages',
        async: false,
        headers: {
            Authorization: userToken
        },
        data: {buildID: localStorage.buildID},
        success: function (result) {
            // Get the list container element
            var list = $('#referenceImages');
            list.empty();

            // Iterate through the rows returned by the database
            for (let i = 0; i < result.images.length; i++) {
                record = result.images[i];
                // Create an <li> element for the row
                var li = $('<li>').addClass('build-item').attr('data-buildID', record.buildID);
                li.on('click', function () {
                    alert("clicked image");
                });

                var imagePath = record.filePath;
                // Create an <img> element for the image
                var img = $('<img>').attr('src', imagePath).addClass('referenceImage');

                // Append the <img> and <h3> elements to the <li> element
                li.append(img)

                // Append the <li> element to the list
                list.append(li);
            }
        },
        error: function (xhr, status, error) {
            // ajax error, show error message
            //$('#login-error').text('An error occurred while logging in.');
            alert("Error: " + error);
        }
    });
}


function initNotesButtons() {

    // Get the buttons and build info section
    var editButton = $('#edit-button');
    var cancelButton = $('#cancel-button');
    var saveButton = $('#save-button');
    var uploadButton = $('#upload-image-button');
    var buildInfo = $('#build-info');
    var notes = $('#notes');
    var noteContent;

    // Edit button click event
    editButton.on('click', function () {
        // Hide the edit button, show the cancel and save buttons
        editButton.hide();
        cancelButton.show();
        saveButton.show();

        noteContent = notes.text();
        // Enable editing of the notes section
        notes.attr('contenteditable', 'true');
        notes.focus();
    });

    // Cancel button click event
    cancelButton.on('click', function () {
        // Show the edit button, hide the cancel and save buttons
        editButton.show();
        cancelButton.hide();
        saveButton.hide();

        // Disable editing of the notes section and reset its content
        notes.attr('contenteditable', 'false');
        notes.text(noteContent);
    });

    // Save button click event
    saveButton.on('click', function () {

        var userToken = localStorage.getItem("token");
        if (userToken == null) {
            window.location.href = "/";
        }
        if (userToken != null) {

            $.ajax({
                type: 'POST',
                url: '/Home/SetNote',
                headers: {
                    Authorization: userToken
                },
                data: { buildID: localStorage.buildID, noteContent: notes.text() },
                success: function (result) {
                    if (result.success) {

                        // Show the edit button, hide the cancel and save buttons
                        editButton.show();
                        cancelButton.hide();
                        saveButton.hide();

                        // Disable editing of the notes section
                        notes.attr('contenteditable', 'false');

                    } else {
                        
                    }
                },
                error: function (xhr, status, error) {
                    // ajax error, show error message
                    //$('#login-error').text('An error occurred while logging in.');
                    alert("Error: " + error);
                }
            });
        }
        // Perform save operation here
        // You can retrieve the updated notes content using: notes.text()
        // You can then send the updated content to the server for saving
    });
}

$(document).ready(function () {
    validateUserToken();
    initBuildForm();
    initUploadReferenceImageForm();    
});