import type { ColumnsType } from "antd/es/table";
import type { Sensor } from "./sensorAPI";

const columnDefinitions: ColumnsType<Sensor>=[
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Name", dataIndex: "name", key: "name", width: 150 },
    { title: "Channel", dataIndex: "channel", key: "channel", width: 50 },
    { title: "Channel Type", dataIndex: "channelType", key: "channelType", width: 75 },
    { title: "Low Value", dataIndex: "lowValue", key: "lowValue", width: 75 },
    { title: "High Value", dataIndex: "highValue", key: "highValue", width: 75 },
    { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits", width: 80 },
    { title: "Unit ID", dataIndex: "unitId", key: "unitId", width: 120 },
    { title: "Company ID", dataIndex: "companyID", key: "companyID", width: 120 },
    { title: "Alarm ID", dataIndex: "alarmId", key: "alarmId", width: 120 },
];

//add custom rendering if required
const columnMapper = (col: any) => {
    return col;
};

export const getSensorColumns = (): ColumnsType<Sensor> => {
    return columnDefinitions.map(columnMapper);
};
