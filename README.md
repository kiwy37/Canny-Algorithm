# Canny-Algorithm

This project aims to implement the Canny edge detection algorithm for color images using C#. The Canny algorithm is a popular method for detecting edges in images, known for its effectiveness in identifying significant gradients and suppressing noise.

## Key Features:

Menu Integration: Add a menu option under Filters - Canny RGB.

Threshold Input: Prompt users to input the thresholds T1 and T2 from a dialog box.

Gaussian Filtering: Apply Gaussian filtering with σ = 1 to each color channel of the image.

Gradient Calculation: Determine the maximum variation direction and corresponding variation for each pixel (x, y) as per the course material.

Non-maximum Suppression: Implement the non-maximum suppression step to refine the detected edges.

Hysteresis Thresholding: Apply hysteresis thresholding to finalize the edge detection process.

## Implementation Details:
The project involves integrating the Canny edge detection algorithm into a C# application. It utilizes image processing techniques such as Gaussian filtering, gradient calculation, non-maximum suppression, and hysteresis thresholding to accurately detect edges in color images.
