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
                        populateBuildUrls();
                        initUrlButtons();
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
    //document.getElementById("add-build-form").style.display = "block";
    $("#add-build-form").show();
}

function hideAddBuildForm() {
    //document.getElementById("add-build-form").style.display = "none"
    $("#add-build-form").hide();
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
    //document.getElementById("upload-reference-image-form").style.display = "block";
    $("#upload-reference-image-form").show();
}

function hideUploadReferenceImageForm() {
    //document.getElementById("upload-reference-image-form").style.display = "none"
    $("#upload-reference-image-form").hide();
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
            var list = $('#sidebarBuildList');

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
                    populateBuildUrls();
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
                        document.getElementById("notes").innerText = fileContents;
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

function populateBuildUrls() {
    var userToken = localStorage.getItem("token");
    if (localStorage.buildID != null) {
        $.ajax({
            type: 'POST',
            url: '/Home/GetBuildUrls',
            headers: {
                Authorization: userToken
            },
            data: { buildID: localStorage.buildID },
            success: function (result) {
                var urlSelect = $('#urlSelect');
                urlSelect.empty(); // Clear any existing options

                // Add the "Create New" option at the top
                var createNewOption = $('<option>')
                    .val(-1)
                    .text('Create New')
                    .data({
                        'url': '',
                        'name': '',
                        'id': null
                    });
                urlSelect.append(createNewOption);

                // Iterate through the retrieved Build URLs and add them as options
                result.buildUrls.forEach(function (buildUrl) {
                    var option = $('<option>')
                        .val(buildUrl.buildUrlID)
                        .text(buildUrl.buildUrlName)
                        .data({
                            'url': buildUrl.buildUrl,
                            'name': buildUrl.buildUrlName,
                            'id': buildUrl.buildUrlID
                        });
                    urlSelect.append(option);
                });
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
            var list = $('#referenceImageList');
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
    var editButton = $('#notes-edit-button');
    var cancelButton = $('#notes-cancel-button');
    var saveButton = $('#notes-save-button');
    //var uploadButton = $('#upload-image-button');
    //var buildInfo = $('#build-info');
    var notes = $('#notes');
    var noteContent;

    // Edit button click event
    editButton.on('click', function () {
        // Hide the edit button, show the cancel and save buttons
        editButton.hide();
        cancelButton.show();
        saveButton.show();

        noteContent = notes.innerText;
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
        notes.innerText = noteContent;
    });

    // Save button click event
    saveButton.on('click', function () {

        var userToken = localStorage.getItem("token");
        if (userToken == null) {
            window.location.href = "/";
        }
        else {

            $.ajax({
                type: 'POST',
                url: '/Home/SetNote',
                headers: {
                    Authorization: userToken
                },
                data: {
                    buildID: localStorage.buildID,
                    noteContent: notes[0].innerText //.text() doesnt preserve new line chars .html() throws security error
                },
                success: function (result) {
                    if (result.success) {

                        // Show the edit button, hide the cancel and save buttons
                        editButton.show();
                        cancelButton.hide();
                        saveButton.hide();

                        // Disable editing of the notes section
                        notes.attr('contenteditable', 'false');

                    } else {
                        alert("Error: " + error)
                    }
                },
                error: function (xhr, status, error) {
                    // ajax error, show error message
                    //$('#login-error').text('An error occurred while logging in.');
                    alert("Error: " + xhr.responseText);
                }
            });
        }
        // Perform save operation here
        // You can retrieve the updated notes content using: notes.text()
        // You can then send the updated content to the server for saving
    });
}

function initUrlButtons() {

    // Get the buttons and build info section
    var editButton = $('#url-edit-button');
    var cancelButton = $('#url-cancel-button');
    var saveButton = $('#url-save-button');
    var deleteButton = $('#url-delete-button');
    var loadButton = $('#url-load-button'); 
    var nameTextBox = $('#txt-url-name');
    var urlTextBox = $('#txt-url-url');
    

    // Edit button click event
    editButton.on('click', function () {
        // Hide the edit button, show the cancel and save buttons
        editingUrl(true);

        var selectedOption = $('#urlSelect option:selected');

        if (selectedOption.data().id == null)
            deleteButton.hide();

        // Populate the textboxes with data from the selected option
        nameTextBox.val(selectedOption.data('name'));
        urlTextBox.val(selectedOption.data('url'));

        // Store the initial content of the textboxes for later use
        nameTextBoxContent = nameTextBox.val();
        urlTextBoxContent = urlTextBox.val();

        // Set focus on the name textbox
        nameTextBox.focus();
    });

    // Cancel button click event
    cancelButton.on('click', function () {
        // Show the edit button, hide the cancel and save buttons
        editingUrl(false);        
    });

    // Save button click event
    saveButton.on('click', function () {
        var selectedOption = $('#urlSelect option:selected').data();

        // Extract the necessary data
        var buildUrlID = selectedOption.id;
        var buildUrl = $('#txt-url-url').val();
        var buildUrlName = $('#txt-url-name').val();
        var userToken = localStorage.getItem("token");

        // Perform the save operation
        if (localStorage.buildID != null) {
            $.ajax({
                type: 'POST',
                url: '/Home/UpdateBuildUrl',
                headers: {
                    Authorization: userToken
                },
                data: {
                    buildUrlID: buildUrlID,
                    buildID: localStorage.buildID,
                    buildUrl: buildUrl,
                    buildUrlName: buildUrlName
                },
                success: function (result) {
                    if (result.success) {
                        editingUrl(false);

                        populateBuildUrls();
                    } else {
                        // Handle failure
                        // For example, display an error message
                    }
                },
                error: function (xhr, status, error) {
                    // Handle error
                    alert("Error: " + error);
                }
            });
        }
    });

    deleteButton.on('click', function () {
        var selectedOption = $('#urlSelect option:selected').data();

        // Extract the buildUrlID and name
        var buildUrlID = selectedOption.id;
        var buildUrlName = selectedOption.name;
        var userToken = localStorage.getItem("token");

        // Display confirmation dialog
        var confirmation = confirm("Are you sure you want to delete " + buildUrlName + "?");
        if (confirmation) {
            // Perform the delete operation
            if (localStorage.buildID != null) {
                $.ajax({
                    type: 'POST',
                    url: '/Home/DeleteBuildUrl',
                    headers: {
                        Authorization: userToken
                    },
                    data: {
                        buildUrlID: buildUrlID
                    },
                    success: function (result) {
                        if (result.success) {
  

                            // If the deletion is successful, refresh the list of build URLs
                            populateBuildUrls();
                        } else {
                            // Handle failure
                            // For example, display an error message
                        }
                    },
                    error: function (xhr, status, error) {
                        // Handle error
                        alert("Error: " + error);
                    }
                });
            }
        }
    });

    // Load button click event
    loadButton.on('click', function () {
        var selectedUrl = $('#urlSelect option:selected').data('url'); // Get the selected URL from the dropdown
        $('#tree').attr('src', selectedUrl); // Set the URL of the iframe
    });
}

function editingUrl(editing) {
    // Get the buttons and build info section
    var urlSelect = $('#urlSelect');
    var editButton = $('#url-edit-button');
    var cancelButton = $('#url-cancel-button');
    var saveButton = $('#url-save-button');
    var deleteButton = $('#url-delete-button');
    var loadButton = $('#url-load-button');
    var nameLabel = $('#lbl-url-name');
    var nameTextBox = $('#txt-url-name');
    var urlLabel = $('#lbl-url-url');
    var urlTextBox = $('#txt-url-url');

    if (editing) {
        // Show the viewing buttons button, hide editing buttons
        urlSelect.hide();
        editButton.hide();
        loadButton.hide();
        deleteButton.show();
        cancelButton.show();
        saveButton.show();
        nameLabel.show();
        nameTextBox.show();
        urlLabel.show();
        urlTextBox.show();

        // Disable editing of the url section
        nameTextBox.prop("readonly", false);
        urlTextBox.prop("readonly", false);
    }
    else {
        // Show the viewing buttons button, hide editing buttons
        urlSelect.show();
        editButton.show();
        loadButton.show();
        deleteButton.hide();
        cancelButton.hide();
        saveButton.hide();
        nameLabel.hide();
        nameTextBox.hide();
        urlLabel.hide();
        urlTextBox.hide();

        // Disable editing of the url section
        nameTextBox.prop("readonly", true);
        urlTextBox.prop("readonly", true);
    }


}
function signOut() {
    localStorage.removeItem("token");
    window.location.href = "/";
}



$(document).ready(function () {
    validateUserToken();
    initBuildForm();
    initUploadReferenceImageForm();   

    // JavaScript code to handle resizing of columns
    //$('.resizable').on('mousedown', function (e) {
    //    var column = $(this);
    //    var startX = e.pageX;
    //    var startWidth = column.width();

    //    $(document).on('mousemove', function (e) {
    //        var newWidth = startWidth + (e.pageX - startX);
    //        column.width(newWidth);
    //    });

    //    $(document).on('mouseup', function () {
    //        $(document).off('mousemove');
    //    });
    //});
});