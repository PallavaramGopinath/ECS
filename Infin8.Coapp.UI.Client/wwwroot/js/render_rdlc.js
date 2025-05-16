function saveAsFile(filename, byteArray) {
    const blob = new Blob([byteArray], { type: "application/pdf" });

    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;

    // Append the link element to the document, trigger a click, and then clean up
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}