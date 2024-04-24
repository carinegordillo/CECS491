

function sendOTP() {
    var userIdentity = document.getElementById("userIdentity").value;
    console.log(userIdentity);

    $.ajax({
        url: 'http://localhost:5270/api/auth/sendOTP',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ userIdentity: userIdentity }),
        success: function (response) {
            document.getElementById("sendOTPSection").style.display = "none";
            document.getElementById("enterOTPSection").style.display = "block";
        },
        error: function (xhr, status, error) {
            alert('Error sending verification code.');
        }
    });
}
function authenticateUser() {
    var otp = document.getElementById("otp").value;
    var userIdentity = document.getElementById("userIdentity").value;

    $.ajax({
        url: 'http://localhost:5270/api/auth/authenticate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ userIdentity: userIdentity, proof: otp }),
        success: function (response) {
            var accessToken = response.accessToken;
            var idToken = response.idToken;
            sessionStorage.setItem('accessToken', accessToken);
            sessionStorage.setItem('idToken', idToken);
            document.getElementById("enterOTPSection").style.display = "none";
            document.getElementById("successResult").style.display = "none";

            var accessToken = sessionStorage.getItem('accessToken');

            $.ajax({
                url: 'http://localhost:5270/api/auth/getRole',
                type: 'POST',
                contentType: 'application/json',
                data: accessToken,
                success: function (response) {
                    if (response === "2" || response === "3") {
                        document.getElementById("homepageManager").style.display = "block";
                        document.getElementById("homepageGen").style.display = "none";
                    }
                    else {
                        document.getElementById("homepageGen").style.display = "block";
                     }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching role:', error);
                }
            });
        },
        error: function (xhr, status, error) {
            document.getElementById("enterOTPSection").style.display = "none";
            document.getElementById("successResult").style.display = "none";
            document.getElementById("failResult").style.display = "block";
        }
    });
}

function logout() {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('idToken');
    document.getElementById("homepageGen").style.display = "none";
    document.getElementById("homepageManager").style.display = "none";
    document.getElementById("sendOTPSection").style.display = "block";
}

function getUserProfile(){

    console.log("get userprofile clicked  clicked");
    document.getElementById('userProfileView').style.display = 'block';

    document.getElementById('homepageGen').style.display = 'none';
    document.getElementById('homepageManager').style.display = 'none';
    document.getElementById('sendOTPSection').style.display = 'none';
    document.getElementById('enterOTPSection').style.display = 'none';
    document.getElementById('successResult').style.display = 'none';
    document.getElementById('failResult').style.display = 'none';

}

function spaceBookingCenterAccess() {
    document.getElementById('spaceBookingView').style.display = 'block';

    document.getElementById('homepageGen').style.display = 'none';
    document.getElementById('homepageManager').style.display = 'none';
    document.getElementById('sendOTPSection').style.display = 'none';
    document.getElementById('enterOTPSection').style.display = 'none';
    document.getElementById('successResult').style.display = 'none';
    document.getElementById('failResult').style.display = 'none';
}

function taskHubAccess() {
    document.getElementById('taskManagerView').style.display = 'block';
    document.getElementById('waitlistView').style.display = 'none';
// do an if user role is whatever then display the manager page 
    document.getElementById('homepageGen').style.display = 'block';
    document.getElementById('homepageManager').style.display = 'none';
    document.getElementById('sendOTPSection').style.display = 'none';
    document.getElementById('enterOTPSection').style.display = 'none';
    document.getElementById('successResult').style.display = 'none';
    document.getElementById('failResult').style.display = 'none';
    document.getElementById('personalOverviewCenter').style.display = 'none';
}

function personalOverviewAccess() {
    // Show the personalOverviewCenter section
    document.getElementById('personalOverviewCenter').style.display = 'block';

    document.getElementById('homepageGen').style.display = 'none';
    document.getElementById('homepageManager').style.display = 'none';
    document.getElementById('sendOTPSection').style.display = 'none';
    document.getElementById('enterOTPSection').style.display = 'none';
    document.getElementById('successResult').style.display = 'none';
    document.getElementById('failResult').style.display = 'none';
    document.getElementById('waitlistView').style.display = 'none';
}
    // Hide other sections if needed
>>>>>>> parent of 48dc8bd (Merge branch 'main' into Sarah-S)
function waitlistAccess() {
    var accessToken = sessionStorage.getItem('accessToken');
    if (!accessToken) {
        console.error('Access token is not available.');
        return;
    }

    $.ajax({
        url: 'http://localhost:5099/api/waitlist/test',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + accessToken
        },
        contentType: 'application/json',
        success: function (response) {
            console.log(response);
            window.location.href = '../Waitlist/index.html';
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        }
    });
    var accessToken = sessionStorage.getItem('accessToken');
    if (!accessToken) {
        console.error('Access token is not available.');
        return;
    }

    $.ajax({
        url: 'http://localhost:5099/api/waitlist/test',
        type: 'GET',
        headers: {
            'Authorization': 'Bearer ' + accessToken
        },
        contentType: 'application/json',
        success: function (response) {
            console.log(response);
            window.location.href = '../Waitlist/index.html';
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        }
    });
}
