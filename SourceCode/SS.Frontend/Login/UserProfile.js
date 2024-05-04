

let isEditing = false;



function initProfile() {

    try {
        const idToken = sessionStorage.getItem('idToken');
    
        if (!idToken) {
            console.error('idToken not found in sessionStorage');
            return;
        }
        const parsedIdToken = JSON.parse(idToken);

        // Log parsed object for debugging
        console.log('Parsed idToken:', parsedIdToken);

        if (!parsedIdToken || !parsedIdToken.Username) {
            console.error('Parsed idToken does not have Username');
            return;
        }

        const username = parsedIdToken.Username;

        fetchUserProfile(username).then(profile => {
            if (profile) {
                displayUserProfile(profile);
            }
        }).catch(error => console.error("Failed to fetch or append profile:", error));
    } catch (error) {
        console.error('Error parsing idToken:', error);
    }
}




// Event listener for profile actions
document.addEventListener('click', function (event) {
    const target = event.target;
    initProfile();

    if (target.id === 'editProfile') {
        event.preventDefault();
        event.stopPropagation();
        toggleEditProfile();
    } else if (target.id === 'saveChangesButton') {
        event.preventDefault();
        event.stopPropagation();
        saveProfileChanges();
    } else if (target.id === 'cancelChangesButton') {
        event.preventDefault();
        event.stopPropagation();
        cancelEditProfile();
    }
});

async function fetchUserProfile(email) {
    const accessToken = sessionStorage.getItem('accessToken');
    const isTokenValid = await checkTokenExpiration(accessToken);
    if (!isTokenValid) {
        logout();
        return;
    }

    try {
        const response = await fetch(`http://localhost:5048/api/profile/getUserProfile?email=${encodeURIComponent(email)}`);
        const data = await response.json();
        return data.length > 0 ? data[0] : null;
    } catch (error) {
        console.error('Error fetching user profile:', error);
        return null;
    }
}

function displayUserProfile(userProfile) {
    isEditing = false; // Set edit mode to false when displaying profile

    // Get email directly from sessionStorage without parsing it as JSON
    const email = sessionStorage.getItem('userIdentity');
    console.log('Email:', email);

    // Check for missing properties in userProfile
    if (!userProfile || typeof userProfile.firstName === 'undefined' || typeof userProfile.lastName === 'undefined') {
        console.error('User profile has missing properties:', userProfile);
        return;
    }

    // Find the panel elements
    const leftPanel = document.querySelector('.left-panel');
    const rightPanel = document.querySelector('.right-panel');

    if (!leftPanel || !rightPanel) {
        console.error('Panel elements not found in the DOM');
        return;
    }

    // Display profile details
    leftPanel.innerHTML = `
        <h2>Profile</h2>
        <p>Name: <span id="displayFirstName">${userProfile.firstName}</span> <span id="displayLastName">${userProfile.lastName}</span></p>
        <p>Bio: <span id="displayAbout">${userProfile.about || "User biography not provided."}</span></p>
        <button id="editProfile">Edit Profile</button>
    `;

    // Display non-editable account details
    rightPanel.innerHTML = `
        <h2>Account Details</h2>
        <p>Email: ${email}</p>
        <p>Backup Email: ${userProfile.backupEmail || "Not provided"}</p>
        <p>Role: ${userProfile.appRole || "User role not specified"}</p>
    `;
}




// Toggle profile to editable mode
function toggleEditProfile() {
    isEditing = true; // Set edit mode to true

    const leftPanel = document.querySelector('.left-panel');

    // Change the displayed profile to an editable form
    leftPanel.innerHTML = `
    <form id="modifyUserProfileForm">
        <h2>Edit Profile</h2>
        <input id="firstName" value="${document.getElementById('displayFirstName').innerText}" />
        <input id="lastName" value="${document.getElementById('displayLastName').innerText}" />
        <p3 id="about">${document.getElementById('displayAbout').innerText}</p>
        <button type="submit" id="saveChangesButton">Save Changes</button>
        <button id="cancelChangesButton">Cancel</button>
        </form>
    `;

}

async function saveProfileChanges() {
    const updatedProfile = {
        username: JSON.parse(sessionStorage.getItem('idToken')).Username, // Parse idToken correctly
        firstname: document.getElementById('firstName').value,
        lastname: document.getElementById('lastName').value,
    };

    try {
        const accessToken = sessionStorage.getItem('accessToken');
        const response = await fetch('http://localhost:5048/api/profile/updateUserProfile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify(updatedProfile)
        });

        const result = await response.json(); // Correctly await the JSON parsing

        if (response.ok) {
            fetchUserProfile(updatedProfile.username).then(profile => {
                if (profile) {
                    displayUserProfile(profile);
                }
            }).catch(error => console.error("Failed to fetch or append profile:", error));
        } else {
            alert('Profile update failed: ' + result.message);
        }
    } catch (error) {
        console.error('Error updating profile:', error);
        alert('An error occurred while updating the profile');
    }
}





function cancelEditProfile() {
    isEditing = false; // Set edit mode to false
    initProfile(); // Re-fetch and display the profile to cancel edits
}

async function checkTokenExpiration(accessToken) {
    try {
        const response = await fetch('http://localhost:5005/api/v1/spaceBookingCenter/reservations/checkTokenExp', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + accessToken,
                'Content-Type': 'application/json'
            }
        });
        return response.ok;
    } catch (error) {
        console.error('Error:', error);
        return false;
    }
}

function logout() {
    sessionStorage.removeItem('accessToken');
    sessionStorage.removeItem('idToken');
    document.getElementById('userProfileView').style.display = 'none';
    // Redirect to login or handle logout UI changes
}
