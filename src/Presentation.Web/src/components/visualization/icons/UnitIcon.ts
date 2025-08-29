import { IconDrawFunction } from './types';

export const drawUnitIcon: IconDrawFunction = ({ ctx, centerX, centerY, size, nodeColor }) => {
    // Set up drawing context
    ctx.fillStyle = '#ffffff';
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;

    // Draw a bigger device shape (rectangle with rounded corners)
    const deviceWidth = size * 0.9;
    const deviceHeight = size * 0.7;
    const deviceX = centerX - deviceWidth / 2;
    const deviceY = centerY - deviceHeight / 2;
    
    ctx.beginPath();
    ctx.roundRect(deviceX, deviceY, deviceWidth, deviceHeight, size * 0.1);
    ctx.fill();
    
    // Add a small screen indicator
    ctx.fillStyle = nodeColor;
    ctx.fillRect(
        deviceX + deviceWidth * 0.2, 
        deviceY + deviceHeight * 0.2, 
        deviceWidth * 0.6, 
        deviceHeight * 0.3
    );
};