import { Table } from "antd";
import type { TableProps } from "antd";
import "./AntDTable.css"; // For custom compact styles
import type { ExpandableConfig } from "antd/es/table/interface";

type GenericTableProps<T> = Omit<TableProps<T>, "dataSource" | "columns"> & {
    data: T[];
    columns: any;
    title?: React.ReactNode | (() => React.ReactNode);
};

export function GenericTable<T extends object>(props: GenericTableProps<T>) {
    const { data, columns, rowKey, pagination, title, ...rest } = props;
    const defaultRowKey = rowKey ?? "id";
    const defaultPagination = pagination ?? false;

    return (
        <Table<T>
            dataSource={data}
            columns={columns}
            bordered
            rowKey={defaultRowKey}
            pagination={defaultPagination}
            scroll={{ x: "max-content" }}
            size="middle"
            className="compact-table"
            title={title}
            { ...rest}
        />
    );
}