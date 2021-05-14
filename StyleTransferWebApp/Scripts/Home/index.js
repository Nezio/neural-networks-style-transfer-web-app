
Dropzone.autoDiscover = false;

window.onload = function () {
    if (responseMessage) {
        alert(responseMessage);
    }

    InitializeDropzoneSettings();
    
};

function InitializeDropzoneSettings() {
    // create global dropzone options object
    var dropzoneGlobalOptions = {
        maxFiles: 1,
        maxFilesize: 10, // MB
        addRemoveLinks: true,
        dictRemoveFile: "Remove",
        acceptedFiles: ".jpg,.jpeg,.png,.bmp",
        // dropzone events
        init: function () {
            this.on("addedfile", function (file) {
                // remove old file if another is uploaded
                if (this.files[1] != null) {
                    this.removeFile(this.files[0]);
                }
            });
        }
    };

    // create options object for the content dropzone
    var dropzoneContentOptions = {
        dictDefaultMessage: 'Drop the content image here or click to select one from your computer.'
    };

    // create options object for the style dropzone
    var dropzoneStyleOptions = {
        dictDefaultMessage: 'Drop the style image here or click to select one from your computer.'
    };

    // append global options to the content dropzone
    for (var key in dropzoneGlobalOptions) {
        dropzoneContentOptions[key] = dropzoneGlobalOptions[key];
    }

    // append global options to the style dropzone
    for (var key in dropzoneGlobalOptions) {
        dropzoneStyleOptions[key] = dropzoneGlobalOptions[key];
    }

    // create and attach the content dropzone
    var dropzoneContentElement = document.querySelector('#dropzoneContent');
    var dropzoneContent = new Dropzone(dropzoneContentElement, dropzoneContentOptions);

    // create and attach the style dropzone
    var dropzoneStyleElement = document.querySelector('#dropzoneStyle');
    var dropzoneStyle = new Dropzone(dropzoneStyleElement, dropzoneStyleOptions);
}