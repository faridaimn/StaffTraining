// Client-side hide/show untuk Vehicle page
function showVehicleFormClientSide() {
    var searchSection = document.getElementById('searchSection');
    var listingSection = document.getElementById('listingSection');
    var formSection = document.getElementById('formSection');
    var addNewBtnContainer = document.getElementById('addNewBtnContainer');

    if (searchSection) searchSection.style.display = 'none';
    if (listingSection) listingSection.style.display = 'none';
    if (addNewBtnContainer) addNewBtnContainer.style.setProperty('display', 'none', 'important');
    if (formSection) formSection.style.display = 'block';
}

function hideVehicleFormClientSide() {
    var searchSection = document.getElementById('searchSection');
    var listingSection = document.getElementById('listingSection');
    var formSection = document.getElementById('formSection');
    var addNewBtnContainer = document.getElementById('addNewBtnContainer');

    if (searchSection) searchSection.style.display = 'block';
    if (listingSection) listingSection.style.display = 'block';
    if (addNewBtnContainer) addNewBtnContainer.style.setProperty('display', 'flex', 'important');
    if (formSection) formSection.style.display = 'none';
}

// Register event handlers when DOM ready
document.addEventListener('DOMContentLoaded', function () {
    var showBtn = document.getElementById('showFormBtn');
    var hideBtn = document.getElementById('hideFormBtn');

    if (showBtn) {
        showBtn.onclick = function () { showVehicleFormClientSide(); return false; };
    }
    if (hideBtn) {
        hideBtn.onclick = function () { hideVehicleFormClientSide(); return false; };
    }
});