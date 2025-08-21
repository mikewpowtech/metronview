import React, { useRef, useEffect, useState, useCallback } from 'react';
import { Card, Select, Button, Space, Tooltip, Badge, message } from 'antd';
import {
    HomeOutlined,
    DatabaseOutlined,
    AlertOutlined,
    ThunderboltOutlined,
    ZoomInOutlined,
    ZoomOutOutlined,
    ReloadOutlined,
    SaveOutlined,
    FullscreenOutlined
} from '@ant-design/icons';
import { useAppSelector } from '../app/hooks';
import { selectAuth } from '../app/store';
import { fetchCompanies, Company } from '../features/companies/companyAPI';
import { fetchUnits, Unit } from '../features/units/unitsAPI';
import { fetchAlarms, Alarm } from '../features/alarms/alarmAPI';
import { fetchTriggers, Trigger } from '../features/triggers/triggerAPI';
import './Visualisation.scss';

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

const Visualization: React.FC = () => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);
    const auth = useAppSelector(selectAuth);

    // State management
    const [nodes, setNodes] = useState<VisualizationNode[]>([]);
    const [connections, setConnections] = useState<NodeConnection[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [units, setUnits] = useState<Unit[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [triggers, setTriggers] = useState<Trigger[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
    const [isDragging, setIsDragging] = useState(false);
    const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [pan, setPan] = useState({ x: 0, y: 0 });
    const [selectedCompany, setSelectedCompany] = useState<number | null>(null);
    const [canvasSize, setCanvasSize] = useState({ width: 1200, height: 800 });

    // Node colors by type
    const nodeColors = {
        [NodeType.COMPANY]: '#1890ff',
        [NodeType.UNIT]: '#52c41a',
        [NodeType.ALARM]: '#fa541c',
        [NodeType.TRIGGER]: '#722ed1'
    };

    // Load data from APIs
    const loadData = async () => {
        if (!auth.accessToken) return;

        setLoading(true);
        try {
            const [companiesData, unitsData, alarmsData, triggersData] = await Promise.all([
                fetchCompanies(auth.accessToken),
                fetchUnits(auth.accessToken),
                fetchAlarms(auth.accessToken),
                fetchTriggers(auth.accessToken)
            ]);

            setCompanies(companiesData);
            setUnits(unitsData);
            setAlarms(alarmsData);
            setTriggers(triggersData);

            generateVisualizationNodes(companiesData, unitsData, alarmsData, triggersData);
        } catch (error) {
            console.error('Failed to load visualization data:', error);
            message.error('Failed to load data');
        } finally {
            setLoading(false);
        }
    };

    // Generate nodes from data
    const generateVisualizationNodes = (
        companiesData: Company[],
        unitsData: Unit[],
        alarmsData: Alarm[],
        triggersData: Trigger[]
    ) => {
        console.log('Generating visualization nodes...');
        
        const newNodes: VisualizationNode[] = [];
        const newConnections: NodeConnection[] = [];

        // Filter data by selected company if applicable
        const filteredCompanies = selectedCompany
            ? companiesData.filter(c => c.id === selectedCompany)
            : companiesData.slice(0, 10); // Limit for performance

        const filteredUnits = selectedCompany
            ? unitsData.filter(u => u.companyId === selectedCompany)
            : unitsData.slice(0, 20);

        const filteredAlarms = selectedCompany
            ? alarmsData.filter(a => a.companyId === selectedCompany)
            : alarmsData.slice(0, 15);

        // Create company nodes
        filteredCompanies.forEach((company, index) => {
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
            newNodes.push(node);
        });

        // Create unit nodes and connections to companies
        filteredUnits.forEach((unit, index) => {
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
            newNodes.push(node);

            // Connect to company if exists
            if (unit.companyId) {
                const companyNodeId = `company-${unit.companyId}`;
                if (newNodes.find(n => n.id === companyNodeId)) {
                    newConnections.push({
                        fromId: companyNodeId,
                        toId: node.id,
                        color: '#d9d9d9',
                        strokeWidth: 2
                    });
                    node.connections.push(companyNodeId);
                }
            }
        });

        // Create alarm nodes and connections
        filteredAlarms.forEach((alarm, index) => {
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
            newNodes.push(node);

            // Connect to company
            const companyNodeId = `company-${alarm.companyId}`;
            if (newNodes.find(n => n.id === companyNodeId)) {
                newConnections.push({
                    fromId: companyNodeId,
                    toId: node.id,
                    color: '#ffa940',
                    strokeWidth: 2
                });
                node.connections.push(companyNodeId);
            }
        });

        // Create trigger nodes and connections
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
            newNodes.push(node);

            // Connect to alarm
            const alarmNodeId = `alarm-${trigger.alarmId}`;
            if (newNodes.find(n => n.id === alarmNodeId)) {
                newConnections.push({
                    fromId: alarmNodeId,
                    toId: node.id,
                    color: '#b37feb',
                    strokeWidth: 2
                });
                node.connections.push(alarmNodeId);
            }
        });

        setNodes(newNodes);
        setConnections(newConnections);
    };

    // Canvas drawing functions
    const drawNode = (ctx: CanvasRenderingContext2D, node: VisualizationNode) => {
        const { x, y, width, height, label, color, isSelected, type } = node;

        // Apply zoom and pan transformations
        const transformedX = (x + pan.x) * zoom;
        const transformedY = (y + pan.y) * zoom;
        const transformedWidth = width * zoom;
        const transformedHeight = height * zoom;

        // Draw node background
        ctx.fillStyle = color;
        ctx.strokeStyle = isSelected ? '#ff4d4f' : '#d9d9d9';
        ctx.lineWidth = isSelected ? 3 : 1;

        // Rounded rectangle
        const radius = 8 * zoom;
        ctx.beginPath();
        ctx.roundRect(transformedX, transformedY, transformedWidth, transformedHeight, radius);
        ctx.fill();
        ctx.stroke();

        // Draw icon
        const iconSize = 20 * zoom;
        const iconX = transformedX + (transformedWidth - iconSize) / 2;
        const iconY = transformedY + 10 * zoom;

        ctx.fillStyle = '#ffffff';
        ctx.font = `${iconSize}px "Segoe UI", sans-serif`;
        ctx.textAlign = 'center';

        let icon = '';
        switch (type) {
            case NodeType.COMPANY: icon = '🏢'; break;
            case NodeType.UNIT: icon = '📟'; break;
            case NodeType.ALARM: icon = '🚨'; break;
            case NodeType.TRIGGER: icon = '⚡'; break;
        }

        ctx.fillText(icon, iconX + iconSize / 2, iconY + iconSize);

        // Draw label
        ctx.fillStyle = '#ffffff';
        ctx.font = `${12 * zoom}px "Segoe UI", sans-serif`;
        ctx.textAlign = 'center';

        // Truncate label if too long
        const maxWidth = transformedWidth - 10 * zoom;
        let displayLabel = label;
        const metrics = ctx.measureText(displayLabel);
        if (metrics.width > maxWidth) {
            while (ctx.measureText(displayLabel + '...').width > maxWidth && displayLabel.length > 0) {
                displayLabel = displayLabel.slice(0, -1);
            }
            displayLabel += '...';
        }

        ctx.fillText(displayLabel, transformedX + transformedWidth / 2, transformedY + transformedHeight - 8 * zoom);
    };

    const drawConnection = (ctx: CanvasRenderingContext2D, connection: NodeConnection) => {
        const fromNode = nodes.find(n => n.id === connection.fromId);
        const toNode = nodes.find(n => n.id === connection.toId);

        if (!fromNode || !toNode) return;

        const fromX = (fromNode.x + fromNode.width / 2 + pan.x) * zoom;
        const fromY = (fromNode.y + fromNode.height / 2 + pan.y) * zoom;
        const toX = (toNode.x + toNode.width / 2 + pan.x) * zoom;
        const toY = (toNode.y + toNode.height / 2 + pan.y) * zoom;

        ctx.strokeStyle = connection.color;
        ctx.lineWidth = connection.strokeWidth * zoom;
        ctx.setLineDash([5 * zoom, 5 * zoom]);

        ctx.beginPath();
        ctx.moveTo(fromX, fromY);
        ctx.lineTo(toX, toY);
        ctx.stroke();

        ctx.setLineDash([]); // Reset line dash
    };

    const drawCanvas = () => {
        const canvas = canvasRef.current;
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        if (!ctx) return;

        // Clear canvas
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Draw grid
        const gridSize = 50 * zoom;
        ctx.strokeStyle = '#f0f0f0';
        ctx.lineWidth = 1;

        for (let x = (pan.x * zoom) % gridSize; x < canvas.width; x += gridSize) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, canvas.height);
            ctx.stroke();
        }

        for (let y = (pan.y * zoom) % gridSize; y < canvas.height; y += gridSize) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(canvas.width, y);
            ctx.stroke();
        }

        // Draw connections first (behind nodes)
        connections.forEach(connection => drawConnection(ctx, connection));

        // Draw nodes
        nodes.forEach(node => drawNode(ctx, node));
    };

    // Mouse event handlers
    const getMousePos = (e: React.MouseEvent<HTMLCanvasElement> | MouseEvent) => {
        const canvas = canvasRef.current;
        if (!canvas) return { x: 0, y: 0 };

        const rect = canvas.getBoundingClientRect();
        return {
            x: (e.clientX - rect.left) / zoom - pan.x,
            y: (e.clientY - rect.top) / zoom - pan.y
        };
    };

    const getNodeAtPosition = (x: number, y: number): VisualizationNode | null => {
        // Check nodes in reverse order (top to bottom)
        for (let i = nodes.length - 1; i >= 0; i--) {
            const node = nodes[i];
            if (x >= node.x && x <= node.x + node.width &&
                y >= node.y && y <= node.y + node.height) {
                return node;
            }
        }
        return null;
    };

    const handleMouseDown = (e: React.MouseEvent<HTMLCanvasElement>) => {
        e.preventDefault();
        
        const mousePos = getMousePos(e);
        const clickedNode = getNodeAtPosition(mousePos.x, mousePos.y);

        console.log('Mouse down:', { mousePos, clickedNode: clickedNode?.id });

        if (clickedNode) {
            setSelectedNodeId(clickedNode.id);
            setIsDragging(true);
            setDragOffset({
                x: mousePos.x - clickedNode.x,
                y: mousePos.y - clickedNode.y
            });

            // Update node selection
            setNodes(prevNodes =>
                prevNodes.map(node => ({
                    ...node,
                    isSelected: node.id === clickedNode.id,
                    isDragging: node.id === clickedNode.id
                }))
            );
        } else {
            setSelectedNodeId(null);
            setIsDragging(false);
            setNodes(prevNodes =>
                prevNodes.map(node => ({
                    ...node,
                    isSelected: false,
                    isDragging: false
                }))
            );
        }
    };

    const handleMouseMove = (e: React.MouseEvent<HTMLCanvasElement>) => {
        if (!isDragging || !selectedNodeId) return;

        e.preventDefault();
        
        const mousePos = getMousePos(e);
        const newX = mousePos.x - dragOffset.x;
        const newY = mousePos.y - dragOffset.y;

        setNodes(prevNodes =>
            prevNodes.map(node => {
                if (node.id === selectedNodeId) {
                    return {
                        ...node,
                        x: newX,
                        y: newY
                    };
                }
                return node;
            })
        );
    };

    const handleMouseUp = () => {
        console.log('Mouse up:', { isDragging, selectedNodeId });
        
        setIsDragging(false);
        setNodes(prevNodes =>
            prevNodes.map(node => ({
                ...node,
                isDragging: false
            }))
        );
    };

    const handleWheel = (e: React.WheelEvent<HTMLCanvasElement>) => {
        e.preventDefault();
        const delta = e.deltaY > 0 ? 0.9 : 1.1;
        const newZoom = Math.min(Math.max(zoom * delta, 0.1), 3);
        setZoom(newZoom);
    };

    // Control functions
    const handleZoomIn = () => setZoom(prev => Math.min(prev * 1.2, 3));
    const handleZoomOut = () => setZoom(prev => Math.max(prev * 0.8, 0.1));
    const handleResetView = () => {
        setZoom(1);
        setPan({ x: 0, y: 0 });
    };

    const handleSaveLayout = () => {
        const layout = {
            nodes: nodes.map(node => ({
                id: node.id,
                x: node.x,
                y: node.y
            })),
            timestamp: new Date().toISOString()
        };

        localStorage.setItem('visualization-layout', JSON.stringify(layout));
        message.success('Layout saved successfully');
    };

    const handleFullscreen = () => {
        if (containerRef.current) {
            if (document.fullscreenElement) {
                document.exitFullscreen();
            } else {
                containerRef.current.requestFullscreen();
            }
        }
    };

    // Effects
    useEffect(() => {
        loadData();
    }, [auth.accessToken, selectedCompany]);

    useEffect(() => {
        drawCanvas();
    }, [nodes, connections, zoom, pan]);

    useEffect(() => {
        const handleResize = () => {
            if (containerRef.current) {
                const container = containerRef.current;
                
                // Calculate available space within the canvas container
                const canvasContainer = container.querySelector('.visualization-canvas-container') as HTMLElement;
                if (canvasContainer) {
                    const containerRect = canvasContainer.getBoundingClientRect();
                    
                    setCanvasSize({
                        width: Math.max(800, containerRect.width - 40),
                        height: Math.max(400, containerRect.height - 20)
                    });
                }
            }
        };

        const resizeObserver = new ResizeObserver(handleResize);
        if (containerRef.current) {
            resizeObserver.observe(containerRef.current);
        }

        setTimeout(handleResize, 100);

        return () => {
            resizeObserver.disconnect();
        };
    }, []);

    // Global mouse event handling for smooth dragging
    useEffect(() => {
        const handleGlobalMouseMove = (e: MouseEvent) => {
            if (!isDragging || !selectedNodeId || !canvasRef.current) return;
            
            const mousePos = getMousePos(e);
            const newX = mousePos.x - dragOffset.x;
            const newY = mousePos.y - dragOffset.y;

            setNodes(prevNodes =>
                prevNodes.map(node => {
                    if (node.id === selectedNodeId) {
                        return {
                            ...node,
                            x: newX,
                            y: newY
                        };
                    }
                    return node;
                })
            );
        };

        const handleGlobalMouseUp = () => {
            if (isDragging) {
                console.log('Global mouse up - ending drag');
                setIsDragging(false);
                setNodes(prevNodes =>
                    prevNodes.map(node => ({
                        ...node,
                        isDragging: false
                    }))
                );
            }
        };

        if (isDragging) {
            document.addEventListener('mousemove', handleGlobalMouseMove);
            document.addEventListener('mouseup', handleGlobalMouseUp);
        }

        return () => {
            document.removeEventListener('mousemove', handleGlobalMouseMove);
            document.removeEventListener('mouseup', handleGlobalMouseUp);
        };
    }, [isDragging, selectedNodeId, dragOffset]);

    // Statistics
    const stats = {
        companies: nodes.filter(n => n.type === NodeType.COMPANY).length,
        units: nodes.filter(n => n.type === NodeType.UNIT).length,
        alarms: nodes.filter(n => n.type === NodeType.ALARM).length,
        triggers: nodes.filter(n => n.type === NodeType.TRIGGER).length,
        activeAlarms: nodes.filter(n => n.type === NodeType.ALARM && (n.data as Alarm).isActive).length,
        enabledTriggers: nodes.filter(n => n.type === NodeType.TRIGGER && (n.data as Trigger).isEnabled).length
    };

    return (
        <div className="visualization-container" ref={containerRef}>
            <Card
                title="System Visualization"
                loading={loading}
                extra={
                    <Space>
                        <Select
                            style={{ width: 200 }}
                            placeholder="Filter by company"
                            allowClear
                            value={selectedCompany}
                            onChange={setSelectedCompany}
                        >
                            {companies.map(company => (
                                <Select.Option key={company.id} value={company.id}>
                                    {company.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Space>
                }
            >
                {/* Statistics Panel */}
                <div className="visualization-stats">
                    <Space size="large">
                        <Badge count={stats.companies} color="#1890ff">
                            <Tooltip title="Companies">
                                <HomeOutlined style={{ fontSize: 20 }} />
                            </Tooltip>
                        </Badge>
                        <Badge count={stats.units} color="#52c41a">
                            <Tooltip title="Units">
                                <DatabaseOutlined style={{ fontSize: 20 }} />
                            </Tooltip>
                        </Badge>
                        <Badge count={`${stats.activeAlarms}/${stats.alarms}`} color="#fa541c">
                            <Tooltip title="Active/Total Alarms">
                                <AlertOutlined style={{ fontSize: 20 }} />
                            </Tooltip>
                        </Badge>
                        <Badge count={`${stats.enabledTriggers}/${stats.triggers}`} color="#722ed1">
                            <Tooltip title="Enabled/Total Triggers">
                                <ThunderboltOutlined style={{ fontSize: 20 }} />
                            </Tooltip>
                        </Badge>
                    </Space>
                </div>

                {/* Control Panel */}
                <div className="visualization-controls">
                    <Space>
                        <Tooltip title="Zoom In">
                            <Button icon={<ZoomInOutlined />} onClick={handleZoomIn} />
                        </Tooltip>
                        <Tooltip title="Zoom Out">
                            <Button icon={<ZoomOutOutlined />} onClick={handleZoomOut} />
                        </Tooltip>
                        <Tooltip title="Reset View">
                            <Button icon={<ReloadOutlined />} onClick={handleResetView} />
                        </Tooltip>
                        <Tooltip title="Save Layout">
                            <Button icon={<SaveOutlined />} onClick={handleSaveLayout} />
                        </Tooltip>
                        <Tooltip title="Fullscreen">
                            <Button icon={<FullscreenOutlined />} onClick={handleFullscreen} />
                        </Tooltip>
                        <span>Zoom: {Math.round(zoom * 100)}%</span>
                    </Space>
                </div>

                {/* Canvas */}
                <div className="visualization-canvas-container">
                    <canvas
                        ref={canvasRef}
                        width={canvasSize.width}
                        height={canvasSize.height}
                        onMouseDown={handleMouseDown}
                        onMouseMove={handleMouseMove}
                        onMouseUp={handleMouseUp}
                        onMouseLeave={handleMouseUp}
                        onWheel={handleWheel}
                        style={{ 
                            border: '1px solid #d9d9d9', 
                            cursor: isDragging ? 'grabbing' : 'grab',
                            userSelect: 'none'
                        }}
                    />
                </div>

                {/* Selected Node Info */}
                {selectedNodeId && (
                    <div className="visualization-node-info">
                        {(() => {
                            const selectedNode = nodes.find(n => n.id === selectedNodeId);
                            if (!selectedNode) return null;

                            return (
                                <Card size="small" title={`${selectedNode.type.toUpperCase()}: ${selectedNode.label}`}>
                                    <pre>{JSON.stringify(selectedNode.data, null, 2)}</pre>
                                </Card>
                            );
                        })()}
                    </div>
                )}
            </Card>
        </div>
    );
};

export default Visualization;