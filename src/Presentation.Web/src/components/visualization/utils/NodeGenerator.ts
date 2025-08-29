import { Company } from '../../../features/companies/companyAPI';
import { Unit } from '../../../features/units/unitsAPI';
import { Alarm } from '../../../features/alarms/alarmAPI';
import { Trigger } from '../../../features/triggers/triggerAPI';
import { VisualizationNode, NodeConnection, NodeType } from '../../../types/visualization';

export interface NodeGeneratorParams {
    companiesData: Company[];
    unitsData: Unit[];
    alarmsData: Alarm[];
    triggersData: Trigger[];
    selectedCompany: number | null;
    nodeColors: Record<NodeType, string>;
}

export class NodeGenerator {
    public static generateNodes({
        companiesData,
        unitsData,
        alarmsData,
        triggersData,
        selectedCompany,
        nodeColors
    }: NodeGeneratorParams): { nodes: VisualizationNode[]; connections: NodeConnection[] } {
        const nodes: VisualizationNode[] = [];
        const connections: NodeConnection[] = [];

        // Filter data by selected company if applicable
        const filteredCompanies = selectedCompany
            ? companiesData.filter(c => c.id === selectedCompany)
            : companiesData.slice(0, 10);

        const filteredUnits = selectedCompany
            ? unitsData.filter(u => u.companyId === selectedCompany)
            : unitsData.slice(0, 20);

        const filteredAlarms = selectedCompany
            ? alarmsData.filter(a => a.companyId === selectedCompany)
            : alarmsData.slice(0, 15);

        // Generate company nodes
        this.generateCompanyNodes(filteredCompanies, nodes, nodeColors);
        
        // Generate unit nodes and connections
        this.generateUnitNodes(filteredUnits, nodes, connections, nodeColors);
        
        // Generate alarm nodes and connections
        this.generateAlarmNodes(filteredAlarms, nodes, connections, nodeColors);
        
        // Generate trigger nodes and connections
        this.generateTriggerNodes(triggersData, alarmsData, selectedCompany, nodes, connections, nodeColors);

        return { nodes, connections };
    }

    private static generateCompanyNodes(
        companies: Company[], 
        nodes: VisualizationNode[], 
        nodeColors: Record<NodeType, string>
    ) {
        companies.forEach((company, index) => {
            const node: VisualizationNode = {
                id: `company-${company.id}`,
                type: NodeType.COMPANY,
                x: 100 + (index % 4) * 300,
                y: 100 + Math.floor(index / 4) * 200,
                width: 120,
                height: 80,
                label: company.name,
                data: company,
                connections: [],
                color: nodeColors[NodeType.COMPANY],
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);
        });
    }

    private static generateUnitNodes(
        units: Unit[], 
        nodes: VisualizationNode[], 
        connections: NodeConnection[], 
        nodeColors: Record<NodeType, string>
    ) {
        units.forEach((unit, index) => {
            const node: VisualizationNode = {
                id: `unit-${unit.id}`,
                type: NodeType.UNIT,
                x: 400 + (index % 5) * 180,
                y: 300 + Math.floor(index / 5) * 150,
                width: 100,
                height: 60,
                label: unit.unitCode || `Unit ${unit.id}`,
                data: unit,
                connections: [],
                color: nodeColors[NodeType.UNIT],
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);

            // Connect to company if exists
            if (unit.companyId) {
                const companyNodeId = `company-${unit.companyId}`;
                if (nodes.find(n => n.id === companyNodeId)) {
                    connections.push({
                        fromId: companyNodeId,
                        toId: node.id,
                        color: '#d9d9d9',
                        strokeWidth: 2
                    });
                    node.connections.push(companyNodeId);
                }
            }
        });
    }

    private static generateAlarmNodes(
        alarms: Alarm[], 
        nodes: VisualizationNode[], 
        connections: NodeConnection[], 
        nodeColors: Record<NodeType, string>
    ) {
        alarms.forEach((alarm, index) => {
            const node: VisualizationNode = {
                id: `alarm-${alarm.id}`,
                type: NodeType.ALARM,
                x: 200 + (index % 3) * 250,
                y: 500 + Math.floor(index / 3) * 120,
                width: 110,
                height: 70,
                label: alarm.name,
                data: alarm,
                connections: [],
                color: alarm.isActive ? nodeColors[NodeType.ALARM] : '#bfbfbf',
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);

            // Connect to company
            const companyNodeId = `company-${alarm.companyId}`;
            if (nodes.find(n => n.id === companyNodeId)) {
                connections.push({
                    fromId: companyNodeId,
                    toId: node.id,
                    color: '#ffa940',
                    strokeWidth: 2
                });
                node.connections.push(companyNodeId);
            }
        });
    }

    private static generateTriggerNodes(
        triggersData: Trigger[],
        alarmsData: Alarm[],
        selectedCompany: number | null,
        nodes: VisualizationNode[], 
        connections: NodeConnection[], 
        nodeColors: Record<NodeType, string>
    ) {
        const alarmTriggers = selectedCompany
            ? triggersData.filter(t => {
                const alarm = alarmsData.find(a => a.id === t.alarmId);
                return alarm && alarm.companyId === selectedCompany;
            })
            : triggersData.slice(0, 20);

        alarmTriggers.forEach((trigger, index) => {
            const node: VisualizationNode = {
                id: `trigger-${trigger.id}`,
                type: NodeType.TRIGGER,
                x: 600 + (index % 4) * 150,
                y: 650 + Math.floor(index / 4) * 100,
                width: 90,
                height: 50,
                label: `Trigger ${trigger.id}`,
                data: trigger,
                connections: [],
                color: trigger.isEnabled ? nodeColors[NodeType.TRIGGER] : '#bfbfbf',
                isSelected: false,
                isDragging: false
            };
            nodes.push(node);

            // Connect to alarm
            const alarmNodeId = `alarm-${trigger.alarmId}`;
            if (nodes.find(n => n.id === alarmNodeId)) {
                connections.push({
                    fromId: alarmNodeId,
                    toId: node.id,
                    color: '#b37feb',
                    strokeWidth: 2
                });
                node.connections.push(alarmNodeId);
            }
        });
    }
}