import { LayoutConfig } from './LayoutConfig';

export interface Position {
    x: number;
    y: number;
}

export class PositionCalculator {
    /**
     * Calculate grid position for a node based on its index and layout config
     */
    public static calculateGridPosition(index: number, config: LayoutConfig): Position {
        const column = index % config.columns;
        const row = Math.floor(index / config.columns);
        
        return {
            x: config.startX + column * config.horizontalSpacing,
            y: config.startY + row * config.verticalSpacing
        };
    }

    /**
     * Calculate position with offset from a reference node
     */
    public static calculateRelativePosition(
        referencePosition: Position,
        offsetX: number,
        offsetY: number
    ): Position {
        return {
            x: referencePosition.x + offsetX,
            y: referencePosition.y + offsetY
        };
    }

    /**
     * Calculate clustered position around a parent node
     */
    public static calculateClusteredPosition(
        parentPosition: Position,
        childIndex: number,
        radius: number = 100,
        childrenCount: number
    ): Position {
        if (childrenCount === 1) {
            return {
                x: parentPosition.x + radius,
                y: parentPosition.y
            };
        }

        const angleStep = (2 * Math.PI) / childrenCount;
        const angle = childIndex * angleStep;
        
        return {
            x: parentPosition.x + Math.cos(angle) * radius,
            y: parentPosition.y + Math.sin(angle) * radius
        };
    }

    /**
     * Calculate hierarchical position based on parent-child relationships
     */
    public static calculateHierarchicalPosition(
        level: number,
        indexInLevel: number,
        itemsInLevel: number,
        levelSpacing: number = 200,
        itemSpacing: number = 150
    ): Position {
        const totalWidth = (itemsInLevel - 1) * itemSpacing;
        const startX = -totalWidth / 2;
        
        return {
            x: startX + indexInLevel * itemSpacing,
            y: level * levelSpacing
        };
    }
}