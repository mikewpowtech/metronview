import { useRef, useEffect, useState } from 'react';
import { CanvasDrawingEngine } from '../components/visualization/canvas/CanvasDrawingEngine';
import { VisualizationNode, NodeConnection, NodeType } from '../types/visualization';

export const useVisualizationCanvas = () => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);
    const [canvasSize, setCanvasSize] = useState({ width: 1200, height: 600 });
    const [drawingEngine, setDrawingEngine] = useState<CanvasDrawingEngine | null>(null);

    useEffect(() => {
        if (canvasRef.current) {
            setDrawingEngine(new CanvasDrawingEngine(canvasRef.current));
        }
    }, [canvasRef.current]);

    const calculateCanvasSize = () => {
        if (!containerRef.current) return;

        const container = containerRef.current;
        const canvasContainer = container.querySelector('.visualization-canvas-container') as HTMLElement;
        
        if (canvasContainer) {
            const rect = canvasContainer.getBoundingClientRect();
            const newWidth = Math.max(800, rect.width - 4);
            const newHeight = Math.max(400, rect.height - 4);
            
            setCanvasSize({
                width: newWidth,
                height: newHeight
            });
        }
    };

    const drawCanvas = (
        nodes: VisualizationNode[],
        connections: NodeConnection[],
        zoom: number,
        pan: { x: number; y: number },
        nodeColors: Record<NodeType, string>
    ) => {
        if (!canvasRef.current || !drawingEngine) return;

        drawingEngine.draw({
            canvas: canvasRef.current,
            nodes,
            connections,
            zoom,
            pan,
            nodeColors
        });
    };

    return {
        canvasRef,
        containerRef,
        canvasSize,
        calculateCanvasSize,
        drawCanvas
    };
};