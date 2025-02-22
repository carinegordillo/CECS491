async function deleteData_Deletion() {
    var accessToken = sessionStorage.getItem('accessToken');
    var idToken = sessionStorage.getItem('idToken');
    var parsedIdToken = JSON.parse(idToken);
    var username = parsedIdToken.Username;
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const CPRAUrl = appConfig.api.CPRA; 

    try {
        console.log("running user data protection endpoint");
        const deleteResponse = await fetch(`${CPRAUrl}/api/userDataProtection/deleteData`, {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + accessToken,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(username)
        });

        if (!deleteResponse.ok) {
            throw new Error(`HTTP error! status: ${deleteResponse.status}`);
        }

        const contentType = deleteResponse.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            const deleteData = await deleteResponse.json();
            console.log('Delete response:', deleteData);
            if (deleteData.newToken) {
                accessToken = deleteData.newToken;
                sessionStorage.setItem('accessToken', accessToken);
                console.log('New access token stored:', accessToken);
            }
        } else {
            console.log('Delete operation successful.');
        }

        console.log('Successfully deleted data.');

        console.log("running account deletion endpoint");
        const DeletionUrl = appConfig.api.AccountDeletion; 
        const secondResponse = await fetch(`${DeletionUrl}/api/AccountDeletion/Delete`, {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + accessToken,
                'Content-Type': 'application/json'
            }
        });

        if (!secondResponse.ok) {
            throw new Error(`HTTP error! status: ${secondResponse.status}`);
        }

        const secondAccountDeletionData = await secondResponse.json();
        if (secondAccountDeletionData.newToken) {
            accessToken = secondAccountDeletionData.newToken;
            sessionStorage.setItem('accessToken', accessToken);
            console.log('New access token stored:', accessToken);
        }

        console.log('Successfully completed account deletion.');

        showDeleteSuccessMessage();
    } catch (error) {
        console.error('Error deleting data:', error);
    }
}

function showDeleteSuccessMessage_Deletion() {
    const modal = document.createElement('div');
    modal.classList.add('modal');

    const modalContent = document.createElement('div');
    modalContent.classList.add('modal-content');
    modalContent.innerHTML = `
        <span class="close-button">&times;</span>
        <h2>Success!</h2>
        <p>Your data has been successfully deleted. Please refer to the email sent to view the requested data.
           You will be redirected to the home screen soon. Thank you for using Space Surfer. We hope to see you again soon!
        </p>
    `;

    const closeButton = modalContent.querySelector('.close-button');
    closeButton.addEventListener('click', function () {
        modal.style.display = 'none';
        logout();
        document.getElementById('cpraView').style.display = 'none';
    });

    modal.appendChild(modalContent);
    document.body.appendChild(modal);

    modal.style.display = 'block';
}
function AccountDeletion_sendCode_Deletion() {
    var userIdentity = document.getElementById("userIdentity").value;
    console.log(userIdentity);
    const loginUrl = appConfig.api.Login; 
    fetch(`${loginUrl}/api/auth/sendOTP`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ userIdentity: userIdentity })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error sending verification code.');
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("deleteDataSection").style.display = "none";
            document.getElementById("verifyOTPSection_Deletion").style.display = "block";
        })
        .catch(error => {
            showModal(error.message);
        });
}
function AccountDeletion_verifyUser_Deletion() {
    var accessToken = sessionStorage.getItem('accessToken');
    var idToken = sessionStorage.getItem('idToken');
    var parsedIdToken = JSON.parse(idToken);
    var username = parsedIdToken.Username;
    var otp = document.getElementById('deletion_otp').value;
    const loginUrl = appConfig.api.Login; 
    fetch(`${loginUrl}/api/auth/verifyCode`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ userIdentity: username, proof: otp })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error verifying OTP.');
            }
            return response;
        })
        .then(data => {
            document.getElementById("verifyOTPSection_Deletion").style.display = "none";
            deleteData_Deletion();
            console.log("otp matches");
        })
        .catch(error => {
            document.getElementById("verifyOTPSection_Deletion").style.display = "none";
            console.log("Error:", error);
        });
}

