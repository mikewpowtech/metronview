import { Company } from '../features/companies/companyAPI';
import { Unit } from '../features/units/unitsAPI';
import { Alarm } from '../features/alarms/alarmAPI';
import { Trigger } from '../features/triggers/triggerAPI';

// Re-export the types for easier importing
export type { Company, Unit, Alarm, Trigger };

// Node types for the visualization
export enum NodeType {
    COMPANY = 'company',
    UNIT = 'unit',
    ALARM = 'alarm',
    TRIGGER = 'trigger'
}

// Interface for visualization nodes
export interface VisualizationNode {
    id: string;
    type: NodeType;
    x: number;
    y: number;
    width: number;
    height: number;
    label: string;
    data: Company | Unit | Alarm | Trigger;
    connections: string[]; // IDs of connected nodes
    color: string;
    isSelected: boolean;
    isDragging: boolean;
}

// Interface for connections between nodes
export interface NodeConnection {
    fromId: string;
    toId: string;
    color: string;
    strokeWidth: number;
}

// Statistics interface
export interface VisualizationStats {
    companies: number;
    units: number;
    alarms: number;
    triggers: number;
    activeAlarms: number;
    enabledTriggers: number;
}