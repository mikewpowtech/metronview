import { IconDrawFunction } from './types';

export const drawCompanyIcon: IconDrawFunction = ({ ctx, centerX, centerY, size, nodeColor }) => {
    // Set up drawing context
    ctx.fillStyle = '#ffffff';
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;

    // Draw a bigger building shape
    const buildingWidth = size * 0.8;
    const buildingHeight = size * 0.9;
    const buildingX = centerX - buildingWidth / 2;
    const buildingY = centerY - buildingHeight / 2;
    
    ctx.fillRect(buildingX, buildingY, buildingWidth, buildingHeight);
    
    // Add windows
    ctx.fillStyle = nodeColor;
    const windowSize = buildingWidth * 0.15;
    for (let i = 0; i < 2; i++) {
        for (let j = 0; j < 3; j++) {
            ctx.fillRect(
                buildingX + (i + 0.5) * (buildingWidth / 3),
                buildingY + (j + 0.5) * (buildingHeight / 4),
                windowSize,
                windowSize
            );
        }
    }
};