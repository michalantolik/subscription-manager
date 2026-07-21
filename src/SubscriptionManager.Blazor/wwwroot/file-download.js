window.flow55 = window.flow55 || {};
window.flow55.downloadFile = (fileName, contentType, base64Content) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64Content}`;
    document.body.appendChild(link);
    link.click();
    link.remove();
};
