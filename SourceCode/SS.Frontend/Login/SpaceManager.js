document.getElementById('replaceImage').addEventListener('change', function(event) {
    var fileModify = event.target.files[0];
    if (!fileModify) {
        showModal('No file selected.');
        return;
    }
    if (fileModify.size > 5242880) {
        showModal('The file is too large. Please select a file smaller than 5MB.');
        return;
    }
    var reader = new FileReader();
    reader.onload = function(e) {
        var imagePreviewContainer = document.getElementById('modifyImagePreviewContainer');
        imagePreviewContainer.innerHTML = '';
        var img = new Image();
        img.src = e.target.result;
        img.style.maxWidth = '200px';
        img.style.maxHeight = '200px';
        imagePreviewContainer.appendChild(img);
        document.getElementById('modifyImageBase64').value = e.target.result.split(',')[1];
    };
    reader.readAsDataURL(fileModify);
});

document.getElementById('imageUpload').addEventListener('change', handleImageUpload);

function handleImageUpload(event) {
    var file = event.target.files[0];
    if (!file) {
        showModal('No file selected.');
        return;
    }
    if (file.size > 5242880) {
        showModal('The file is too large. Please select a file smaller than 5MB.');
        return;
    }
    var reader = new FileReader();
    reader.onload = function(e) {
        var imagePreviewContainer = document.getElementById('imagePreviewContainer');
        imagePreviewContainer.innerHTML = '';
        var img = new Image();
        img.src = e.target.result;
        img.style.width = '200px'; 
        img.style.height = '200px'; 
        imagePreviewContainer.appendChild(img);
        document.getElementById('imageBase64').value = e.target.result.split(',')[1];
    };
    reader.readAsDataURL(file);
}

function addSpaceRow(containerId, rowClass) {
    const container = document.getElementById(containerId);
    const currentSpaces = container.getElementsByClassName(rowClass).length;
    if (currentSpaces >= 20) {
        showModal('You cannot add more than 20 spaces.');
        return;
    }
    const newRow = document.createElement('div');
    newRow.className = rowClass;
    newRow.innerHTML = `
        <input type="text" class="spaceID" name="spaceID[]" placeholder="Space ID">
        <input type="text" class="timeLimit" name="timeLimit[]" placeholder="Time Limit">
    `;
    if (rowClass === 'spaceRowModify') {
        const deleteButton = document.createElement('button');
        deleteButton.type = 'button';
        deleteButton.className = 'btn btn-danger deleteSpace';
        deleteButton.textContent = 'Delete Space';
        deleteButton.onclick = function() { this.closest('.'+rowClass).remove(); };
        newRow.appendChild(deleteButton);
    }
    container.appendChild(newRow);
}

document.getElementById('addSpace').addEventListener('click', function() {
    const container = document.getElementById('spacesContainer');
    const currentSpaces = container.getElementsByClassName('spaceRow').length;

    if (currentSpaces >= 20) {
        showModal('You cannot add more than 20 spaces.');
        return;
    }

    const newRow = document.createElement('div');
    newRow.classList.add('spaceRow');
    newRow.innerHTML = `
        <input type="text" class="spaceID" name="spaceID[]" placeholder="Space ID">
        <input type="text" class="timeLimit" name="timeLimit[]" placeholder="Time Limit">
    `;
    container.appendChild(newRow);
});

document.getElementById('addSpaceModify').addEventListener('click', function() {
    const container = document.getElementById('spacesModificationContainer');
    const currentModifiedSpaces = container.getElementsByClassName('spaceRowModify').length;

    if (currentModifiedSpaces >= 20) {
        showModal('You cannot add more than 20 modified spaces.');
        return;
    }

    const newRow = document.createElement('div');
    newRow.classList.add('spaceRowModify');
    newRow.innerHTML = `
        <input type="text" class="modifySpaceID" name="spaceID[]" placeholder="Space ID">
        <input type="text" class="modifyTimeLimit" name="modifytimeLimit[]" placeholder="Time Limit">
        <button type="button" class="btn btn-danger deleteSpace">Delete Space</button>
    `;
    container.appendChild(newRow);

    newRow.querySelector('.deleteSpace').addEventListener('click', function() {
        this.closest('.spaceRowModify').remove();
    });
});

