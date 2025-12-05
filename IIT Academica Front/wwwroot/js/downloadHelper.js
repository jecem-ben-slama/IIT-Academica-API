window.BlazorDownloadFile = (fileName, contentStreamReference, contentType) => {
    contentStreamReference.arrayBuffer().then(buffer => {
        const blob = new Blob([buffer], { type: contentType });
        const url = URL.createObjectURL(blob);

        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;

        document.body.appendChild(anchor);
        anchor.click();
        anchor.remove();
        URL.revokeObjectURL(url);
    });
};
