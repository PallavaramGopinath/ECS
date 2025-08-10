window.initializeCamera_old = async function () {
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
window.captureImage_old = function () {
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

window.initializeCamera = async () => {
    const video = document.getElementById('cameraFeed');
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        video.style.display = 'block';
        return true;
    } catch (err) {
        console.error("Camera error: ", err);
        throw err;
    }
};
window.captureImage = () => {
    try {
        const video = document.getElementById('cameraFeed');
        if (!video || !video.srcObject) {
            throw new Error("Camera not initialized");
        }
        const canvas = document.createElement('canvas');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);

        // Convert to JPEG with 0.7 quality (adjust as needed)
        return canvas.toDataURL('image/jpeg', 0.7);
    }
    catch (err) {
        console.error("Error capturing image: ", err);
        throw err; /// Let Blazor handle the exception
    }
};