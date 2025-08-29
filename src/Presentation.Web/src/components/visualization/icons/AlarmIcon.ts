import { IconDrawFunction } from './types';

export const drawAlarmIcon: IconDrawFunction = ({ ctx, centerX, centerY, size, nodeColor }) => {
    // Set up drawing context
    ctx.fillStyle = '#ffffff';
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;

    // Draw a bigger warning triangle
    const alarmRadius = size * 0.75;
    ctx.beginPath();
    ctx.moveTo(centerX, centerY - alarmRadius);
    ctx.lineTo(centerX - alarmRadius * 0.866, centerY + alarmRadius * 0.5);
    ctx.lineTo(centerX + alarmRadius * 0.866, centerY + alarmRadius * 0.5);
    ctx.closePath();
    ctx.fill();
    
    // Add bigger exclamation mark
    ctx.fillStyle = nodeColor;
    ctx.fillRect(centerX - size * 0.07, centerY - size * 0.25, size * 0.14, size * 0.35);
    ctx.beginPath();
    ctx.arc(centerX, centerY + size * 0.2, size * 0.07, 0, Math.PI * 2);
    ctx.fill();
};