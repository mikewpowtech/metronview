import { VisualizationNode } from '../../../types/visualization';

export interface MousePosition {
    x: number;
    y: number;
}

export class MouseEventHandler {
    public static getMousePosition(
        e: React.MouseEvent<HTMLCanvasElement> | MouseEvent,
        canvas: HTMLCanvasElement,
        zoom: number,
        pan: { x: number; y: number }
    ): MousePosition {
        const rect = canvas.getBoundingClientRect();
        return {
            x: (e.clientX - rect.left) / zoom - pan.x,
            y: (e.clientY - rect.top) / zoom - pan.y
        };
    }

    public static getNodeAtPosition(
        x: number, 
        y: number, 
        nodes: VisualizationNode[]
    ): VisualizationNode | null {
        for (let i = nodes.length - 1; i >= 0; i--) {
            const node = nodes[i];
            if (x >= node.x && x <= node.x + node.width &&
                y >= node.y && y <= node.y + node.height) {
                return node;
            }
        }
        return null;
    }
}