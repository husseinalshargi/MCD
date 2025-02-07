
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url:'/home/getall'},
        "columns": [
            { data: 'fileName', "width": "15%" },
            {
                data: 'fileName', "width": "15%", render: function (data, type, row) { // all data returned will be passed in this function
                    return data.replace(/\.[^/.]+$/, ""); //to remove file extension using Regular Expression
                }
            },
            {
                data: 'fileName', "width": "15%", render: function (data, type, row) { // all data returned will be passed in this function
                    return data.replace(/\.[^/.]+$/, ""); //to remove file extension using Regular Expression
                }
            },
            { data: 'uploadDate', "width": "15%" },
            { data: 'updateDate', "width": "15%" },
            { data: 'category.categoryName', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //the data is the id
                    return `<div class="w-75 btn-group" role="group">
                    <a href="" class="btn btn-info mx-2"> <i class="bi bi-info-circle"></i> Info </a>
                    </div>
                    `

                    //here it will return a button for the info
                },
                "width": "15%"
            },
        ]
    });
}

