#!/usr/bin/env python3
"""
The code is edited from docs (https://docs.luxonis.com/projects/api/en/latest/samples/Yolo/tiny_yolo/)
We add parsing from JSON files that contain configuration
"""

from pathlib import Path
import sys
import cv2
import depthai as dai
import numpy as np
import time
import argparse
import json
import blobconverter
import zmq
import json

context = zmq.Context()
socket = context.socket(zmq.PUB)
socket.bind("tcp://*:5555")

# parse arguments
parser = argparse.ArgumentParser()
parser.add_argument("-m", "--model", help="Provide model name or model path for inference",
                    default='yolov5n_new.blob', type=str)
parser.add_argument("-c", "--config", help="Provide config path for inference",
                    default='yolov5n_new.json', type=str)
args = parser.parse_args()

# parse config
configPath = Path(args.config)
if not configPath.exists():
    raise ValueError("Path {} does not exist!".format(configPath))

with configPath.open() as f:
    config = json.load(f)
nnConfig = config.get("nn_config", {})

# parse input shape
if "input_size" in nnConfig:
    W, H = tuple(map(int, nnConfig.get("input_size").split('x')))

# extract metadata
metadata = nnConfig.get("NN_specific_metadata", {})
classes = metadata.get("classes", {})
coordinates = metadata.get("coordinates", {})
anchors = metadata.get("anchors", {})
anchorMasks = metadata.get("anchor_masks", {})
iouThreshold = metadata.get("iou_threshold", {})
confidenceThreshold = metadata.get("confidence_threshold", {})

print(metadata)

# parse labels
nnMappings = config.get("mappings", {})
labels = nnMappings.get("labels", {})

# get model path
nnPath = args.model
if not Path(nnPath).exists():
    print("No blob found at {}. Looking into DepthAI model zoo.".format(nnPath))
    nnPath = str(blobconverter.from_zoo(args.model, shaves = 6, zoo_type = "depthai", use_cache=True))
# sync outputs
syncNN = True

# Create pipeline
pipeline = dai.Pipeline()

# Define sources and outputs
camRgb = pipeline.create(dai.node.ColorCamera)
manip = pipeline.create(dai.node.ImageManip)
detectionNetwork = pipeline.create(dai.node.YoloDetectionNetwork)
nnOut = pipeline.create(dai.node.XLinkOut)

nnOut.setStreamName("nn")

# Properties
camRgb.setPreviewSize(576, 324)
# 854 × 480

camRgb.setResolution(dai.ColorCameraProperties.SensorResolution.THE_1080_P)
camRgb.setInterleaved(False)
camRgb.setColorOrder(dai.ColorCameraProperties.ColorOrder.BGR)
camRgb.setFps(20)

manip.setMaxOutputFrameSize(608256)
manip.initialConfig.setResizeThumbnail(576, 352)
manip.inputImage.setQueueSize(1)
manip.inputImage.setBlocking(False)

# Network specific settings
detectionNetwork.setConfidenceThreshold(confidenceThreshold)
detectionNetwork.setNumClasses(classes)
detectionNetwork.setCoordinateSize(coordinates)
detectionNetwork.setAnchors(anchors)
detectionNetwork.setAnchorMasks(anchorMasks)
detectionNetwork.setIouThreshold(iouThreshold)
detectionNetwork.setBlobPath(nnPath)
detectionNetwork.setNumInferenceThreads(2)
detectionNetwork.input.setBlocking(False)

# Linking
camRgb.preview.link(manip.inputImage)
manip.out.link(detectionNetwork.input)
detectionNetwork.out.link(nnOut.input)

# Connect to device and start pipeline
with dai.Device(pipeline) as device:
    qDet = device.getOutputQueue(name="nn", maxSize=4, blocking=False)

    detections = []
    message = np.array([0.0 for i in range(6 * classes)], dtype='f')
    pos=0

    while True:
        inDet = qDet.get()
        detections = inDet.detections
        message[0::6]= np.array([0.0], dtype='f')
        if detections is not None:
            for detection in detections:
                pos = detection.label * 6
                message[pos:pos+6] = np.array([1.0, detection.xmin, detection.xmax, detection.ymin, detection.ymax, detection.confidence], dtype='f')
                message.clip(0, 1, out=message)
        socket.send(message.tobytes('A'))
