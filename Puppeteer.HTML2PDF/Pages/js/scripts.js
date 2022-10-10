$(document).ready(function () {
    particlesJS("particles-js", {
        "particles": {
            "number": {
                "value": 120,
                "density": {
                    "enable": true,
                    "value_area": 800
                }
            },
            "color": {
                "value": "#ffffff"
            },
            "opacity": {
                "value": 0.5,
                "random": false,
                "anim": {
                    "enable": false,
                    "speed": 1,
                    "opacity_min": 0.1,
                    "sync": false
                }
            },
            "size": {
                "value": 3,
                "random": true,
                "anim": {
                    "enable": false,
                    "speed": 40,
                    "size_min": 0.1,
                    "sync": false
                }
            },
            "line_linked": {
                "enable": true,
                "distance": 150,
                "color": "#ffffff",
                "opacity": 0.4,
                "width": 1
            },
            "move": {
                "enable": true,
                "speed": 6,
                "direction": "none",
                "random": false,
                "straight": false,
                "out_mode": "out",
                "bounce": false,
                "attract": {
                    "enable": false,
                    "rotateX": 600,
                    "rotateY": 1200
                }
            }
        },
        "retina_detect": true
    });

    $("#urlInput").inputmask({
        mask: "https{0,1}://*{1,1024}",
        definitions: {
            '*': {
                validator: "[0-9A-Za-z_.-?&-]",
                casing: "lower"
            }
        }
    });
    
    let progress =  $(".progress-bar");
    if (progress.length > 0){
        setTimeout(UpdateTaskData, 50);
    }


});

function UpdateTaskData() {

    let progress =  $(".progress-bar");
    
    let header = $("#headerLabel")[0];
    let pdfButton = $("#pdfButton")[0];
    pdfButton.hidden = true;

    let paramList = new URLSearchParams(window.location.search);
    let id = paramList.get("id");

    if (id==="")
    {
        window.location.replace("index.html");
        return;
    }


    $.ajax({
        url: "/api/ConvertToPdf",
        method: "GET",
        data: {
            id: id
        },
        success: function (res) {
            
            if (res.state === 2){
                progress[0].style.width = '100%';
                progress[0].classList.replace('bg-primary', 'bg-success')
                header.innerText = 'Task for converting "' + res.fromUrl + '" is done'
                
                pdfButton.href = "" + res.outPdfFileFullName;
                
                pdfButton.hidden = false;

                return;
            }

            header.innerText = 'Task for converting ' + res.fromUrl + 'in progress'
            progress[0].style.width = '50%';
            
            setTimeout(UpdateTaskData, 5000);
        },
        error: function (req) {
            console.log('not ok \n' + req.responseText);

            if (req.status === 404) {
                header.innerText = 'Cant found task with id ' + id
                progress[0].style.width = '0%';
            }

        }
    });


}


function SendNewTask() {

    let urlString = $('#urlInput')[0].value;

    if (urlString === "") {
        alert("Empty url string")
        return;
    }


    $.ajax({
        url: "/api/ConvertToPdf",
        contentType: "application/x-www-form-urlencoded",
        method: "POST",
        data: {
            url: urlString
        },
        success: function (res) {

            console.log('ok \n' + res);
            window.location.replace("status.html?id=" + res.id);
        },
        error: function (req) {
            
            
            console.log('not ok \n' + req.responseText);

        }
    });

}

