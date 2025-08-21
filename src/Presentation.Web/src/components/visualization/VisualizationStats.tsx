import React from 'react';
import { Space, Tooltip, Badge } from 'antd';
import {
    HomeOutlined,
    DatabaseOutlined,
    AlertOutlined,
    ThunderboltOutlined
} from '@ant-design/icons';
import { NodeType, VisualizationNode, Alarm, Trigger } from '../../types/visualization';

interface VisualizationStatsProps {
    nodes: VisualizationNode[];
    className?: string;
}

interface Stats {
    companies: number;
    units: number;
    alarms: number;
    triggers: number;
    activeAlarms: number;
    enabledTriggers: number;
}

export const VisualizationStats: React.FC<VisualizationStatsProps> = ({ 
    nodes, 
    className = "visualization-stats" 
}) => {
    // Calculate statistics
    const stats: Stats = {
        companies: nodes.filter(n => n.type === NodeType.COMPANY).length,
        units: nodes.filter(n => n.type === NodeType.UNIT).length,
        alarms: nodes.filter(n => n.type === NodeType.ALARM).length,
        triggers: nodes.filter(n => n.type === NodeType.TRIGGER).length,
        activeAlarms: nodes.filter(n => n.type === NodeType.ALARM && (n.data as Alarm).isActive).length,
        enabledTriggers: nodes.filter(n => n.type === NodeType.TRIGGER && (n.data as Trigger).isEnabled).length
    };

    return (
        <div className={className}>
            <Space size="large">
                <Badge count={stats.companies} color="#1890ff">
                    <Tooltip title="Companies (Double-click to edit)">
                        <HomeOutlined style={{ fontSize: 16 }} />
                    </Tooltip>
                </Badge>
                <Badge count={stats.units} color="#52c41a">
                    <Tooltip title="Units (Double-click to edit)">
                        <DatabaseOutlined style={{ fontSize: 16 }} />
                    </Tooltip>
                </Badge>
                <Badge count={`${stats.activeAlarms}/${stats.alarms}`} color="#fa541c">
                    <Tooltip title="Active/Total Alarms (Double-click to edit)">
                        <AlertOutlined style={{ fontSize: 16 }} />
                    </Tooltip>
                </Badge>
                <Badge count={`${stats.enabledTriggers}/${stats.triggers}`} color="#722ed1">
                    <Tooltip title="Enabled/Total Triggers (Double-click to edit)">
                        <ThunderboltOutlined style={{ fontSize: 16 }} />
                    </Tooltip>
                </Badge>
            </Space>
        </div>
    );
};

export default VisualizationStats;