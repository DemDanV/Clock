# Project Overview

This Unity project is an Android application that displays an analog clock, synchronizes time with internet time services, and includes a built-in alarm functionality. The app supports both portrait and landscape orientations and is designed to handle time updates from multiple sources.

## Key Features

### 1. Analog Clock Display
- **Clock Hands**: The app shows moving **hour**, **minute**, and **second** hands that accurately reflect the current time.
- **Digital Time**: In addition to the analog clock, the current time is also displayed in a digital format.

### 2. Time Synchronization
- **Multiple Time Services**: Upon startup, the app requests the current time from at least two different internet time services and adjusts the clock hands accordingly.
- **Hourly Updates**: The app automatically checks the time from these services every hour to ensure accuracy and updates the clock hands if needed.

### 3. Alarm Functionality
- **Single Alarm**: The app allows the user to set one alarm. The alarm can be set in two ways:
  - **Manual Input**: Users can input the time by entering numerical values.
  - **Drag-and-Drop**: Users can adjust the time by dragging the clock hands to the desired alarm time.
- **Alarm Management**: Only one alarm can be active at any given time.

### 4. Screen Orientation and Responsiveness
- **Portrait and Landscape Support**: The app is fully responsive and displays correctly in both **portrait** and **landscape** modes.
- **Screen Rotation**: The clock and all UI elements dynamically adjust to screen rotation, ensuring a consistent user experience across orientations.

## Additional Notes
- **Code Repository**: The source code for this project is hosted in a public Git repository, accessible via the provided link.
- **Platform**: The project is specifically designed for the **Android** platform using **Unity**.
