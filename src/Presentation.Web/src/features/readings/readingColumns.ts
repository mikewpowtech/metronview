import type { ColumnsType } from "antd/es/table";
import dayjs from "dayjs";
import type { Reading } from "./readingAPI";
import type { Sensor } from "../sensors/sensorAPI";

const columnDefinitions: ColumnsType<Reading> = [
    {
        title: "Date Received",
        dataIndex: "dateReceivedUtc",
        key: "dateReceivedUtc",
        width: 180,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : ""
    },
    {
        title: "Date Recorded",
        dataIndex: "dateRecordedUtc",
        key: "dateRecordedUtc",
        width: 180,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : ""
    },
    {
        title: "Sensor ID",
        dataIndex: "sensor",
        key: "sensorId",
        width: 100,
        render: (sensor: Sensor) => sensor?.id ?? ""
    },
    {
        title: "Sensor Name",
        dataIndex: "sensor",
        key: "sensorName",
        width: 200,
        render: (sensor: Sensor) => {
            if (sensor?.name) {
                return sensor.name;
            } else if (sensor?.id) {
                return `Sensor ${sensor.id}`;
            }
            return "Unknown Sensor";
        }
    },
    {
        title: "Value",
        dataIndex: "value",
        key: "value",
        width: 120
    },
];

// Add custom rendering if required
const columnMapper = (col: any) => {
    return col;
};

export const getReadingColumns = (): ColumnsType<Reading> => {
    return columnDefinitions.map(columnMapper);
};

// Get reading columns without sensor columns (for when filtering by specific sensor)
export const getReadingColumnsWithoutSensor = (): ColumnsType<Reading> => {
    return columnDefinitions
        .filter(col => col.key !== "sensorId" && col.key !== "sensorName")
        .map(columnMapper);
};
