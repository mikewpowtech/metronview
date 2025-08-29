import { IconDrawFunction } from './types';

export const drawTriggerIcon: IconDrawFunction = ({ ctx, centerX, centerY, size }) => {
    // Set up drawing context
    ctx.fillStyle = '#ffffff';
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;

    // Draw a much better trigger symbol - a clean lightning bolt
    const triggerRadius = size * 0.4;

    // Create a more defined lightning bolt shape
    ctx.beginPath();
    // Start at top
    ctx.moveTo(centerX - triggerRadius * 0.2, centerY - triggerRadius);
    // Top right edge
    ctx.lineTo(centerX + triggerRadius * 0.4, centerY - triggerRadius);
    // Inner notch (top)
    ctx.lineTo(centerX + triggerRadius * 0.1, centerY - triggerRadius * 0.1);
    // Right side to middle
    ctx.lineTo(centerX + triggerRadius * 0.6, centerY - triggerRadius * 0.1);
    // Bottom point
    ctx.lineTo(centerX + triggerRadius * 0.2, centerY + triggerRadius);
    // Bottom left edge
    ctx.lineTo(centerX - triggerRadius * 0.4, centerY + triggerRadius);
    // Inner notch (bottom)
    ctx.lineTo(centerX - triggerRadius * 0.1, centerY + triggerRadius * 0.1);
    // Left side to middle
    ctx.lineTo(centerX - triggerRadius * 0.6, centerY + triggerRadius * 0.1);
    // Close the path back to start
    ctx.closePath();
    ctx.fill();
};