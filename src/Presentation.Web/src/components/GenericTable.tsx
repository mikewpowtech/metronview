import { Table } from "antd";
import type { TableProps } from "antd";
import "./AntDTable.css"; // For custom compact styles

type GenericTableProps<T> = Omit<TableProps<T>, "dataSource" | "columns"> & {
    data: T[];
    columns: any;
};

export function GenericTable<T extends object>(props: GenericTableProps<T>) {
    const { data, columns, rowKey, pagination, ...rest } = props;
    const resolvedRowKey = rowKey ?? "id";
    const resolvedPagination = pagination ?? false;
    return (
        <Table<T>
            dataSource={data}
            columns={columns}
            bordered
            rowKey={resolvedRowKey}
            pagination={resolvedPagination}
            scroll={{ x: "max-content" }}
            size="middle"
            className="compact-table"
            {...rest}
        />
    );
}