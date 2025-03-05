
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#sharedDocumentsTable').DataTable({
        "ajax": { url: '/document/getallshareddocuments' },
        "columns": [
            {
                data: 'document.fileName',
                render: function (data, type, row) {
                    //row to get other data from columns
                    let userId = row.sharedFromId; // in order to get the user id to get the path
                    return `<div>
                                <button onclick="openDocument('${userId}', '${data}')" class="btn btn-danger">
                                    <i class="bi bi-file-earmark"></i> Access File
                                </button>
                            </div>`;
                },
                "width": "20%"
            },
            { data: 'document.fileName', "width": "20%" },
            { data: 'document.title', "width": "20%" },
            { data: 'document.applicationUser.email', "width": "15%" },
            {
                data: 'sharedAt', "width": "15%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }

            }
        ]
    });
}

function openDocument(userId, fileName) { // a function to call inside render for getting html in the table
    $.ajax({
        url: `/home/GetDocument?userId=${userId}&fileName=${fileName}`, //avoid redundant code by using the same method
        type: 'GET',
        success: function (response) {
            if (response && response.fileUrl) {
                window.open(response.fileUrl, '_blank'); // Open in new tab -> _self to open in the same page
            } else {
                alert("Error: Unable to fetch the document.");
            }
        },
        error: function (xhr) {
            console.log("AJAX Error:", xhr.responseText);
            alert("Error: Could not open the document.");
        }
    });
}