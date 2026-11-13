function openPdfInNewTab(base64String) {
    try {
        // Step 1: Decode the Base64 string into a binary string.
        const binaryString = window.atob(base64String);

        // Step 2: Create a byte array (Uint8Array) from the binary string.
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Step 3: Create a Blob from the byte array with the correct MIME type.
        const blob = new Blob([bytes], { type: "application/pdf" });

        // Step 4: Create a short, temporary URL for the Blob.
        const url = URL.createObjectURL(blob);

        // Step 5: Open this clean, short URL in a new tab.
        const newWindow = window.open(url, '_blank');
        if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
            alert('Please allow pop-ups for this website to view the PDF.');
        }

        // Optional: Revoke the object URL after a delay to free up memory.
        // The PDF viewer in the new tab needs time to load the data.
        setTimeout(() => URL.revokeObjectURL(url), 10000);

    } catch (e) {
        console.error("Failed to open PDF: ", e);
        alert("An error occurred while trying to display the PDF.");
    }

    

    //// Create the Data URL
    //const dataUrl = `data:application/pdf;base64,${base64String}`;

    //// Open the Data URL in a new tab
    //const newWindow = window.open(dataUrl, '_blank');

    //// A good practice is to check if the pop-up was blocked
    //if (!newWindow || newWindow.closed || typeof newWindow.closed === 'undefined') {
    //    alert('Please allow pop-ups for this website to view the PDF.');
    //}
}


// Office Note modal (ECSLoanSanction): a Blob URL shown in an on-page iframe, so no pop-up is needed.
function createPdfObjectUrl(base64String) {
    const binaryString = window.atob(base64String);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    return URL.createObjectURL(new Blob([bytes], { type: "application/pdf" }));
}

function revokePdfObjectUrl(url) {
    if (url) URL.revokeObjectURL(url);
}

function printPdfFrame(frameId) {
    const frame = document.getElementById(frameId);
    if (!frame || !frame.contentWindow) return;
    frame.contentWindow.focus();
    frame.contentWindow.print();
}

// Prints one element's HTML through a hidden iframe, so the page chrome is not printed and no pop-up opens.
function printElement(elementId, title) {
    const el = document.getElementById(elementId);
    if (!el) return;
    const frame = document.createElement("iframe");
    frame.style.cssText = "position:fixed;right:0;bottom:0;width:0;height:0;border:0";
    document.body.appendChild(frame);
    const doc = frame.contentDocument;
    doc.open();
    doc.write("<!doctype html><html><head><meta charset='utf-8'><title></title>" +
        "<style>@page{size:A4;margin:12mm}body{margin:0}</style></head><body></body></html>");
    doc.close();
    doc.title = title || "";
    doc.body.innerHTML = el.outerHTML;
    frame.contentWindow.focus();
    frame.contentWindow.print();
    setTimeout(() => frame.remove(), 1000);
}

