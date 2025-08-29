export interface IconDrawParams {
    ctx: CanvasRenderingContext2D;
    centerX: number;
    centerY: number;
    size: number;
    nodeColor: string;
}

export type IconDrawFunction = (params: IconDrawParams) => void;