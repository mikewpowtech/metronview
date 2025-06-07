import { useEffect, useState } from "react";
import { Table, Button, Dropdown, Checkbox, Modal, Input, Form } from "antd";
import { DownOutlined } from "@ant-design/icons";
import ResizableTitle from "../components/ResizableTitle";
import { fetchUnits, addUnit } from "../features/units/unitsAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

export interface Unit {
    id: string;
    name: string;
    type: string;
    status: string;
}

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Name", dataIndex: "name", key: "name", width: 150 },
    { title: "Type", dataIndex: "type", key: "type", width: 120 },
    { title: "Status", dataIndex: "status", key: "status", width: 120 }
];

const UnitList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [units, setUnits] = useState<Unit[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [newUnit, setNewUnit] = useState<Omit<Unit, "id">>({
        name: "",
        type: "",
        status: ""
    });

    // Table column visibility
    const [visibleKeys, setVisibleKeys] = useState<string[]>(allColumnDefs.map(col => col.key as string));
    const [columns, setColumns] = useState(allColumnDefs);

    useEffect(() => {
        setLoading(true);
        fetchUnits(token)
            .then((data) => {
                setUnits(data);
                setError(null);
            })
            .catch((err) => {
                setError(err.message || "An error occurred while fetching units.");
                setUnits([]);
            })
            .finally(() => setLoading(false));
    }, [token]);

    useEffect(() => {
        setColumns(
            allColumnDefs.filter(col => visibleKeys.includes(col.key as string))
        );
    }, [visibleKeys]);

    const handleResize = (index: number) => (_: any, { size }: any) => {
        const nextColumns = [...columns];
        nextColumns[index] = {
            ...nextColumns[index],
            width: size.width,
        };
        setColumns(nextColumns);
    };

    const resizeableColumns = columns.map((col, index) => ({
        ...col,
        onHeaderCell: (column: any) => ({
            width: column.width,
            onResize: handleResize(index),
        }),
    }));

    const handleAddUnit = async () => {
        try {
            const addedUnit = await addUnit(newUnit, token);
            setUnits((prev) => [...prev, addedUnit]);
            setShowModal(false);
            setNewUnit({ name: "", type: "", status: "" });
        } catch (err: any) {
            setError(err.message || "An error occurred while adding the unit.");
        }
    };

    const columnMenuItems = allColumnDefs.map(col => ({
        key: col.key,
        label: (
            <Checkbox
                checked={visibleKeys.includes(col.key as string)}
                onChange={e => {
                    const checked = e.target.checked;
                    setVisibleKeys(keys =>
                        checked
                            ? [...keys, col.key as string]
                            : keys.filter(k => k !== col.key)
                    );
                }}
                disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
                style={{ width: "100%", padding: "4px 12px" }}
            >
                {col.title}
            </Checkbox>
        ),
    }));

    if (loading) return <div>Loading units...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div>
            <div style={{ marginBottom: 16 }}>
                <Dropdown menu={{ items: columnMenuItems }} trigger={["click"]}>
                    <Button>
                        Columns <DownOutlined />
                    </Button>
                </Dropdown>
            </div>
            <h2>Units List</h2>
            <Button onClick={() => setShowModal(true)} style={{ marginBottom: 16 }}>
                Add Unit
            </Button>
            <Modal
                title="Add Unit"
                open={showModal}
                onCancel={() => setShowModal(false)}
                onOk={handleAddUnit}
                okText="Save"
                destroyOnClose
            >
                <Form
                    layout="vertical"
                    onFinish={handleAddUnit}
                    initialValues={{ name: "", type: "", status: "" }}
                >
                    <Form.Item
                        label="Name"
                        required
                        rules={[{ required: true, message: "Please enter a unit name" }]}
                    >
                        <Input
                            value={newUnit.name}
                            onChange={e => setNewUnit(u => ({ ...u, name: e.target.value }))}
                            placeholder="Unit Name"
                        />
                    </Form.Item>
                    <Form.Item
                        label="Type"
                        required
                        rules={[{ required: true, message: "Please enter a unit type" }]}
                    >
                        <Input
                            value={newUnit.type}
                            onChange={e => setNewUnit(u => ({ ...u, type: e.target.value }))}
                            placeholder="Unit Type"
                        />
                    </Form.Item>
                    <Form.Item
                        label="Status"
                        required
                        rules={[{ required: true, message: "Please enter a unit status" }]}
                    >
                        <Input
                            value={newUnit.status}
                            onChange={e => setNewUnit(u => ({ ...u, status: e.target.value }))}
                            placeholder="Unit Status"
                        />
                    </Form.Item>
                </Form>
            </Modal>
            <Table<Unit>
                bordered
                components={{
                    header: {
                        cell: ResizableTitle,
                    },
                }}
                dataSource={units}
                columns={resizeableColumns}
                rowKey="id"
                pagination={false}
                scroll={{ x: "max-content" }}
            />
        </div>
    );
};

export default UnitList;