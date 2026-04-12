window.printSection = function (screenContent, printContent) {
    sessionStorage.setItem('printContent', printContent);
    sessionStorage.setItem('screenContent', screenContent);

    var win = window.open('', '_blank');
    win.document.write('<html><head><title>Employee Profiles</title>');
    win.document.write('<style>');
    win.document.write('body { font-family: Arial, sans-serif; }');
    win.document.write('table { width: 100%; border-collapse: collapse; table-layout: fixed; }');
    win.document.write('th, td { border: 1px solid black; padding: 8px; word-wrap: break-word; }');
    win.document.write('h2 { text-align: center; }');
    win.document.write('#printBtn { display:block; margin:20px auto; padding:10px 30px; font-size:16px; cursor:pointer;  }');
    win.document.write('@media print { #printBtn { display: none; } }');
    win.document.write('</style></head><body></body></html>');
    win.document.close();

    setTimeout(function () {
        // inject screen content
        win.document.body.innerHTML = sessionStorage.getItem('screenContent');

        // buat button
        var btn = win.document.createElement('button');
        btn.id = 'printBtn';
        btn.innerText = 'Print / Save PDF';
        btn.setAttribute('style', 'display:block; margin:20px auto; padding:10px 30px; font-size:16px; cursor:pointer;  ');
        win.document.body.appendChild(btn);

        // event listener untuk print
        btn.addEventListener('click', function () {
            var style = win.document.createElement('style');
            style.innerHTML =
                'body { font-family: Arial, sans-serif; }' +
                'table { width: 100%; border-collapse: collapse; table-layout: fixed; }' +
                'th, td { border: 1px solid black; padding: 8px; word-wrap: break-word; }' +
                'h2 { text-align: center; }' +
                '@media screen { body { visibility: hidden; } }' + 
                '@media print { body { visibility: visible; } }';   

            win.document.body.innerHTML = sessionStorage.getItem('printContent');
            win.document.head.appendChild(style);

            setTimeout(function () {
                win.print();
                // lepas print, balik ke screen content
                win.document.body.style.visibility = 'visible';
            }, 100);
        });

        win.focus();
    }, 500);
}