function collectSpacesAndTimeLimits() {
    let spacesDict = {};
    // Assuming each spaceID input is directly followed by its corresponding timeLimit input
    const spaceIDs = document.querySelectorAll('.spaceID');
    const timeLimits = document.querySelectorAll('.timeLimit');
    const uniqueSpaceIDs = new Set(); 
    spaceIDs.forEach((spaceInput, index) => {
        const spaceID = spaceInput.value;
        const timeLimit = timeLimits[index].value; // Assuming the same index
        if (isValidSpaceID(spaceID) && isValidTimeLimit(timeLimit)) {
            if (uniqueSpaceIDs.has(spaceID)) {
                showModal('Space ID must be unique.');
                return; // Stop processing and return an empty object.
            }
            uniqueSpaceIDs.add(spaceID);
            spacesDict[spaceID] = parseInt(timeLimit);
        } else {
            showModal('Invalid Space ID or Time Limit.');
            return; // Stop processing and return an empty object.
        }
    });

    return spacesDict;
}

function isValidSpaceID(spaceID) {
    const regex = /^[a-zA-Z0-9]{2,10}$/; // Only alphanumeric characters, 2-10 in length
    return regex.test(spaceID);
}

function isValidTimeLimit(timeLimit) {
    const num = parseInt(timeLimit);
    return !isNaN(num) && num >= 1 && num <= 12; // Must be an integer between 1 and 12
}

function collectSpacesAndTimeLimitsToModify() {
    let modifications = [];
    const spaceIDs = document.querySelectorAll('.modifySpaceID');
    const timeLimits = document.querySelectorAll('.modifyTimeLimit');

    spaceIDs.forEach((spaceInput, index) => {
        const spaceID = spaceInput.value;
        const timeLimit = timeLimits[index].value;
        if (spaceID && timeLimit) {
            modifications.push({
                spaceID: spaceID,
                newTimeLimit: parseInt(timeLimit)
            });
        }
    });

    return modifications;
}

document.querySelectorAll('.accordion').forEach(function(button) {
    button.addEventListener('click', function() {
        this.classList.toggle('active');
        var formContainer = this.nextElementSibling;
        formContainer.style.display = formContainer.style.display === 'flex' ? 'none' : 'flex';
    });
});


document.getElementById('spaceCreationForm').addEventListener('submit', function(e) {
    fetchInsertUsedFeature('Space Manager');
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const SpaceManagerUrl = appConfig.api.SpaceManager; 
    e.preventDefault();
    const floorPlanName = document.getElementById('floorPlanName').value;

    if (!isValidInput(floorPlanName)) {
        showModal('Invalid input: Floor plan name must be 2-50 characters long and contain only letters and numbers.');
        return;
    }
    const companyFloor = {
        hashedUsername: JSON.parse(sessionStorage.getItem('idToken')).Username,
        FloorPlanName: document.getElementById('floorPlanName').value,
        FloorPlanImage: document.getElementById('imageBase64').value,
        FloorSpaces: collectSpacesAndTimeLimits()
    };
    sendData(`${SpaceManagerUrl}/api/SpaceManager/postSpace`, companyFloor);
    document.getElementById('spaceCreationForm').reset();
    fetchFloorPlan();
});

function isValidInput(input) {
    const regex = /^[a-zA-Z0-9 ]{2,50}$/; 
    return regex.test(input);
}

document.getElementById('modifySpaceForm').addEventListener('submit', function(e) {
    fetchInsertUsedFeature('Space Manager');
    e.preventDefault();
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const SpaceManagerUrl = appConfig.api.SpaceManager; 
    const modifications = collectSpacesAndTimeLimitsToModify();
    modifications.forEach(modification => {
        const request = {
            hashedUsername: JSON.parse(sessionStorage.getItem('idToken')).Username,
            spaceID: modification.spaceID,
            newTimeLimit: modification.newTimeLimit
        };
        sendData(`${SpaceManagerUrl}/api/SpaceManager/modifyTimeLimits`, request);
        document.getElementById('modifySpaceForm').reset();
        fetchFloorPlan();

    });
});

document.getElementById('modifyImage').addEventListener('click', function() {
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const SpaceManagerUrl = appConfig.api.SpaceManager; 
    const newFloor = {
        hashedUsername: JSON.parse(sessionStorage.getItem('idToken')).Username,
        FloorPlanName: document.getElementById('modifyFloorPlanName').value,
        FloorPlanImage: document.getElementById('modifyImageBase64').value
    };
    sendData(`${SpaceManagerUrl}/api/SpaceManager/modifyFloorPlan`, newFloor);
});

