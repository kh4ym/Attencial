window.cameraInterop = {
    stream: null,

    startCamera: async function (videoElementId) {
        const video = document.getElementById(videoElementId);
        if (!video) {
            throw new Error(`Camera element "${videoElementId}" was not found.`);
        }
        if (this.stream) {
            this.stopCamera();
        }
        this.stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: "user" }  // Front camera
        });
        video.srcObject = this.stream;
        await video.play();
    },

    captureFrame: function (videoElementId) {
        const video = document.getElementById(videoElementId);
        const canvas = document.createElement("canvas");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        canvas.getContext("2d").drawImage(video, 0, 0);

        // Return Base64 WITHOUT the "data:image/jpeg;base64," prefix
        return canvas.toDataURL("image/jpeg").split(",")[1];
    },

    stopCamera: function () {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }
    }
};
