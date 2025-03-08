
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#userManagementTable').DataTable({
        "ajax": { url: '/Employee/user/getall' },
        "columns": [
            { data: 'firstName', "width": "15%" },
            { data: 'lastName', "width": "15%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "20%" },
            { data: 'role', "width": "10%" },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `
                     <div class="text-center">
                        <button onclick="changeUserRole('${data}')" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                            <i class="bi bi-pencil-square"></i> Change Role
                        </button>
                    </div>
                    `;
                },
                "width": "10%"
            },
            {
                data: { id: "id", lockoutEnd: "lockoutEnd" }, //to place more than one field in the same col
                render: function (data) {
                    var today = new Date().getTime(); //to get today's date
                    var lockout = new Date(data.lockoutEnd).getTime(); //get lockout date and convert it to date obj

                    if (lockout > today) { //means that the user is currently locked
                        return `
                        <div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white mb-1" style="cursor:pointer; width:150px;">
                                <i class="bi bi-lock-fill"></i> Lock
                            </a>
                        </div>
                    `
                    }
                    else {
                        return `
                        <div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white mb-1" style="cursor:pointer; width:150px;">
                                <i class="bi bi-unlock-fill"></i> Unclock
                            </a>
                        </div>
                    `
                    }


                },
                "width": "10%"
            }
        ]
    });
}



function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Employee/user/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) { //if the user un/locked
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
} 


function changeUserRole(userId) { //to use the method in the controller to change the role by using post (not get as at isn't secure)
    $.ajax({
        type: "POST", 
        url: `/Employee/user/ChangeRole?userId=${userId}`, //to use the method
        contentType: "application/json",
        success: function (response) {
            toastr.success("User role changed successfully!"); //to show a message to the user
            dataTable.ajax.reload();
        }
    });
}

