import { VisualizationNode, NodeConnection, NodeType } from '../../../types/visualization';
import { drawGeometricIcon } from '../icons';

export interface CanvasDrawingParams {
    canvas: HTMLCanvasElement;
    nodes: VisualizationNode[];
    connections: NodeConnection[];
    zoom: number;
    pan: { x: number; y: number };
    nodeColors: Record<NodeType, string>;
}

export class CanvasDrawingEngine {
    private ctx: CanvasRenderingContext2D;

    constructor(canvas: HTMLCanvasElement) {
        const context = canvas.getContext('2d');
        if (!context) {
            throw new Error('Could not get 2D context from canvas');
        }
        this.ctx = context;
    }

    public draw({ canvas, nodes, connections, zoom, pan, nodeColors }: CanvasDrawingParams) {
        this.clearCanvas(canvas);
        this.drawGrid(canvas, zoom, pan);
        this.drawConnections(connections, nodes, zoom, pan);
        this.drawNodes(nodes, zoom, pan, nodeColors);
    }

    private clearCanvas(canvas: HTMLCanvasElement) {
        this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    }

    private drawGrid(canvas: HTMLCanvasElement, zoom: number, pan: { x: number; y: number }) {
        const gridSize = 50 * zoom;
        this.ctx.strokeStyle = '#f0f0f0';
        this.ctx.lineWidth = 1;

        for (let x = (pan.x * zoom) % gridSize; x < canvas.width; x += gridSize) {
            this.ctx.beginPath();
            this.ctx.moveTo(x, 0);
            this.ctx.lineTo(x, canvas.height);
            this.ctx.stroke();
        }

        for (let y = (pan.y * zoom) % gridSize; y < canvas.height; y += gridSize) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, y);
            this.ctx.lineTo(canvas.width, y);
            this.ctx.stroke();
        }
    }

    private drawConnections(
        connections: NodeConnection[], 
        nodes: VisualizationNode[], 
        zoom: number, 
        pan: { x: number; y: number }
    ) {
        connections.forEach(connection => {
            const fromNode = nodes.find(n => n.id === connection.fromId);
            const toNode = nodes.find(n => n.id === connection.toId);

            if (!fromNode || !toNode) return;

            const fromX = (fromNode.x + fromNode.width / 2 + pan.x) * zoom;
            const fromY = (fromNode.y + fromNode.height / 2 + pan.y) * zoom;
            const toX = (toNode.x + toNode.width / 2 + pan.x) * zoom;
            const toY = (toNode.y + toNode.height / 2 + pan.y) * zoom;

            this.ctx.strokeStyle = connection.color;
            this.ctx.lineWidth = connection.strokeWidth * zoom;
            this.ctx.setLineDash([5 * zoom, 5 * zoom]);

            this.ctx.beginPath();
            this.ctx.moveTo(fromX, fromY);
            this.ctx.lineTo(toX, toY);
            this.ctx.stroke();

            this.ctx.setLineDash([]);
        });
    }

    private drawNodes(
        nodes: VisualizationNode[], 
        zoom: number, 
        pan: { x: number; y: number }, 
        nodeColors: Record<NodeType, string>
    ) {
        nodes.forEach(node => {
            const { x, y, width, height, label, color, isSelected, type } = node;

            // Apply zoom and pan transformations
            const transformedX = (x + pan.x) * zoom;
            const transformedY = (y + pan.y) * zoom;
            const transformedWidth = width * zoom;
            const transformedHeight = height * zoom;

            // Draw node background
            this.ctx.fillStyle = color;
            this.ctx.strokeStyle = isSelected ? '#ff4d4f' : '#d9d9d9';
            this.ctx.lineWidth = isSelected ? 3 : 1;

            // Rounded rectangle
            const radius = 8 * zoom;
            this.ctx.beginPath();
            this.ctx.roundRect(transformedX, transformedY, transformedWidth, transformedHeight, radius);
            this.ctx.fill();
            this.ctx.stroke();

            // Draw geometric icon
            const iconSize = 20 * zoom;
            const iconX = transformedX + (transformedWidth - iconSize) / 2;
            const iconY = transformedY + 10 * zoom;

            drawGeometricIcon(this.ctx, type, iconX, iconY, iconSize, nodeColors);

            // Draw label
            this.ctx.fillStyle = '#ffffff';
            this.ctx.font = `${12 * zoom}px "Segoe UI", sans-serif`;
            this.ctx.textAlign = 'center';

            // Truncate label if too long
            const maxWidth = transformedWidth - 10 * zoom;
            let displayLabel = label;
            const metrics = this.ctx.measureText(displayLabel);
            if (metrics.width > maxWidth) {
                while (this.ctx.measureText(displayLabel + '...').width > maxWidth && displayLabel.length > 0) {
                    displayLabel = displayLabel.slice(0, -1);
                }
                displayLabel += '...';
            }

            this.ctx.fillText(displayLabel, transformedX + transformedWidth / 2, transformedY + transformedHeight - 8 * zoom);
        });
    }
}