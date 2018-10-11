#import <AVFoundation/AVFoundation.h>

extern "C" {
void _goodiesEnableFlashlight(bool enable) {
    AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if ([device hasTorch]) {
        [device lockForConfiguration:nil];
        [device setTorchMode:enable ? AVCaptureTorchModeOn : AVCaptureTorchModeOff];
        [device unlockForConfiguration];
    }
}

bool _goodiesDeviceHasFlashlight() {
    return [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo].hasTorch;
}

void _goodiesSetFlashlightLevel(float level) {
    AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    if ([device hasTorch]) {
        [device lockForConfiguration:nil];
        if (level <= 0.0) {
            [device setTorchMode:AVCaptureTorchModeOff];
        } else {
            if (level >= 1.0) {
                level = AVCaptureMaxAvailableTorchLevel;
            }
            BOOL success = [device setTorchModeOnWithLevel:level error:nil];
        }
        [device unlockForConfiguration];
    }
}
}
