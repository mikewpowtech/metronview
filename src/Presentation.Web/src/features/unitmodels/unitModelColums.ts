import type { ColumnsType } from "antd/es/table";
import type { UnitModel } from "./unitModelAPI";

const columnDefinitions: ColumnsType<UnitModel>=[
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Code", dataIndex: "code", key: "code", width: 120 },
    { title: "Name", dataIndex: "name", key: "name", width: 150 },
    { title: "Description", dataIndex: "description", key: "description", width: 200 },
];

//add custom rendering if required
const columnMapper = (col: any) => {
    return col;
};

export const getColumns = (): ColumnsType<UnitModel> => {
    return columnDefinitions.map(columnMapper);
};
