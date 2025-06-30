import { Tooltip, Button, Popconfirm } from "antd";
import { EditOutlined, DeleteOutlined } from "@ant-design/icons";

export type TableColumnsProps<T extends { id: number }> = {
    columns: any[];
    onEdit?: (entity: T) => void;
    onDelete?: (entityId: number) => void;
};

export function TableColumns<T extends { id: number }>({
    columns,
    onEdit,
    onDelete,
}: TableColumnsProps<T>) {
    const actionsColumn = {
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
                    </Tooltip>
                )}
                {onDelete && (
                    <Popconfirm
                        title="Delete this item?"
                        onConfirm={() => onDelete(record.id)}
                        okText="Yes"
                        cancelText="No"
                    >
                        <Button
                            icon={<DeleteOutlined />}
                            size="small"
                            danger
                        />
                    </Popconfirm>
                )}
            </span>
        ),
    };

    return [actionsColumn, ...columns];
}