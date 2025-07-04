import type { ColumnsType } from "antd/es/table";
import { Tag, Tooltip } from "antd";
import type { Trigger, TriggerType, CommunicationMode } from "./triggerAPI";
import type { Alarm } from "../alarms/alarmAPI";

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
            width: 150,
            render: (triggerTypeId) => {
                const triggerType = triggerTypes.find(tt => tt.id === triggerTypeId);
                return (
                    <Tag color="blue">
                        {triggerType ? triggerType.code : `Type ${triggerTypeId}`}
                    </Tag>
                );
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
            render: (communicationModeId) => {
                const mode = communicationModes.find(cm => cm.id === communicationModeId);
                return (
                    <Tag color="green">
                        {mode ? mode.code : `Mode ${communicationModeId}`}
                    </Tag>
                );
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