function createDeletionUI() {
    const contentContainer = document.getElementById('contentContainer_Deletion');

    contentContainer.innerHTML = '';

    const deleteDataSection = document.createElement('section');
    deleteDataSection.id = 'deleteDataSection';
    deleteDataSection.classList.add('form-container');
    deleteDataSection.innerHTML = `
        <h2>Delete Your Data</h2>
        <p>By deleting your data, you acknowledge that your account will be permanently removed from our system. This action is irreversible.</p>
        <form id="deleteDataForm">
            <label for="deleteUsername">Enter Your Username:</label>
            <input type="text" id="deleteUsername" name="deleteUsername" required>
            <button type="submit">Delete Data</button>
        </form>
    `;
    contentContainer.appendChild(deleteDataSection);

    deleteDataSection.style.display = 'block';

    const verifyOTPSection_Deletion = document.createElement('section');
    verifyOTPSection_Deletion.id = 'verifyOTPSection_Deletion';
    verifyOTPSection_Deletion.classList.add('form-container');
    verifyOTPSection_Deletion.style.display = 'none';
    verifyOTPSection_Deletion.innerHTML = `
        <h2>Verify Your Identity</h2>
        <form id="verifyOTPForm_Deletion">
            <label for="deletion_otp">Enter Verification Code:</label>
            <input type="text" id="deletion_otp" name="deletion_otp" required>
            <button type="submit">Submit</button>
        </form>
    `;
    contentContainer.appendChild(verifyOTPSection_Deletion);

    document.getElementById('deleteDataForm').addEventListener('submit', function (event) {
        event.preventDefault();
        AccountDeletion_sendCode_Deletion();
    });

    const verifyOTPForm_Deletion = document.getElementById('verifyOTPForm_Deletion');
    if (verifyOTPForm_Deletion) {
        verifyOTPForm_Deletion.addEventListener('submit', function (event) {
            event.preventDefault();
            AccountDeletion_verifyUser_Deletion();
        });
    }
}

function showDeletionSection() {
    const deletionSection = document.getElementById('deletionView');
    deletionSection.style.display = 'block';
    createDeletionUI();
}

function hideAllSectionsDeletion() {
    const currentView = sessionStorage.getItem('currentView');

    if (currentView) {
        endTimerAndInsertDuration(currentView);
    }

    document.getElementById('employeeSetup').style.display = 'none';
    document.getElementById('spaceBookingView').style.display = 'none';
    document.getElementById('sendOTPSection').style.display = 'none';
    document.getElementById('enterOTPSection').style.display = 'none';
    document.getElementById('successResult').style.display = 'none';
    document.getElementById("taskManagerView").style.display = "none";
    document.getElementById('personalOverviewCenter').style.display = 'none';
    document.getElementById('waitlistView').style.display = 'none';
    document.getElementById("welcomeSection").style.display = "none";
    document.getElementById('UnAuthnAbout').style.display = 'none';
    document.getElementById('Registration').style.display = 'none';
    document.getElementById('spaceManagerView').style.display = 'none';
    document.getElementById('userProfileView').style.display = 'none';
    document.getElementById('noLogin').style.display = 'none';
    document.getElementById('confirmationView').style.display = 'none';
    document.getElementById('systemObservability').style.display = 'none';
    document.getElementById("accountRecoverySection").style.display = "none";
    document.getElementById("userRequestsView").style.display = "none";
    document.getElementById("enterRegistrationOTPSection").style.display = "none";
    document.getElementById('cpraView').style.display = 'none';
    document.getElementById('deletionView').style.display = 'none';
}

function deletionAccess() {
    hideAllSectionsDeletion();
    document.getElementById('deletionView').style.display = 'block';
    showDeletionSection();

}