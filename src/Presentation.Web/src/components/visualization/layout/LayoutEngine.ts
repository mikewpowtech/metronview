import { Company } from '../../../features/companies/companyAPI';
import { Unit } from '../../../features/units/unitsAPI';
import { Alarm } from '../../../features/alarms/alarmAPI';
import { Trigger } from '../../../features/triggers/triggerAPI';
import { NodeType, VisualizationNode, NodeConnection } from '../../../types/visualization';
import { DEFAULT_LAYOUT_CONFIGS, LayoutConfigs } from './LayoutConfig';
import { PositionCalculator } from './PositionCalculator';
import { ForceDirectedLayout, DEFAULT_FORCE_CONFIG, ForceDirectedConfig } from './ForceDirectedLayout';

export enum LayoutType {
    GRID = 'grid',
    HIERARCHICAL = 'hierarchical',
    CLUSTERED = 'clustered',
    FORCE_DIRECTED = 'force-directed'
}

export interface LayoutEngineParams {
    companiesData: Company[];
    unitsData: Unit[];
    alarmsData: Alarm[];
    triggersData: Trigger[];
    selectedCompany: number | null;
    nodeColors: Record<NodeType, string>;
    layoutConfigs?: LayoutConfigs;
    layoutType?: LayoutType;
    forceConfig?: ForceDirectedConfig;
    canvasSize?: { width: number; height: number };
}

export class LayoutEngine {
    private layoutConfigs: LayoutConfigs;
    private layoutType: LayoutType;
    private forceLayout: ForceDirectedLayout | null = null;

    constructor(layoutConfigs: LayoutConfigs = DEFAULT_LAYOUT_CONFIGS, layoutType: LayoutType = LayoutType.GRID) {
        this.layoutConfigs = layoutConfigs;
        this.layoutType = layoutType;
    }

    public generateLayout(params: LayoutEngineParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        switch (this.layoutType) {
            case LayoutType.GRID:
                return this.generateGridLayout(params);
            case LayoutType.HIERARCHICAL:
                return this.generateHierarchicalLayout(params);
            case LayoutType.CLUSTERED:
                return this.generateClusteredLayout(params);
            case LayoutType.FORCE_DIRECTED:
                return this.generateForceDirectedLayout(params);
            default:
                return this.generateGridLayout(params);
        }
    }

    private generateForceDirectedLayout(params: LayoutEngineParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        const { companiesData, unitsData, alarmsData, triggersData, selectedCompany, nodeColors, forceConfig, canvasSize } = params;
        
        // First, generate nodes using grid layout as starting positions
        const gridResult = this.generateGridLayout(params);
        
        // Configure force-directed algorithm
        const config: ForceDirectedConfig = {
            ...DEFAULT_FORCE_CONFIG,
            ...forceConfig,
            width: canvasSize?.width || DEFAULT_FORCE_CONFIG.width,
            height: canvasSize?.height || DEFAULT_FORCE_CONFIG.height
        };
        
        // Initialize force layout
        this.forceLayout = new ForceDirectedLayout(config);
        
        // Apply force-directed positioning
        const forceNodes = this.forceLayout.calculateLayout(gridResult.nodes, gridResult.connections);
        
        return {
            nodes: forceNodes,
            connections: gridResult.connections
        };
    }

    public simulateForceStep(): VisualizationNode[] | null {
        if (!this.forceLayout) return null;
        
        this.forceLayout.simulateStep();
        return null; // Return current state if needed
    }

    public isForceSimulationComplete(): boolean {
        return this.forceLayout?.isComplete() || false;
    }

    public getForceSimulationProgress(): number {
        return this.forceLayout?.getProgress() || 0;
    }

    private generateGridLayout(params: LayoutEngineParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        const { companiesData, unitsData, alarmsData, triggersData, selectedCompany, nodeColors } = params;
        const nodes: VisualizationNode[] = [];
        const connections: NodeConnection[] = [];

        // Filter data
        const filteredCompanies = this.filterCompaniesBySelection(companiesData, selectedCompany);
        const filteredUnits = this.filterUnitsBySelection(unitsData, selectedCompany);
        const filteredAlarms = this.filterAlarmsBySelection(alarmsData, selectedCompany);
        const filteredTriggers = this.filterTriggersBySelection(triggersData, alarmsData, selectedCompany);

        // Generate nodes using grid positioning
        this.generateCompanyNodes(filteredCompanies, nodes, nodeColors);
        this.generateUnitNodes(filteredUnits, nodes, connections, nodeColors);
        this.generateAlarmNodes(filteredAlarms, nodes, connections, nodeColors);
        this.generateTriggerNodes(filteredTriggers, nodes, connections, nodeColors);

        return { nodes, connections };
    }

