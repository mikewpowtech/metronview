import { useEffect, useState } from "react";
import { Table, Button, Dropdown, Checkbox, Modal, Input, Form } from "antd";
import { DownOutlined } from "@ant-design/icons";
import "react-resizable/css/styles.css";
import { fetchCompanies, addCompany } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import ResizableTitle from "../components/ResizableTitle";

export interface Company {
    id: string;
    name: string;
    parentCompanyId?: string | null;
}

const allColumnDefs = [
    { title: "Name", dataIndex: "name", key: "name", width: 200 },
    { title: "Id", dataIndex: "id", key: "id", width: 300 },
    { title: "Parent Company Id", dataIndex: "parentCompanyId", key: "parentCompanyId", width: 300, render: (val: string | null) => val ?? "-" },
];

const CompanyList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth["accessToken"];
    const [companies, setCompanies] = useState<Company[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [newCompanyName, setNewCompanyName] = useState("");
    const [newParentCompanyId, setNewParentCompanyId] = useState<string | null>(null);

    // Table column visibility
    const [visibleKeys, setVisibleKeys] = useState<string[]>(allColumnDefs.map(col => col.key as string));
    const [columns, setColumns] = useState(allColumnDefs);

    useEffect(() => {
        fetchCompanies(token)
            .then((data) => {
                setCompanies(data);
                setError(null);
            })
            .catch((err) => {
                setError(err.message || "An error occurred while fetching companies.");
                setCompanies([]);
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

    const handleAddCompany = async () => {
        try {
            const addedCompany = await addCompany(newCompanyName, newParentCompanyId, token);
            setCompanies((prev) => [...prev, addedCompany]);
            setShowModal(false);
            setNewCompanyName("");
            setNewParentCompanyId(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while adding the company.");
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

    if (loading) return <div>Loading companies...</div>;
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
            <h2>Company List</h2>
            <Button onClick={() => setShowModal(true)} style={{ marginBottom: 16 }}>
                Add Company
            </Button>
            <Modal
                title="Add Company"
                open={showModal}
                onCancel={() => setShowModal(false)}
                onOk={handleAddCompany}
                okText="Save"
                destroyOnClose
            >
                <Form
                    layout="vertical"
                    onFinish={handleAddCompany}
                    initialValues={{ name: "", parentCompanyId: "" }}
                >
                    <Form.Item
                        label="Company Name"
                        required
                        rules={[{ required: true, message: "Please enter a company name" }]}
                    >
                        <Input
                            value={newCompanyName}
                            onChange={e => setNewCompanyName(e.target.value)}
                            placeholder="Company Name"
                        />
                    </Form.Item>
                    <Form.Item label="Parent Company Id (optional)">
                        <Input
                            value={newParentCompanyId ?? ""}
                            onChange={e => setNewParentCompanyId(e.target.value || null)}
                            placeholder="Parent Company Id"
                        />
                    </Form.Item>
                </Form>
            </Modal>
            <Table<Company>
                bordered
                components={{
                    header: {
                        cell: ResizableTitle,
                    },
                }}
                dataSource={companies}
                columns={resizeableColumns}
                rowKey="id"
                pagination={false}
                scroll={{ x: "max-content" }}
            />
        </div>
    );
};

export default CompanyList;