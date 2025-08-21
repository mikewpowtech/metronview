import React from 'react';
import { Button, Space, Tooltip } from 'antd';
import {
    ZoomInOutlined,
    ZoomOutOutlined,
    ReloadOutlined,
    SaveOutlined,
    FullscreenOutlined
} from '@ant-design/icons';

interface VisualizationControlsProps {
    zoom: number;
    onZoomIn: () => void;
    onZoomOut: () => void;
    onResetView: () => void;
    onSaveLayout: () => void;
    onFullscreen: () => void;
    className?: string;
}

export const VisualizationControls: React.FC<VisualizationControlsProps> = ({
    zoom,
    onZoomIn,
    onZoomOut,
    onResetView,
    onSaveLayout,
    onFullscreen,
    className = "visualization-controls"
}) => {
    return (
        <div className={className}>
            <Space>
                <Tooltip title="Zoom In">
                    <Button icon={<ZoomInOutlined />} onClick={onZoomIn} />
                </Tooltip>
                <Tooltip title="Zoom Out">
                    <Button icon={<ZoomOutOutlined />} onClick={onZoomOut} />
                </Tooltip>
                <Tooltip title="Reset View">
                    <Button icon={<ReloadOutlined />} onClick={onResetView} />
                </Tooltip>
                <Tooltip title="Save Layout">
                    <Button icon={<SaveOutlined />} onClick={onSaveLayout} />
                </Tooltip>
                <Tooltip title="Fullscreen">
                    <Button icon={<FullscreenOutlined />} onClick={onFullscreen} />
                </Tooltip>
                <span>Zoom: {Math.round(zoom * 100)}% | Double-click nodes to edit</span>
            </Space>
        </div>
    );
};

export default VisualizationControls;