    private generateHierarchicalLayout(params: LayoutEngineParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        const { companiesData, unitsData, alarmsData, triggersData, selectedCompany, nodeColors } = params;
        const nodes: VisualizationNode[] = [];
        const connections: NodeConnection[] = [];

        // Filter data
        const filteredCompanies = this.filterCompaniesBySelection(companiesData, selectedCompany);
        const filteredUnits = this.filterUnitsBySelection(unitsData, selectedCompany);
        const filteredAlarms = this.filterAlarmsBySelection(alarmsData, selectedCompany);

        // Generate hierarchical layout
        // Level 0: Companies
        filteredCompanies.forEach((company, index) => {
            const position = PositionCalculator.calculateHierarchicalPosition(0, index, filteredCompanies.length);
            const node = this.createCompanyNode(company, position.x + 600, position.y + 100, nodeColors);
            nodes.push(node);
        });

        // Level 1: Units grouped by company
        let unitIndex = 0;
        filteredCompanies.forEach((company) => {
            const companyUnits = filteredUnits.filter(u => u.companyId === company.id);
            companyUnits.forEach((unit, localIndex) => {
                const position = PositionCalculator.calculateHierarchicalPosition(1, unitIndex, filteredUnits.length);
                const node = this.createUnitNode(unit, position.x + 600, position.y + 300, nodeColors);
                nodes.push(node);

                // Connect to company
                this.createConnection(nodes, connections, `company-${company.id}`, node.id, '#d9d9d9');
                unitIndex++;
            });
        });

        // Continue with alarms and triggers...
        this.generateAlarmNodes(filteredAlarms, nodes, connections, nodeColors);
        this.generateTriggerNodes(this.filterTriggersBySelection(triggersData, alarmsData, selectedCompany), nodes, connections, nodeColors);

        return { nodes, connections };
    }

    private generateClusteredLayout(params: LayoutEngineParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        const { companiesData, unitsData, alarmsData, triggersData, selectedCompany, nodeColors } = params;
        const nodes: VisualizationNode[] = [];
        const connections: NodeConnection[] = [];

        // Filter data
        const filteredCompanies = this.filterCompaniesBySelection(companiesData, selectedCompany);
        const filteredUnits = this.filterUnitsBySelection(unitsData, selectedCompany);

        // Generate company nodes in center
        filteredCompanies.forEach((company, index) => {
            const position = PositionCalculator.calculateGridPosition(index, this.layoutConfigs[NodeType.COMPANY]);
            const node = this.createCompanyNode(company, position.x, position.y, nodeColors);
            nodes.push(node);

            // Cluster units around each company
            const companyUnits = filteredUnits.filter(u => u.companyId === company.id);
            companyUnits.forEach((unit, unitIndex) => {
                const unitPosition = PositionCalculator.calculateClusteredPosition(
                    position, 
                    unitIndex, 
                    120, 
                    companyUnits.length
                );
                const unitNode = this.createUnitNode(unit, unitPosition.x, unitPosition.y, nodeColors);
                nodes.push(unitNode);

                // Connect unit to company
                this.createConnection(nodes, connections, node.id, unitNode.id, '#d9d9d9');
            });
        });

        return { nodes, connections };
    }

    // Helper methods for filtering data
    private filterCompaniesBySelection(companies: Company[], selectedCompany: number | null): Company[] {
        return selectedCompany
            ? companies.filter(c => c.id === selectedCompany)
            : companies.slice(0, 10);
    }

    private filterUnitsBySelection(units: Unit[], selectedCompany: number | null): Unit[] {
        return selectedCompany
            ? units.filter(u => u.companyId === selectedCompany)
            : units.slice(0, 20);
    }

    private filterAlarmsBySelection(alarms: Alarm[], selectedCompany: number | null): Alarm[] {
        return selectedCompany
            ? alarms.filter(a => a.companyId === selectedCompany)
            : alarms.slice(0, 15);
    }

    private filterTriggersBySelection(triggers: Trigger[], alarms: Alarm[], selectedCompany: number | null): Trigger[] {
        return selectedCompany
            ? triggers.filter(t => {
                const alarm = alarms.find(a => a.id === t.alarmId);
                return alarm && alarm.companyId === selectedCompany;
            })
            : triggers.slice(0, 20);
    }

