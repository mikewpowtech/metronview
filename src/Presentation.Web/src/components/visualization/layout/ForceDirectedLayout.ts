import { VisualizationNode, NodeConnection } from '../../../types/visualization';
import { Position } from './PositionCalculator';

export interface ForceDirectedConfig {
    width: number;
    height: number;
    iterations: number;
    temperature: number;
    coolingFactor: number;
    repulsionStrength: number;
    attractionStrength: number;
    optimalDistance: number;
    centeringForce: number;
    damping: number;
}

export const DEFAULT_FORCE_CONFIG: ForceDirectedConfig = {
    width: 1200,
    height: 800,
    iterations: 300,
    temperature: 100,
    coolingFactor: 0.95,
    repulsionStrength: 50000,
    attractionStrength: 0.8,
    optimalDistance: 150,
    centeringForce: 0.02,
    damping: 0.85
};

export interface NodeForce {
    id: string;
    x: number;
    y: number;
    vx: number; // velocity x
    vy: number; // velocity y
    fx: number; // force x
    fy: number; // force y
    mass: number;
    fixed: boolean;
}

export class ForceDirectedLayout {
    private config: ForceDirectedConfig;
    private nodeForces: Map<string, NodeForce> = new Map();
    private connections: NodeConnection[] = [];
    private temperature: number;
    private iteration: number = 0;

    constructor(config: ForceDirectedConfig = DEFAULT_FORCE_CONFIG) {
        this.config = { ...config };
        this.temperature = config.temperature;
    }

    public calculateLayout(
        nodes: VisualizationNode[], 
        connections: NodeConnection[],
        animated: boolean = false
    ): VisualizationNode[] {
        this.initializeForces(nodes);
        this.connections = connections;
        this.iteration = 0;
        this.temperature = this.config.temperature;

        if (animated) {
            // Return current positions for animated layout
            return this.applyForcesToNodes(nodes);
        } else {
            // Run full simulation
            for (let i = 0; i < this.config.iterations; i++) {
                this.simulateStep();
            }
            return this.applyForcesToNodes(nodes);
        }
    }

    public simulateStep(): void {
        if (this.iteration >= this.config.iterations) return;

        // Reset forces
        this.nodeForces.forEach(node => {
            node.fx = 0;
            node.fy = 0;
        });

        // Apply repulsion forces (all nodes repel each other)
        this.applyRepulsionForces();

        // Apply attraction forces (connected nodes attract)
        this.applyAttractionForces();

        // Apply centering force (prevent nodes from drifting too far)
        this.applyCenteringForce();

        // Update positions based on forces
        this.updatePositions();

        // Cool down the system
        this.temperature *= this.config.coolingFactor;
        this.iteration++;
    }

    private initializeForces(nodes: VisualizationNode[]): void {
        this.nodeForces.clear();
        
        nodes.forEach((node, index) => {
            // Initialize with random positions if not set, or use current positions
            const angle = (index / nodes.length) * 2 * Math.PI;
            const radius = Math.min(this.config.width, this.config.height) * 0.3;
            
            this.nodeForces.set(node.id, {
                id: node.id,
                x: node.x || (this.config.width / 2 + Math.cos(angle) * radius),
                y: node.y || (this.config.height / 2 + Math.sin(angle) * radius),
                vx: 0,
                vy: 0,
                fx: 0,
                fy: 0,
                mass: this.getNodeMass(node),
                fixed: false
            });
        });
    }

    private getNodeMass(node: VisualizationNode): number {
        // Larger nodes have more mass (companies > units > alarms > triggers)
        const massMap = {
            company: 3.0,
            unit: 2.0,
            alarm: 1.5,
            trigger: 1.0
        };
        return massMap[node.type] || 1.0;
    }

    private applyRepulsionForces(): void {
        const nodes = Array.from(this.nodeForces.values());
        
        for (let i = 0; i < nodes.length; i++) {
            for (let j = i + 1; j < nodes.length; j++) {
                const node1 = nodes[i];
                const node2 = nodes[j];
                
                const dx = node1.x - node2.x;
                const dy = node1.y - node2.y;
                const distance = Math.sqrt(dx * dx + dy * dy);
                
                if (distance < 0.1) continue; // Avoid division by zero
                
                // Coulomb's law: F = k * (m1 * m2) / r^2
                const force = this.config.repulsionStrength * (node1.mass * node2.mass) / (distance * distance);
                
                const fx = (dx / distance) * force;
                const fy = (dy / distance) * force;
                
                node1.fx += fx;
                node1.fy += fy;
                node2.fx -= fx;
                node2.fy -= fy;
            }
        }
    }

    private applyAttractionForces(): void {
        this.connections.forEach(connection => {
            const node1 = this.nodeForces.get(connection.fromId);
            const node2 = this.nodeForces.get(connection.toId);
            
            if (!node1 || !node2) return;
            
            const dx = node2.x - node1.x;
            const dy = node2.y - node1.y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance < 0.1) return;
            
            // Hooke's law: F = k * (d - l0)
            const displacement = distance - this.config.optimalDistance;
            const force = this.config.attractionStrength * displacement;
            
            const fx = (dx / distance) * force;
            const fy = (dy / distance) * force;
            
            node1.fx += fx;
            node1.fy += fy;
            node2.fx -= fx;
            node2.fy -= fy;
        });
    }

    private applyCenteringForce(): void {
        const centerX = this.config.width / 2;
        const centerY = this.config.height / 2;
        
        this.nodeForces.forEach(node => {
            const dx = centerX - node.x;
            const dy = centerY - node.y;
            
            node.fx += dx * this.config.centeringForce;
            node.fy += dy * this.config.centeringForce;
        });
    }

    private updatePositions(): void {
        this.nodeForces.forEach(node => {
            if (node.fixed) return;
            
            // Update velocity with damping
            node.vx = (node.vx + node.fx / node.mass) * this.config.damping;
            node.vy = (node.vy + node.fy / node.mass) * this.config.damping;
            
            // Limit velocity based on temperature
            const maxVelocity = this.temperature;
            const velocity = Math.sqrt(node.vx * node.vx + node.vy * node.vy);
            if (velocity > maxVelocity) {
                node.vx = (node.vx / velocity) * maxVelocity;
                node.vy = (node.vy / velocity) * maxVelocity;
            }
            
            // Update position
            node.x += node.vx;
            node.y += node.vy;
            
            // Keep nodes within bounds
            const margin = 50;
            node.x = Math.max(margin, Math.min(this.config.width - margin, node.x));
            node.y = Math.max(margin, Math.min(this.config.height - margin, node.y));
        });
    }

    private applyForcesToNodes(nodes: VisualizationNode[]): VisualizationNode[] {
        return nodes.map(node => {
            const force = this.nodeForces.get(node.id);
            if (force) {
                return {
                    ...node,
                    x: force.x,
                    y: force.y
                };
            }
            return node;
        });
    }

    public getProgress(): number {
        return Math.min(this.iteration / this.config.iterations, 1);
    }

    public isComplete(): boolean {
        return this.iteration >= this.config.iterations;
    }

    public getTemperature(): number {
        return this.temperature;
    }
}