document.addEventListener('click', function(e) {
    if (e.target.classList.contains('deleteSpace')) {
        if (!appConfig) {
            console.error('Configuration is not loaded!');
            return;
        }
        const SpaceManagerUrl = appConfig.api.SpaceManager; 
        const spaceID = e.target.closest('.spaceRowModify').querySelector('.modifySpaceID').value;
        const request = {
            hashedUsername: JSON.parse(sessionStorage.getItem('idToken')).Username,
            spaceID: spaceID
        }
        sendData(`${SpaceManagerUrl}/api/SpaceManager/deleteSpace`, request);
    }
});

document.getElementById('deleteFloor').addEventListener('click', function() {
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const SpaceManagerUrl = appConfig.api.SpaceManager; 
    const FloorPlanName = document.getElementById('modifyFloorPlanName').value;
    const request = {
        hashedUsername: JSON.parse(sessionStorage.getItem('idToken')).Username,
        FloorPlanName: FloorPlanName
    }
    sendData(`${SpaceManagerUrl}/api/SpaceManager/deleteFloor`, request);
});


function sendData(url, data) {
    const accessToken = sessionStorage.getItem('accessToken'); 
    if (!accessToken) {
        console.error('Token expired or invalid');
        logout();
        return;
    }
    fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${accessToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => {
        console.log('Success:', data);
        showModal('Operation successful!');
    })
    .catch((error) => {
        console.error('Error:', error);
        showModal('An error occurred. Please try again.');
    });
}

document.getElementById('companyList').addEventListener('click', function () {
    console.log("Company List button was clicked")
    // const userName = JSON.parse(sessionStorage.getItem('idToken')).Username;
    fetchFloorPlan();
});

function fetchFloorPlan() {
    if (!appConfig) {
        console.error('Configuration is not loaded!');
        return;
    }
    const CompanyAPIUrl = appConfig.api.CompanyAPI; 
    console.log("sending request");
    const userName = JSON.parse(sessionStorage.getItem('idToken')).Username;
    let accessToken = sessionStorage.getItem('accessToken');
    if (!accessToken) {
        console.error('Token expired or invalid');
        logout();
        return;
    }

    const url = `${CompanyAPIUrl}/api/v1/spaceBookingCenter/companies/FloorPlanManager?hashedUsername=${encodeURIComponent(userName)}`;
    fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${accessToken}`,
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        let floorPlanContent = document.getElementById('floorPlanContent');
        if (!floorPlanContent) {
            console.error('floorPlanContent element is missing');
            return;
        }
        floorPlanContent.innerHTML = '';

        // Access the floorPlans array within the data object
        if (data.floorPlans && Array.isArray(data.floorPlans)) {
            data.floorPlans.forEach(floorPlan => {
                const floorDiv = document.createElement('div');
                floorDiv.classList.add('floor-plan');
                const imageStyles = 'max-width: 400px; max-height: 400px; width: auto; height: auto;';
                floorDiv.innerHTML = `<h3>${floorPlan.floorPlanName}</h3>
                                      <img src="data:image/png;base64,${floorPlan.floorPlanImageBase64}" alt="${floorPlan.floorPlanName}" style="${imageStyles}"/>
                                      <div>FloorSpaces:</div>`;

                const spacesList = document.createElement('ul');
                // Ensure that floorSpaces is an object before trying to access its keys
                if (typeof floorPlan.floorSpaces === 'object' && floorPlan.floorSpaces !== null) {
                    Object.entries(floorPlan.floorSpaces).forEach(([spaceID, timeLimit]) => {
                        const spaceItem = document.createElement('li');
                        spaceItem.textContent = `SpaceID: ${spaceID}, Time Limit: ${timeLimit}`;
                        spacesList.appendChild(spaceItem);
                    });
                }

                floorDiv.appendChild(spacesList);
                floorPlanContent.appendChild(floorDiv);
            });
        } else {
            console.error('No floor plans data found or floorPlans is not an array:', data);
        }
    })
    .catch(error => {
        console.error('Error fetching floor plans:', error);
    });
}