    // Node creation methods
    private createCompanyNode(company: Company, x: number, y: number, nodeColors: Record<NodeType, string>): VisualizationNode {
        const config = this.layoutConfigs[NodeType.COMPANY];
        return {
            id: `company-${company.id}`,
            type: NodeType.COMPANY,
            x,
            y,
            width: config.nodeWidth,
            height: config.nodeHeight,
            label: company.name,
            data: company,
            connections: [],
            color: nodeColors[NodeType.COMPANY],
            isSelected: false,
            isDragging: false
        };
    }

    private createUnitNode(unit: Unit, x: number, y: number, nodeColors: Record<NodeType, string>): VisualizationNode {
        const config = this.layoutConfigs[NodeType.UNIT];
        return {
            id: `unit-${unit.id}`,
            type: NodeType.UNIT,
            x,
            y,
            width: config.nodeWidth,
            height: config.nodeHeight,
            label: unit.unitCode || `Unit ${unit.id}`,
            data: unit,
            connections: [],
            color: nodeColors[NodeType.UNIT],
            isSelected: false,
            isDragging: false
        };
    }

    // Grid-based node generation methods (original logic)
    private generateCompanyNodes(companies: Company[], nodes: VisualizationNode[], nodeColors: Record<NodeType, string>): void {
        const config = this.layoutConfigs[NodeType.COMPANY];
        companies.forEach((company, index) => {
            const position = PositionCalculator.calculateGridPosition(index, config);
            const node = this.createCompanyNode(company, position.x, position.y, nodeColors);
            nodes.push(node);
        });
    }

    private generateUnitNodes(units: Unit[], nodes: VisualizationNode[], connections: NodeConnection[], nodeColors: Record<NodeType, string>): void {
        const config = this.layoutConfigs[NodeType.UNIT];
        units.forEach((unit, index) => {
            const position = PositionCalculator.calculateGridPosition(index, config);
            const node = this.createUnitNode(unit, position.x, position.y, nodeColors);
            nodes.push(node);

            // Connect to company if exists
            if (unit.companyId) {
                this.createConnection(nodes, connections, `company-${unit.companyId}`, node.id, '#d9d9d9');
            }
        });
    }

    private generateAlarmNodes(alarms: Alarm[], nodes: VisualizationNode[], connections: NodeConnection[], nodeColors: Record<NodeType, string>): void {
        const config = this.layoutConfigs[NodeType.ALARM];
        alarms.forEach((alarm, index) => {
            const position = PositionCalculator.calculateGridPosition(index, config);
            const node: VisualizationNode = {
                id: `alarm-${alarm.id}`,
                type: NodeType.ALARM,
                x: position.x,
                y: position.y,
                width: config.nodeWidth,
                height: config.nodeHeight,
                label: alarm.name,
                data: alarm,
                connections: [],
                color: alarm.isActive ? nodeColors[NodeType.ALARM] : '#bfbfbf',
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);

            // Connect to company
            this.createConnection(nodes, connections, `company-${alarm.companyId}`, node.id, '#ffa940');
        });
    }

    private generateTriggerNodes(triggers: Trigger[], nodes: VisualizationNode[], connections: NodeConnection[], nodeColors: Record<NodeType, string>): void {
        const config = this.layoutConfigs[NodeType.TRIGGER];
        triggers.forEach((trigger, index) => {
            const position = PositionCalculator.calculateGridPosition(index, config);
            const node: VisualizationNode = {
                id: `trigger-${trigger.id}`,
                type: NodeType.TRIGGER,
                x: position.x,
                y: position.y,
                width: config.nodeWidth,
                height: config.nodeHeight,
                label: `Trigger ${trigger.id}`,
                data: trigger,
                connections: [],
                color: trigger.isEnabled ? nodeColors[NodeType.TRIGGER] : '#bfbfbf',
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);

            // Connect to alarm
            this.createConnection(nodes, connections, `alarm-${trigger.alarmId}`, node.id, '#b37feb');
        });
    }

    private createConnection(nodes: VisualizationNode[], connections: NodeConnection[], fromId: string, toId: string, color: string): void {
        if (nodes.find(n => n.id === fromId)) {
            connections.push({
                fromId,
                toId,
                color,
                strokeWidth: 2
            });
            const toNode = nodes.find(n => n.id === toId);
            if (toNode) {
                toNode.connections.push(fromId);
            }
        }
    }
}