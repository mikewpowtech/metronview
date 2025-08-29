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
import { drawGeometricIcon } from '../components/visualization/icons';
import { NodeType, VisualizationNode, NodeConnection } from '../types/visualization';
import { LayoutEngine, LayoutType, ForceDirectedConfig, DEFAULT_FORCE_CONFIG } from '../components/visualization/layout';
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
    const [layoutType, setLayoutType] = useState<LayoutType>(LayoutType.GRID);
    const [layoutEngine, setLayoutEngine] = useState<LayoutEngine | null>(null);
    const [isAnimating, setIsAnimating] = useState(false);
    const [animationProgress, setAnimationProgress] = useState(0);

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
        const engine = new LayoutEngine();
        setLayoutEngine(engine);
        
        const forceConfig: ForceDirectedConfig = {
            ...DEFAULT_FORCE_CONFIG,
            width: canvasSize.width,
            height: canvasSize.height,
            iterations: 200 // Reduce for better performance
        };
        
        const { nodes: newNodes, connections: newConnections } = engine.generateLayout({
            companiesData,
            unitsData,
            alarmsData,
            triggersData,
            selectedCompany,
            nodeColors,
            layoutType,
            forceConfig,
            canvasSize
        });

        setNodes(newNodes);
        setConnections(newConnections);

        // Start animation for force-directed layout
        if (layoutType === LayoutType.FORCE_DIRECTED) {
            setIsAnimating(true);
            setAnimationProgress(0);
        }
    };

    // Canvas drawing functions - updated to use the new modular icon system
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

        // Draw geometric icon using the new modular system
        const iconSize = 20 * zoom;
        const iconX = transformedX + (transformedWidth - iconSize) / 2;
        const iconY = transformedY + 10 * zoom;

        drawGeometricIcon(ctx, type, iconX, iconY, iconSize, nodeColors);

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

    // Animation effect for force-directed layout
    useEffect(() => {
        if (!isAnimating || !layoutEngine || layoutType !== LayoutType.FORCE_DIRECTED) return;

        const animationFrame = () => {
            if (layoutEngine.isForceSimulationComplete()) {
                setIsAnimating(false);
                setAnimationProgress(1);
                return;
            }

            layoutEngine.simulateForceStep();
            const progress = layoutEngine.getForceSimulationProgress();
            setAnimationProgress(progress);

            // Continue animation
            requestAnimationFrame(animationFrame);
        };

        const animationId = requestAnimationFrame(animationFrame);
        return () => cancelAnimationFrame(animationId);
    }, [isAnimating, layoutEngine, layoutType]);

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
                            style={{ width: 150 }}
                            placeholder="Layout"
                            value={layoutType}
                            onChange={(value: LayoutType) => {
                                setLayoutType(value);
                                // Regenerate layout with new type
                                generateVisualizationNodes(companies, units, alarms, triggers);
                            }}
                        >
                            <Select.Option value={LayoutType.GRID}>Grid</Select.Option>
                            <Select.Option value={LayoutType.HIERARCHICAL}>Hierarchical</Select.Option>
                            <Select.Option value={LayoutType.CLUSTERED}>Clustered</Select.Option>
                            <Select.Option value={LayoutType.FORCE_DIRECTED}>Force-Directed</Select.Option>
                        </Select>
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
                        {isAnimating && (
                            <span style={{ color: '#1890ff' }}>
                                Animating... {Math.round(animationProgress * 100)}%
                            </span>
                        )}
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