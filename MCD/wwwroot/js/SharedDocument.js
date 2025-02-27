
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#sharedDocsTable').DataTable({
        "ajax": { url: '/home/getallshareddocuments' },
        "columns": [
        ]
    });
}

//function openDocument(userId, fileName) { // a function to call inside render for getting html in the table
//    $.ajax({
//        url: `/home/GetDocument?userId=${userId}&fileName=${fileName}`,
//        type: 'GET',
//        success: function (response) {
//            if (response && response.fileUrl) {
//                window.open(response.fileUrl, '_blank'); // Open in new tab -> _self to open in the same page
//            } else {
//                alert("Error: Unable to fetch the document.");
//            }
//        },
//        error: function (xhr) {
//            console.log("AJAX Error:", xhr.responseText);
//            alert("Error: Could not open the document.");
//        }
//    });
//}