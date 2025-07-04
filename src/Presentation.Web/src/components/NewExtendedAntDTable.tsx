/* eslint-disable @typescript-eslint/no-explicit-any */
import { Table as AntdTable, Checkbox, Dropdown, Button, Tooltip, Popconfirm } from "antd";
import { PlusOutlined, SettingOutlined, EditOutlined, DeleteOutlined } from "@ant-design/icons";
import type { TableProps } from "antd";
import "./AntDTable.css"; // For custom compact styles
import { useState, type ReactNode, useEffect } from "react";
import type { ColumnsType, ColumnType } from "antd/es/table";
import ResizableTitle from "../components/ResizableTitle";
//import { table } from "console";


export type ExtendedTableProps<T> = Omit<TableProps<T>, "dataSource" | "columns" | "title" > & {
    title: string;
    data: T[];
    tableColumns: ColumnsType<T>;
    onEdit?: (record: T) => void;
    customActions?: (record: T) => any;
    onDelete?: (record: T) => void;
    onAdd?: () => void; 
    columnMapper?: (col: any) => any;
};

export function ExtendedAntDTable<T>(props: ExtendedTableProps<T>): ReactNode {

    const { data, tableColumns, rowKey, pagination, title, onAdd, onDelete, onEdit, columnMapper,customActions, ...rest } = props;
    const defaultRowKey = rowKey ?? "id";
    const defaultPagination = pagination ?? false;
    const defaultVisibleKeys = tableColumns.filter(col => col.key !== "id").map(col => col.key as string);
    const [visibleKeys, setVisibleKeys] = useState<string[]>(() => getPersistedVisibleKeys(defaultVisibleKeys));
    const [columns, setColumns] = useState<ColumnsType<T>>(tableColumns);

    useEffect(() => {
        // Map columns and add onHeaderCell for resizing
        setColumns(
            tableColumns
                .filter(col => visibleKeys.includes(col.key as string))
                .map((col, index) => {
                    const mappedCol = columnMapper ? columnMapper(col) : col;
                    return {
                        ...mappedCol,
                        onHeaderCell: (column: any) => ({
                            width: column.width,
                            onResize: handleResize(index),
                        }),
                    };
                })
        );
    }, [visibleKeys, columnMapper, tableColumns]);

    //sets the visible columns in localStorage for the given title
    function setPersistedVisibleKeys(keys: string[]) {
        if (typeof window !== "undefined") {
            localStorage.setItem(title+".visibleColumns", JSON.stringify(keys));
        }
    }

    //returns an arrau of visible columns from localStorage or default keys
    function getPersistedVisibleKeys(defaultKeys: string[]) {

        if (typeof window === "undefined") return defaultKeys;
        const stored = localStorage.getItem(title + ".visibleColumns");
        if (!stored) return defaultKeys;
        try {
            const parsed = JSON.parse(stored);
            if (Array.isArray(parsed)) return parsed;
            return defaultKeys;
        } catch {
            return defaultKeys;
        }
    }

    //gets a list of columns for the table and their visibility based on the local storage item
    function getColumnMenuItems():any[] {
        return tableColumns.map(col => ({
            key: col.key,
            label: (
                <Checkbox
                    checked={visibleKeys.includes(col.key as string)}
                    onChange={e => {
                        const checked = e.target.checked;
                        setVisibleKeys(keys => {
                            const newKeys = checked
                                ? [...keys, col.key as string]
                                : keys.filter(k => k !== col.key);
                            setPersistedVisibleKeys(newKeys);
                            return newKeys;
                        });
                    }}
                    disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
                    style={{ width: "100%", padding: "4px 12px" }}
                >
                    {String(col.title) || col.key}
                </Checkbox>
            ),
        }));
    }

    const tableTitle = () => (
        <>
            <h4>{title}</h4>
            <div>
                {onAdd && (
                    <Button size="small" onClick={onAdd}
                        style={{ margin: "0 10px 0 0" }} >
                        Add <PlusOutlined />
                    </Button>)
                }
                <Dropdown menu={{ items: getColumnMenuItems() }} trigger={["click"]}>
                    <Button size="small">
                        Columns <SettingOutlined />
                    </Button>
                </Dropdown>
            </div>
        </>
    );

    const actionsColumn: ColumnType<T> = {
        title: "",
        key: "actions",
        align: "center" as const,
        className: "actions-col",
        render: (_: any, record: T) => (
            <span className="actions-col-inner">
                {onEdit && (
                    <Tooltip title="edit">
                        <Button
                            icon={<EditOutlined />}
                            size="small"
                            style={{ padding: 0, minWidth: 0, width: 28, height: 28 }}
                            onClick={() => onEdit(record)}
                        />
                    </Tooltip>)
                }
                {onDelete && (
                    <Popconfirm
                        title="Delete this item?"
                        onConfirm={() => onDelete(record)}
                        okText="Yes"
                        cancelText="No"
                    >
                        <Tooltip title="delete">

                            <Button
                                icon={<DeleteOutlined />}
                                size="small"
                                danger
                            />
                        </Tooltip>
                    </Popconfirm>)
                }
                {/* Render custom actions if defined */}
                {customActions && customActions(record)}
            </span>
        ),
    };

    const handleResize = (index: number) => (_: any, { size }: any) => {
        setColumns(prevColumns => {
            const nextColumns = [...prevColumns];
            nextColumns[index] = {
                ...nextColumns[index],
                width: size.width,
            };
            return nextColumns;
        });
    };


    // Place Actions column first
    const finalizedColumns: ColumnsType<T> = [
        actionsColumn,
        ...columns,
    ];



    return (
        <AntdTable<T>
            dataSource={data}
            columns={finalizedColumns}
            bordered
            rowKey={defaultRowKey}
            pagination={defaultPagination}
            scroll={{ x: "max-content" }}
            size="middle"
            className="compact-table"
            title={tableTitle}
            components={{
                header: {
                    cell: ResizableTitle,
                },
            }}
            {...rest}
        />
    );
}