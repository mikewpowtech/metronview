/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import { Table, Button, Modal, Input, Form, Popconfirm, message, Dropdown, Checkbox, Select, Tooltip } from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined, DownOutlined } from "@ant-design/icons";
import {
    fetchCompanies,
    addCompany,
    updateCompany,
    deleteCompany,
    type Company
} from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import "./AntDTable.css"; // For custom compact styles

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Company Name", dataIndex: "name", key: "name", width: 200 },
    { title: "Parent Company", dataIndex: "parentCompanyId", key: "parentCompanyId", width: 200 },
];

//record: Company, index: number, companies?: Company[]

const CompanyList: React.FC = () => {

    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [companies, setCompanies] = useState<Company[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Column visibility state
    const [visibleKeys, setVisibleKeys] = useState<string[]>(
        allColumnDefs.filter(col => col.key !== "id").map(col => col.key as string)
    );
    const [columns, setColumns] = useState(allColumnDefs);

    useEffect(() => {
        loadCompanies();
        // eslint-disable-next-line
    }, [token]);

    useEffect(() => {
        // Replace the parentCompanyId render function to show the company name
        setColumns(
            allColumnDefs
                .filter(col => visibleKeys.includes(col.key as string))
                .map(col =>
                    col.key === "parentCompanyId"
                        ? {
                            ...col,
                            render: (parentCompanyId: number | null) => {
                                if (!parentCompanyId) return "";
                                const parent = companies.find(c => c.id === parentCompanyId);
                                return parent ? parent.name : parentCompanyId;
                            }
                        }
                        : col
                )
        );
    }, [visibleKeys, companies]);

    const loadCompanies = async () => {
        setLoading(true);
        try {
            const data = await fetchCompanies(token);
            setCompanies(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching companies.");
            setCompanies([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        setShowModal(true);
    };

    const handleEdit = (record: Company) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name,
            parentCompanyId: record.parentCompanyId ?? undefined,
        });
        setShowModal(true);
    };

    const handleDelete = async (id: number) => {
        try {
            await deleteCompany(id, token);
            setCompanies(prev => prev.filter(c => c.id !== id));
            message.success("Company deleted");
        } catch (err: any) {
            message.error(err.message || "Failed to delete company");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            values.Id = editingId ?? 0; // Ensure Id is set for updates
            if (isEdit && editingId !== null) {
                await updateCompany(editingId, values, token);
                setCompanies(prev =>
                    prev.map(c =>
                        c.id === editingId ? { ...c, ...values } : c
                    )
                );
                message.success("Company updated");
            } else {
                const added = await addCompany(values.name, values.parentCompanyId ?? null, token);
                setCompanies(prev => [...prev, added]);
                message.success("Company added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save company");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    // Dropdown menu items for columns
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

    // Add the Actions column as the first column, with minimal width for the buttons
    const actionsColumn = {
        title: "",
        key: "actions",
        align: "center" as const,
        className: "actions-col",
        render: (_: any, record: Company) => (
            <span className="actions-col-inner">
                <Tooltip title="edit">
                    <Button
                        icon={<EditOutlined />}
                        size="small"
                        style={{ padding: 0, minWidth: 0, width: 28, height: 28 }}
                        onClick={() => handleEdit(record)}
                    />
                </Tooltip>
                {/*<Popconfirm*/}
                {/*    title="Delete this company?"*/}
                {/*    onConfirm={() => handleDelete(record.id)}*/}
                {/*    okText="Yes"*/}
                {/*    cancelText="No"*/}
                {/*>*/}
                {/*    <Tooltip title="delete">*/}

                {/*    <Button*/}
                {/*        icon={<DeleteOutlined />}*/}
                {/*        size="small"*/}
                {/*        danger*/}
                {/*        />*/}
                {/*    </Tooltip>*/}
                {/*</Popconfirm>*/}

                {/* Add more buttons here if needed */}
            </span>
        ),
    };

    // Place Actions column first
    const tableColumns = [
        actionsColumn,
        ...columns,
    ];

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
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 16 }}>
                <h2 style={{ margin: 0, fontSize: 20 }}>Companies</h2>
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleAdd}
                >
                    Add Company
                </Button>
            </div>
            <Modal
                title={isEdit ? "Edit Company" : "Add Company"}
                open={showModal}
                onCancel={handleModalCancel}
                onOk={handleModalOk}
                okText="Save"
                confirmLoading={modalLoading}
                destroyOnHidden={true}
            >
                <Form
                    layout="vertical"
                    form={form}
                    initialValues={{
                        name: "",
                        parentCompanyId: undefined,
                    }}
                >
                    <Form.Item
                        label="Name"
                        name="name"
                        rules={[{ required: true, message: "Please enter a name" }]}
                    >
                        <Input placeholder="Company Name" />
                    </Form.Item>
                    <Form.Item label="Parent Company" name="parentCompanyId">
                        <Select
                            allowClear
                            showSearch
                            placeholder="Select a parent company"
                            optionFilterProp="children"
                            filterOption={(input, option) =>
                                typeof option?.children === "string" &&
                                (option.children as string).toLowerCase().includes(input.toLowerCase())
                            }
                        >
                            {companies
                                .filter(c => !isEdit || c.id !== editingId) // Prevent selecting self as parent
                                .map(company => (
                                    <Select.Option key={company.id} value={company.id}>
                                        {company.name}
                                    </Select.Option>
                                ))}
                        </Select>
                    </Form.Item>
                </Form>
            </Modal>
            <Table<Company>
                bordered
                dataSource={companies}
                columns={tableColumns}
                rowKey="id"
                pagination={false}
                scroll={{ x: "max-content" }}
                size="middle"
                className="compact-table"
            />
        </div>
    );
};

export default CompanyList;