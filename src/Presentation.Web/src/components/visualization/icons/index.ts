import { NodeType } from '../../../types/visualization';
import { IconDrawFunction } from './types';
import { drawCompanyIcon } from './CompanyIcon';
import { drawUnitIcon } from './UnitIcon';
import { drawAlarmIcon } from './AlarmIcon';
import { drawTriggerIcon } from './TriggerIcon';

export type { IconDrawFunction, IconDrawParams } from './types';

// Map of node types to their corresponding icon drawing functions
export const iconDrawers: Record<NodeType, IconDrawFunction> = {
    [NodeType.COMPANY]: drawCompanyIcon,
    [NodeType.UNIT]: drawUnitIcon,
    [NodeType.ALARM]: drawAlarmIcon,
    [NodeType.TRIGGER]: drawTriggerIcon,
};

// Main function to draw any icon based on node type
export const drawGeometricIcon = (
    ctx: CanvasRenderingContext2D, 
    type: NodeType, 
    x: number, 
    y: number, 
    size: number,
    nodeColors: Record<NodeType, string>
) => {
    const centerX = x + size / 2;
    const centerY = y + size / 2;
    const nodeColor = nodeColors[type];

    const iconDrawer = iconDrawers[type];
    if (iconDrawer) {
        iconDrawer({
            ctx,
            centerX,
            centerY,
            size,
            nodeColor,
        });
    }
};