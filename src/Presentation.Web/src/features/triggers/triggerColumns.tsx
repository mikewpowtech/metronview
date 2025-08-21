import type { ColumnsType } from "antd/es/table";
import { Tag, Tooltip } from "antd";
import type { Trigger, TriggerType, CommunicationMode } from "./triggerAPI";
import type { Alarm } from "../alarms/alarmAPI";

// Helper function to convert trigger code to ASCII character
const convertToAsciiChar = (code: string | number): string => {
    if (typeof code === 'string') {
        // If it's already a string, check if it's a single character
        if (code.length === 1) {
            return code; // Already an ASCII character
        }
        // If it's a string representation of a number, convert it
        const numCode = parseInt(code, 10);
        if (!isNaN(numCode)) {
            return String.fromCharCode(numCode);
        }
        return code; // Return as-is if we can't convert
    } else if (typeof code === 'number') {
        // Convert numeric ASCII code to character
        return String.fromCharCode(code);
    }
    return String(code); // Fallback to string conversion
};

export function getColumns(
    triggerTypes: TriggerType[] = [],
    communicationModes: CommunicationMode[] = [],
    alarms: Alarm[] = []
): ColumnsType<Trigger> {
    return [
        {
            title: "ID",
            dataIndex: "id",
            key: "id",
            width: 80,
            sorter: (a, b) => a.id - b.id,
        },
        {
            title: "Alarm",
            dataIndex: "alarmId",
            key: "alarmId",
            width: 200,
            render: (alarmId) => {
                const alarm = alarms.find(a => a.id === alarmId);
                return alarm ? alarm.name : `Alarm ${alarmId}`;
            },
            sorter: (a, b) => a.alarmId - b.alarmId,
        },
        {
            title: "Trigger Type",
            dataIndex: "triggerTypeId",
            key: "triggerTypeId", 
            width: 180,
            render: (triggerTypeId, record) => {
                // First try to find by navigation property if available
                if (record.triggerType) {
                    const asciiCode = convertToAsciiChar(record.triggerType.code);
                    return (
                        <Tag color="blue">
                            {asciiCode} - {record.triggerType.name}
                        </Tag>
                    );
                }
                
                // Fallback to lookup by ID in the triggerTypes array and display code + name
                const triggerType = triggerTypes.find(tt => tt.id === triggerTypeId);
                if (triggerType) {
                    const asciiCode = convertToAsciiChar(triggerType.code);
                    return (
                        <Tag color="blue">
                            {asciiCode} - {triggerType.name}
                        </Tag>
                    );
                }
                
                // If no triggerType found, just show the ID
                return (
                    <Tag color="blue">
                        Type {triggerTypeId}
                    </Tag>
                );
            },
            sorter: (a, b) => {
                // Sort by trigger type code for consistency
                const aType = a.triggerType || triggerTypes.find(tt => tt.id === a.triggerTypeId);
                const bType = b.triggerType || triggerTypes.find(tt => tt.id === b.triggerTypeId);
                const aCode = aType ? convertToAsciiChar(aType.code) : '';
                const bCode = bType ? convertToAsciiChar(bType.code) : '';
                return aCode.localeCompare(bCode);
            },
        },
        {
            title: "Trigger Value",
            dataIndex: "triggerValue",
            key: "triggerValue",
            width: 120,
            sorter: (a, b) => a.triggerValue - b.triggerValue,
        },
        {
            title: "Communication Mode",
            dataIndex: "communicationModeId",
            key: "communicationModeId",
            width: 180,
            render: (communicationModeId, record) => {
                // First try to find by navigation property if available
                if (record.communicationMode) {
                    return (
                        <Tag color="green">
                            {record.communicationMode.code} - {record.communicationMode.name}
                        </Tag>
                    );
                }
                
                // Fallback to lookup by ID in the communicationModes array
                const mode = communicationModes.find(cm => cm.id === communicationModeId);
                if (mode) {
                    return (
                        <Tag color="green">
                            {mode.code} - {mode.name}
                        </Tag>
                    );
                }
                
                // If no mode found, just show the ID
                return (
                    <Tag color="green">
                        Mode {communicationModeId}
                    </Tag>
                );
            },
            sorter: (a, b) => {
                // Sort by communication mode code for consistency
                const aMode = a.communicationMode || communicationModes.find(cm => cm.id === a.communicationModeId);
                const bMode = b.communicationMode || communicationModes.find(cm => cm.id === b.communicationModeId);
                const aCode = aMode ? aMode.code : '';
                const bCode = bMode ? bMode.code : '';
                return aCode.localeCompare(bCode);
            },
        },
        {
            title: "Subject",
            dataIndex: "subject",
            key: "subject",
            width: 200,
            ellipsis: {
                showTitle: false,
            },
            render: (subject) => (
                <Tooltip placement="topLeft" title={subject}>
                    {subject || "-"}
                </Tooltip>
            ),
        },
        {
            title: "Min Interval (min)",
            dataIndex: "minimumSendIntervalMinutes",
            key: "minimumSendIntervalMinutes",
            width: 140,
            sorter: (a, b) => a.minimumSendIntervalMinutes - b.minimumSendIntervalMinutes,
        },
        {
            title: "Status",
            dataIndex: "isEnabled",
            key: "isEnabled",
            width: 100,
            render: (isEnabled) => (
                <Tag color={isEnabled ? "success" : "error"}>
                    {isEnabled ? "Enabled" : "Disabled"}
                </Tag>
            ),
            filters: [
                { text: "Enabled", value: true },
                { text: "Disabled", value: false },
            ],
            onFilter: (value, record) => record.isEnabled === value,
        },
    ];
}