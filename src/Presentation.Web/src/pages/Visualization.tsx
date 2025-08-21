import React, { useRef, useEffect, useState } from 'react';
import { Card, Select, Space, message, Form } from 'antd';
import { useAppSelector } from '../app/hooks';
import { selectAuth } from '../app/store';
import { fetchCompanies, Company, updateCompany } from '../features/companies/companyAPI';
import { fetchUnits, Unit, updateUnit } from '../features/units/unitsAPI';
import { fetchAlarms, Alarm, updateAlarm } from '../features/alarms/alarmAPI';
import { fetchTriggers, Trigger, updateTrigger } from '../features/triggers/triggerAPI';
import { fetchUnitModels, UnitModel } from '../features/unitmodels/unitModelAPI';
import { CompanyModal } from '../features/companies/CompanyModal';
import { UnitListModal } from '../features/units/UnitListModal';
import { AlarmModal } from '../features/alarms/AlarmModal';
import { TriggerModal } from '../features/triggers/TriggerModal';
import { VisualizationStats } from '../components/visualization/VisualizationStats';
import { VisualizationControls } from '../components/visualization/VisualizationControls';
import { NodeType, VisualizationNode, NodeConnection } from '../types/visualization';
import './Visualization.scss';

// Re-export types for backward compatibility
export { NodeType, type VisualizationNode, type NodeConnection };

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
    const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
    const [loading, setLoading] = useState(false);
    const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
    const [isDragging, setIsDragging] = useState(false);
    const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [pan, setPan] = useState({ x: 0, y: 0 });
    const [selectedCompany, setSelectedCompany] = useState<number | null>(null);
    const [canvasSize, setCanvasSize] = useState({ width: 1200, height: 600 });

    // Modal state management
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [editingNode, setEditingNode] = useState<VisualizationNode | null>(null);
    const [modalLoading, setModalLoading] = useState(false);
    const [clickTimeout, setClickTimeout] = useState<NodeJS.Timeout | null>(null);

    // Form instances for each modal type
    const [companyForm] = Form.useForm();
    const [unitForm] = Form.useForm();
    const [alarmForm] = Form.useForm();
    const [triggerForm] = Form.useForm();

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
            const [companiesData, unitsData, alarmsData, triggersData, unitModelsData] = await Promise.all([
                fetchCompanies(auth.accessToken),
                fetchUnits(auth.accessToken),
                fetchAlarms(auth.accessToken),
                fetchTriggers(auth.accessToken),
                fetchUnitModels(auth.accessToken)
            ]);

            setCompanies(companiesData);
            setUnits(unitsData);
            setAlarms(alarmsData);
            setTriggers(triggersData);
            setUnitModels(unitModelsData);

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

    // Helper function to draw simple geometric icons instead of emojis
    const drawGeometricIcon = (ctx: CanvasRenderingContext2D, type: NodeType, x: number, y: number, size: number) => {
        ctx.fillStyle = '#ffffff';
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 2;

        const centerX = x + size / 2;
        const centerY = y + size / 2;
        const radius = size * 0.3;

        switch (type) {
            case NodeType.COMPANY:
                // Draw a bigger building shape
                const buildingWidth = size * 0.8; // Increased from 0.6 to 0.8
                const buildingHeight = size * 0.9; // Increased from 0.7 to 0.9
                const buildingX = centerX - buildingWidth / 2;
                const buildingY = centerY - buildingHeight / 2;
                
                ctx.fillRect(buildingX, buildingY, buildingWidth, buildingHeight);
                // Add windows
                ctx.fillStyle = nodeColors[NodeType.COMPANY];
                const windowSize = buildingWidth * 0.15;
                for (let i = 0; i < 2; i++) {
                    for (let j = 0; j < 3; j++) {
                        ctx.fillRect(
                            buildingX + (i + 0.5) * (buildingWidth / 3),
                            buildingY + (j + 0.5) * (buildingHeight / 4),
                            windowSize,
                            windowSize
                        );
                    }
                }
                break;

            case NodeType.UNIT:
                // Draw a bigger device shape (rectangle with rounded corners)
                const deviceWidth = size * 0.9; // Increased from 0.7 to 0.9
                const deviceHeight = size * 0.7; // Increased from 0.5 to 0.7
                const deviceX = centerX - deviceWidth / 2;
                const deviceY = centerY - deviceHeight / 2;
                
                ctx.beginPath();
                ctx.roundRect(deviceX, deviceY, deviceWidth, deviceHeight, size * 0.1);
                ctx.fill();
                
                // Add a small screen indicator
                ctx.fillStyle = nodeColors[NodeType.UNIT];
                ctx.fillRect(deviceX + deviceWidth * 0.2, deviceY + deviceHeight * 0.2, deviceWidth * 0.6, deviceHeight * 0.3);
                break;

            case NodeType.ALARM:
                // Draw a bigger warning triangle
                const alarmRadius = size * 0.75; // Increased from 0.3 to 0.45 (50% bigger)
                ctx.beginPath();
                ctx.moveTo(centerX, centerY - alarmRadius);
                ctx.lineTo(centerX - alarmRadius * 0.866, centerY + alarmRadius * 0.5);
                ctx.lineTo(centerX + alarmRadius * 0.866, centerY + alarmRadius * 0.5);
                ctx.closePath();
                ctx.fill();
                
                // Add bigger exclamation mark
                ctx.fillStyle = nodeColors[NodeType.ALARM];
                ctx.fillRect(centerX - size * 0.07, centerY - size * 0.25, size * 0.14, size * 0.35); // Made wider and taller
                ctx.beginPath();
                ctx.arc(centerX, centerY + size * 0.2, size * 0.07, 0, Math.PI * 2); // Made bigger
                ctx.fill();
                break;

            case NodeType.TRIGGER:
                // Draw a much better trigger symbol - a clean lightning bolt
                const triggerRadius = size * 0.4;

                // Create a more defined lightning bolt shape
                ctx.beginPath();
                // Start at top
                ctx.moveTo(centerX - triggerRadius * 0.2, centerY - triggerRadius);
                // Top right edge
                ctx.lineTo(centerX + triggerRadius * 0.4, centerY - triggerRadius);
                // Inner notch (top)
                ctx.lineTo(centerX + triggerRadius * 0.1, centerY - triggerRadius * 0.1);
                // Right side to middle
                ctx.lineTo(centerX + triggerRadius * 0.6, centerY - triggerRadius * 0.1);
                // Bottom point
                ctx.lineTo(centerX + triggerRadius * 0.2, centerY + triggerRadius);
                // Bottom left edge
                ctx.lineTo(centerX - triggerRadius * 0.4, centerY + triggerRadius);
                // Inner notch (bottom)
                ctx.lineTo(centerX - triggerRadius * 0.1, centerY + triggerRadius * 0.1);
                // Left side to middle
                ctx.lineTo(centerX - triggerRadius * 0.6, centerY + triggerRadius * 0.1);
                // Close the path back to start
                ctx.closePath();
                ctx.fill();
                break;
        }
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

        // Draw geometric icon instead of emoji
        const iconSize = 20 * zoom;
        const iconX = transformedX + (transformedWidth - iconSize) / 2;
        const iconY = transformedY + 10 * zoom;

        drawGeometricIcon(ctx, type, iconX, iconY, iconSize);

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

    // Calculate canvas size
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
        for (let i = nodes.length - 1; i >= 0; i--) {
            const node = nodes[i];
            if (x >= node.x && x <= node.x + node.width &&
                y >= node.y && y <= node.y + node.height) {
                return node;
            }
        }
        return null;
    };

    // Double-click handler for opening edit modals
    const handleNodeDoubleClick = (node: VisualizationNode) => {
        setEditingNode(node);
        
        // Set form values based on node type
        const nodeData = node.data;
        
        switch (node.type) {
            case NodeType.COMPANY:
                const company = nodeData as Company;
                companyForm.setFieldsValue({
                    name: company.name,
                    parentCompanyId: company.parentCompanyId
                });
                break;
            case NodeType.UNIT:
                const unit = nodeData as Unit;
                unitForm.setFieldsValue({
                    manufacturerCode: unit.manufacturerCode,
                    unitTypeId: unit.unitTypeId?.toString(),
                    companyID: unit.companyId,
                    phoneNumber: unit.phoneNumber,
                    pin: unit.pin,
                    unitCode: unit.unitCode,
                    secret: unit.secret,
                    status: unit.status,
                    daysBeforeNotReported: unit.daysBeforeNotReported,
                    customFieldValues: unit.customFieldValues
                });
                break;
            case NodeType.ALARM:
                const alarm = nodeData as Alarm;
                alarmForm.setFieldsValue({
                    name: alarm.name,
                    companyId: alarm.companyId,
                    recipientSetId: alarm.recipientSetId,
                    isActive: alarm.isActive
                });
                break;
            case NodeType.TRIGGER:
                const trigger = nodeData as Trigger;
                triggerForm.setFieldsValue({
                    alarmId: trigger.alarmId,
                    triggerTypeId: trigger.triggerTypeId,
                    triggerValue: trigger.triggerValue,
                    communicationModeId: trigger.communicationModeId,
                    subject: trigger.subject,
                    body: trigger.body,
                    minimumSendIntervalMinutes: trigger.minimumSendIntervalMinutes,
                    isEnabled: trigger.isEnabled
                });
                break;
        }
        
        setEditModalOpen(true);
    };

    const handleMouseDown = (e: React.MouseEvent<HTMLCanvasElement>) => {
        e.preventDefault();
        
        const mousePos = getMousePos(e);
        const clickedNode = getNodeAtPosition(mousePos.x, mousePos.y);

        if (clickedNode) {
            // Clear any existing timeout
            if (clickTimeout) {
                clearTimeout(clickTimeout);
                setClickTimeout(null);
                // This is a double-click
                handleNodeDoubleClick(clickedNode);
                return;
            }

            // Set timeout for single click
            const timeout = setTimeout(() => {
                // Single click logic
                setSelectedNodeId(clickedNode.id);
                setIsDragging(true);
                setDragOffset({
                    x: mousePos.x - clickedNode.x,
                    y: mousePos.y - clickedNode.y
                });

                setNodes(prevNodes =>
                    prevNodes.map(node => ({
                        ...node,
                        isSelected: node.id === clickedNode.id,
                        isDragging: node.id === clickedNode.id
                    }))
                );
                setClickTimeout(null);
            }, 300); // 300ms timeout for double-click detection

            setClickTimeout(timeout);
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
                    return { ...node, x: newX, y: newY };
                }
                return node;
            })
        );
    };

    const handleMouseUp = () => {
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

    // Modal handlers
    const handleModalCancel = () => {
        setEditModalOpen(false);
        setEditingNode(null);
        companyForm.resetFields();
        unitForm.resetFields();
        alarmForm.resetFields();
        triggerForm.resetFields();
    };

    const handleModalOk = async () => {
        if (!editingNode || !auth.accessToken) return;

        setModalLoading(true);
        try {
            const nodeData = editingNode.data;
            
            switch (editingNode.type) {
                case NodeType.COMPANY:
                    const companyValues = await companyForm.validateFields();
                    await updateCompany(nodeData.id, { 
                        id: nodeData.id, 
                        ...companyValues 
                    }, auth.accessToken);
                    message.success('Company updated successfully');
                    break;
                case NodeType.UNIT:
                    const unitValues = await unitForm.validateFields();
                    await updateUnit(nodeData.id, { 
                        id: nodeData.id, 
                        ...unitValues 
                    }, auth.accessToken);
                    message.success('Unit updated successfully');
                    break;
                case NodeType.ALARM:
                    const alarmValues = await alarmForm.validateFields();
                    await updateAlarm(nodeData.id, { 
                        id: nodeData.id, 
                        ...alarmValues 
                    }, auth.accessToken);
                    message.success('Alarm updated successfully');
                    break;
                case NodeType.TRIGGER:
                    const triggerValues = await triggerForm.validateFields();
                    await updateTrigger(nodeData.id, { 
                        id: nodeData.id, 
                        ...triggerValues 
                    }, auth.accessToken);
                    message.success('Trigger updated successfully');
                    break;
            }
            
            // Reload data to reflect changes
            await loadData();
            handleModalCancel();
        } catch (error: any) {
            if (error.errorFields) return; // Form validation error
            message.error(error.message || 'Failed to update');
        } finally {
            setModalLoading(false);
        }
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
            nodes: nodes.map(node => ({ id: node.id, x: node.x, y: node.y })),
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
    }, [nodes, connections, zoom, pan, canvasSize]);

    useEffect(() => {
        const timers = [50, 100, 200, 500].map(delay =>
            setTimeout(calculateCanvasSize, delay)
        );

        const resizeObserver = new ResizeObserver(() => {
            calculateCanvasSize();
        });

        if (containerRef.current) {
            resizeObserver.observe(containerRef.current);
        }

        return () => {
            timers.forEach(clearTimeout);
            resizeObserver.disconnect();
        };
    }, []);

    // Global mouse handling for smooth dragging
    useEffect(() => {
        const handleGlobalMouseMove = (e: MouseEvent) => {
            if (!isDragging || !selectedNodeId) return;
            
            const mousePos = getMousePos(e);
            const newX = mousePos.x - dragOffset.x;
            const newY = mousePos.y - dragOffset.y;

            setNodes(prevNodes =>
                prevNodes.map(node => {
                    if (node.id === selectedNodeId) {
                        return { ...node, x: newX, y: newY };
                    }
                    return node;
                })
            );
        };

        const handleGlobalMouseUp = () => {
            if (isDragging) {
                setIsDragging(false);
                setNodes(prevNodes =>
                    prevNodes.map(node => ({ ...node, isDragging: false }))
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

    return (
        <div className="visualization-container" ref={containerRef}>
            <Card
                title="Metronview Designer™"
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
                {/* Statistics Panel - now as separate component */}
                <VisualizationStats nodes={nodes} />

                {/* Control Panel - now as separate component */}
                <VisualizationControls
                    zoom={zoom}
                    onZoomIn={handleZoomIn}
                    onZoomOut={handleZoomOut}
                    onResetView={handleResetView}
                    onSaveLayout={handleSaveLayout}
                    onFullscreen={handleFullscreen}
                />

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
                            userSelect: 'none',
                            borderRadius: '4px',
                            boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)'
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
                                <Card 
                                    size="small" 
                                    title={`${selectedNode.type.toUpperCase()}: ${selectedNode.label}`}
                                    extra={
                                        <small style={{ color: '#666' }}>
                                            Double-click to edit
                                        </small>
                                    }
                                >
                                    <pre>{JSON.stringify(selectedNode.data, null, 2)}</pre>
                                </Card>
                            );
                        })()}
                    </div>
                )}
            </Card>

            {/* Edit Modals */}
            {editingNode?.type === NodeType.COMPANY && (
                <CompanyModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={companyForm}
                    companies={companies}
                    onOk={handleModalOk}
                    onCancel={handleModalCancel}
                />
            )}

            {editingNode?.type === NodeType.UNIT && (
                <UnitListModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={unitForm}
                    companies={companies}
                    unitModels={unitModels}
                    onOk={handleModalOk}
                    onCancel={handleModalCancel}
                />
            )}

            {editingNode?.type === NodeType.ALARM && (
                <AlarmModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={alarmForm}
                    companies={companies}
                    onOk={handleModalOk}
                    onCancel={handleModalCancel}
                />
            )}

            {editingNode?.type === NodeType.TRIGGER && (
                <TriggerModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={triggerForm}
                    alarms={alarms}
                    triggerTypes={[]} // You'll need to add trigger types data
                    communicationModes={[]} // You'll need to add communication modes data
                    onOk={handleModalOk}
                    onCancel={handleModalCancel}
                />
            )}
        </div>
    );
};

export default Visualization;