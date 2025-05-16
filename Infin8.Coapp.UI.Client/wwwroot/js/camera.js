window.initializeCamera = async function () {
    try {
        const video = document.getElementById('cameraFeed');
        if (video) {
            video.style.display = 'block';
            const stream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                    facingMode: 'environment' // Use back camera on mobile
                }
            });
            video.srcObject = stream;
        }
    } catch (err) {
        console.error("Error initializing camera:", err);
        alert("Could not access camera: " + err.message);
    }
};
window.captureImage = function () {
    return new Promise((resolve, reject) => {
        try {
            const video = document.getElementById('cameraFeed');
            if (!video || !video.srcObject) {
                reject("Camera is not initialized");
                return;
            }

            // Create a canvas element
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;

            // Draw the current video frame to canvas
            const context = canvas.getContext('2d');
            context.drawImage(video, 0, 0, canvas.width, canvas.height);

            // Convert to base64 image
            const imageData = canvas.toDataURL('image/jpeg');

            // Return the image data
            resolve(imageData);
        } catch (err) {
            console.error("Error capturing image:", err);
            reject(err);
        }
    });
};