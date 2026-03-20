function setFocus(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}
function openInNewTab(url) {
    window.open(url, '_blank');
}