window.showCreationSectionClientSide = function () {
    var search = document.getElementById('searchAndListSection');
    var clientForm = document.getElementById('clientSideFormSection');

    if (search) search.style.display = 'none';
    if (clientForm) clientForm.style.display = 'block';
}

window.hideCreationSectionClientSide = function () {
    var search = document.getElementById('searchAndListSection');
    var clientForm = document.getElementById('clientSideFormSection');

    if (clientForm) clientForm.style.display = 'none';
    if (search) search.style.display = 'block';
}