import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Switch, Select, Button } from "antd";
import {
    fetchRecipients,
    fetchRecipientsByRecipientSet, // New function for filtering
    addRecipient,
    updateRecipient,
    deleteRecipient,
    type Recipient,
    RecipientNotFoundError
} from "../features/recipients/recipientAPI";
import { addRecipientToSet } from "../features/recipientSets/recipientSetAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { PlusOutlined } from "@ant-design/icons";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Name", dataIndex: "name", key: "name", width: 180 },
    { title: "Email", dataIndex: "email", key: "email", width: 180 },
    { title: "SMS", dataIndex: "sms", key: "sms", width: 120 },
    { title: "Web Service Root", dataIndex: "webServiceRoot", key: "webServiceRoot", width: 180 },
    { title: "Company", dataIndex: "companyId", key: "companyId", width: 120 },
    { title: "Enabled", dataIndex: "isEnabled", key: "isEnabled", width: 80 },
];

interface RecipientListProps {
    recipientSetId?: number; // Optional recipientSetId prop
}

const RecipientList: React.FC<RecipientListProps> = ({ recipientSetId }) => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [recipients, setRecipients] = useState<Recipient[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Companies for company selection
    const [companies, setCompanies] = useState<Company[]>([]);

    useEffect(() => {
        loadRecipients();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        // eslint-disable-next-line
    }, [token, recipientSetId]); // Add recipientSetId to dependencies

    const loadRecipients = async () => {
        setLoading(true);
        try {
            let data: Recipient[];
            
            if (recipientSetId) {
                try {
                    // Fetch recipients for specific recipient set
                    data = await fetchRecipientsByRecipientSet(recipientSetId, token);
                    // Handle successful response
                } catch (error) {
                    if (error instanceof RecipientNotFoundError) {
                        // Handle 404 specifically
                        data = [];
                    } else {
                        // Handle other errors
                        console.error("An unexpected error occurred:", error);
                        throw error;
                    }
                }
            } else {
                // Fetch all recipients
                data = await fetchRecipients(token);
            }
            
            setRecipients(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching recipients.");
            setRecipients([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        // Set default values
        const defaultValues: any = {
            isEnabled: true
        };
        form.setFieldsValue(defaultValues);
        setShowModal(true);
    };

    const handleEdit = (record: Recipient) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            email: record.email ?? "",
            sms: record.sms ?? "",
            webServiceRoot: record.webServiceRoot ?? "",
            companyId: record.companyId ?? "",
            isEnabled: record.isEnabled ?? true,
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Recipient) => {
        try {
            if (record.id) {
                await deleteRecipient(record.id, token);
                setRecipients(prev => prev.filter(r => r.id !== record.id));
                message.success("Recipient deleted");
            } else {
                message.error("Recipient ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete recipient");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                await updateRecipient(editingId, { ...values, id: editingId }, token);
                setRecipients(prev =>
                    prev.map(r =>
                        r.id === editingId ? { ...r, ...values } : r
                    )
                );
                message.success("Recipient updated");
            } else {
                const added = await addRecipient(values, token);
                
                // If we're in a recipient set context, also add the recipient to the set
                if (recipientSetId && added.id) {
                    try {
                        await addRecipientToSet(recipientSetId, added.id, token);
                        message.success("Recipient added to recipient set");
                    } catch (err: any) {
                        console.error("Failed to add recipient to set:", err);
                        message.warning("Recipient created but failed to add to recipient set");
                    }
                }
                
                setRecipients(prev => [...prev, added]);
                message.success("Recipient added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save recipient");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    const MainModal: React.FC = () => (
        <Modal
            title={
                <div style={{
                    fontSize: '16px',
                    fontWeight: 600,
                    color: '#ffffff',
                    padding: '4px 0',
                    marginBottom: '8px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                }}>
                    <PlusOutlined style={{ color: '#ffffff' }} />
                    {isEdit ? "Edit Recipient" : "Add Recipient"}
                </div>
            }
            open={showModal}
            onCancel={handleModalCancel}
            onOk={handleModalOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={530}
            styles={{
                header: {
                    background: '#1890ff',
                    borderRadius: '8px 8px 0 0',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderBottom: 'none',
                    marginBottom: '0'
                },
                body: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderBottom: 'none',
                    marginTop: '0'
                },
                footer: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderRadius: '0 0 8px 8px',
                    marginTop: '0'
                },
                content: {
                    padding: '0',
                    overflow: 'hidden',
                    borderRadius: '8px',
                    border: 'none'
                }
            }}
            closeIcon={
                <span style={{ 
                    color: 'white', 
                    fontWeight: 'bold',
                    fontSize: '16px'
                }}>×</span>
            }
        >
            <Form
                layout="vertical"
                form={form}
                size="small"
                initialValues={{
                    name: "",
                    email: "",
                    sms: "",
                    webServiceRoot: "",
                    companyId: "",
                    isEnabled: true,
                }}
                style={{ marginTop: 16 }}
            >
                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item
                        label="Name"
                        name="name"
                        rules={[{ required: true, message: "Please enter a name" }]}
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Input placeholder="Recipient Name" />
                    </Form.Item>

                    <Form.Item
                        label="Company"
                        name="companyId"
                        rules={[{ required: true, message: "Please select a company" }]}
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Select
                            showSearch
                            allowClear
                            placeholder="Select a company"
                            optionFilterProp="children"
                            filterOption={(input, option) =>
                                typeof option?.children === "string" &&
                                (option.children as string).toLowerCase().includes(input.toLowerCase())
                            }
                        >
                            {companies.map(company => (
                                <Select.Option key={company.id} value={company.id}>
                                    {company.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>
                </div>

                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item
                        label="Email"
                        name="email"
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Input placeholder="Email Address" />
                    </Form.Item>

                    <Form.Item
                        label="SMS"
                        name="sms"
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Input placeholder="SMS Number" />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Web Service Root"
                    name="webServiceRoot"
                    style={{ marginBottom: 12 }}
                >
                    <Input placeholder="Web Service Root URL" />
                </Form.Item>

                <Form.Item shouldUpdate>
                    {({ getFieldValue }) => {
                        const isEnabled = getFieldValue('isEnabled') ?? true;
                        return (
                            <Form.Item 
                                label="Status"
                                name="isEnabled" 
                                valuePropName="checked"
                                style={{ marginBottom: 0 }}
                            >
                                <Switch 
                                    size="default"
                                    checkedChildren="Enabled" 
                                    unCheckedChildren="Disabled"
                                    style={{
                                        backgroundColor: isEnabled ? '#52c41a' : '#ff4d4f',
                                        transform: 'scale(1.2)',
                                        transformOrigin: 'left center',
                                        minWidth: '80px'
                                    }}
                                />
                            </Form.Item>
                        );
                    }}
                </Form.Item>
            </Form>
        </Modal>
    );

    // Column mapping for company name and enabled switch
    const columnMapper = (col: any) => {
        if (col.key === "companyId") {
            return {
                ...col,
                render: (companyId: number) => {
                    const company = companies.find(c => c.id === companyId);
                    return company ? company.name : companyId;
                }
            };
        }
        if (col.key === "isEnabled") {
            return {
                ...col,
                render: (isEnabled: boolean) => (
                    <span>{isEnabled ? "Yes" : "No"}</span>
                )
            };
        }
        return col;
    };

    if (loading) return <div>Loading recipients...</div>;
    if (error) return <div style={{ color: "red" }}>{error}</div>;

    // Determine the title based on whether filtering by recipient set
    const tableTitle = recipientSetId 
        ? `Recipients for Recipient Set ${recipientSetId}` 
        : "Recipients";

    // If no recipients and this is for a specific recipient set, show a compact message
    if (!loading && recipients.length === 0 && recipientSetId) {
        return (
            <div style={{
                padding: "16px",
                textAlign: "center",
                backgroundColor: "#fafafa",
                border: "1px solid #f0f0f0",
                borderRadius: "4px",
                margin: "8px 0"
            }}>
                <div style={{
                    color: "#666",
                    fontStyle: "italic",
                    fontSize: "14px",
                    marginBottom: "12px"
                }}>
                    No recipients configured for this recipient set
                </div>
                <Button
                    type="primary"
                    size="small"
                    icon={<PlusOutlined />}
                    onClick={handleAdd}
                >
                    Add Recipient
                </Button>
                <MainModal />
            </div>
        );
    }

    return (
        <div style={{ 
            padding: recipientSetId ? "8px" : "12px 0 12px 30px" // Less padding when embedded
        }}>
            <MainModal />
            <ExtendedAntDTable<Recipient>
                data={recipients}
                tableColumns={allColumnDefs}
                title={ tableTitle }
                onAdd={ handleAdd } // Disable add when filtering
                onEdit={ handleEdit } // Disable edit when filtering
                onDelete={ handleDelete } // Disable delete when filtering
                columnMapper={columnMapper}
                size={recipientSetId ? "small" : undefined} // Smaller table when embedded
            />
        </div>
    );
};

export default RecipientList;