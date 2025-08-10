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

