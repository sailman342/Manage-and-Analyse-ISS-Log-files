function SaveToFile_SaveFileToDownloadFolder(fileNameArg, dataArg) {
    // usual save function, will save with filename in download folder
    var link = document.createElement('a');
    link.download = fileNameArg;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(dataArg)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}


async function SaveToFile_PerformSaveDataToFile() {

    const options = {
        suggestedName: window.SaveDataToFile_FileName
    };

    try {
        const handle = await window.showSaveFilePicker(options);

        await SaveToFile_WriteFile(handle, window.SaveDataToFile_DataToSave);
        alert("Données sauvées dans : " + handle.name);
    }
    catch (err) {
        alert("Erreur ou Annulation par l'utilisateur : " + err);
    }

    if (SaveDataToFile_Instance != undefined) {
        SaveDataToFile_Instance.invokeMethodAsync(SaveDataToFile_CallBack);
    }


    delete window.SaveDataToFile_DataToSave;
    delete window.SaveDataToFile_FileName;
    delete window.SaveDataToFile_CallBack ;
    delete window.SaveDataToFile_Instance ;

}

// fileHandle is an instance of FileSystemFileHandle..
async function SaveToFile_WriteFile(fileHandle, contents) {

    const writable = await fileHandle.createWritable();
    await writable.write(contents);
    await writable.close();
}