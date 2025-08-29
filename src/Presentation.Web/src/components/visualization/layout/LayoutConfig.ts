import { NodeType } from '../../../types/visualization';

export interface LayoutConfig {
    startX: number;
    startY: number;
    columns: number;
    horizontalSpacing: number;
    verticalSpacing: number;
    nodeWidth: number;
    nodeHeight: number;
}

export interface LayoutConfigs {
    [NodeType.COMPANY]: LayoutConfig;
    [NodeType.UNIT]: LayoutConfig;
    [NodeType.ALARM]: LayoutConfig;
    [NodeType.TRIGGER]: LayoutConfig;
}

export const DEFAULT_LAYOUT_CONFIGS: LayoutConfigs = {
    [NodeType.COMPANY]: {
        startX: 100,
        startY: 100,
        columns: 4,
        horizontalSpacing: 300,
        verticalSpacing: 200,
        nodeWidth: 120,
        nodeHeight: 80
    },
    [NodeType.UNIT]: {
        startX: 400,
        startY: 300,
        columns: 5,
        horizontalSpacing: 180,
        verticalSpacing: 150,
        nodeWidth: 100,
        nodeHeight: 60
    },
    [NodeType.ALARM]: {
        startX: 200,
        startY: 500,
        columns: 3,
        horizontalSpacing: 250,
        verticalSpacing: 120,
        nodeWidth: 110,
        nodeHeight: 70
    },
    [NodeType.TRIGGER]: {
        startX: 600,
        startY: 650,
        columns: 4,
        horizontalSpacing: 150,
        verticalSpacing: 100,
        nodeWidth: 90,
        nodeHeight: 50
    }
};