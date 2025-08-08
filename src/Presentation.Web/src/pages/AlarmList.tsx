import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Switch, Select, Tooltip, Button } from "antd";
import { DownOutlined, RightOutlined } from "@ant-design/icons";
import {
    fetchAlarms,
    addAlarm,
    updateAlarm,
    deleteAlarm,
    type Alarm
} from "../features/alarms/alarmAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchRecipientSets, type RecipientSet } from "../features/recipientSets/recipientSetAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getColumns } from "../features/alarms/alarmColumns";
import TriggerList from "./TriggerList";

const AlarmList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);
    const [expandedRowKeys, setExpandedRowKeys] = useState<React.Key[]>([]);

    // Companies and recipient sets for selection
    const [companies, setCompanies] = useState<Company[]>([]);
    const [recipientSets, setRecipientSets] = useState<RecipientSet[]>([]);

    useEffect(() => {
        loadAlarms();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        fetchRecipientSets(token)
            .then(setRecipientSets)
            .catch(() => setRecipientSets([]));
        // eslint-disable-next-line
    }, [token]);

    const loadAlarms = async () => {
        setLoading(true);
        try {
            const data = await fetchAlarms(token);
            setAlarms(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching alarms.");
            setAlarms([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        // Set initial value for new alarm
        form.setFieldsValue({
            isActive: false // Default new alarms to inactive so user must explicitly activate
        });
        setShowModal(true);
    };

    const handleEdit = (record: Alarm) => {
        setIsEdit(true);
        setEditingId(record.id);
        console.log('handleEdit - Editing alarm record:', record);
        console.log('handleEdit - Alarm isActive value:', record.isActive, typeof record.isActive);
        
        // Reset form and immediately set values
        form.resetFields();
        const formValues = {
            name: record.name ?? "",
            companyId: record.companyId ?? undefined,
            recipientSetId: record.recipientSetId ?? undefined,
            isActive: Boolean(record.isActive), // Ensure it's a proper boolean
        };
        console.log('handleEdit - Setting form values:', formValues);
        form.setFieldsValue(formValues);
        
        setShowModal(true);
        
        // Check form values after modal opens
        setTimeout(() => {
            const currentValues = form.getFieldsValue();
            console.log('handleEdit - Form values after modal opens:', currentValues);
        }, 100);
    };

    const handleDelete = async (record: Alarm) => {
        try {
            if (record.id) {
                await deleteAlarm(record.id, token);
                setAlarms(prev => prev.filter(a => a.id !== record.id));
                message.success("Alarm deleted");
            } else {
                message.error("Alarm ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete alarm");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            console.log('Form values when submitting:', values);
            
            // Ensure data types are correct and validate
            const companyId = parseInt(values.companyId);
            const recipientSetId = parseInt(values.recipientSetId);
            
            if (isNaN(companyId) || isNaN(recipientSetId)) {
                message.error('Please select valid Company and Recipient Set');
                return;
            }
            
            const alarmData = {
                name: values.name,
                companyId: companyId,
                recipientSetId: recipientSetId,
                isActive: Boolean(values.isActive) // Ensure we always have a boolean value
            };
            console.log('Alarm data being saved:', alarmData);
            
            if (isEdit && editingId !== null) {
                // For update, send the alarm data with the ID
                const alarmWithId = { ...alarmData, id: editingId };
                console.log('Updating alarm with:', alarmWithId);
                await updateAlarm(editingId, alarmWithId, token);
                setAlarms(prev =>
                    prev.map(a =>
                        a.id === editingId ? { ...a, ...alarmData } : a
                    )
                );
                message.success("Alarm updated");
            } else {
                const added = await addAlarm(alarmData, token);
                setAlarms(prev => [...prev, added]);
                message.success("Alarm added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            console.error('API Error:', err.response?.data || err.message);
            message.error(err.response?.data?.message || err.response?.data || err.message || "Failed to save alarm");
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
                    <div style={{
                        width: '24px',
                        height: '24px',
                        borderRadius: '50%',
                        background: 'linear-gradient(135deg, #1890ff 0%, #40a9ff 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'white',
                        fontSize: '12px',
                        fontWeight: 'bold'
                    }}>
                        {isEdit ? "E" : "A"}
                    </div>
                    {isEdit ? "Edit Alarm" : "Add Alarm"}
                </div>
            }
            open={showModal}
            onCancel={handleModalCancel}
            onOk={handleModalOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
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
                initialValues={{
                    name: "",
                    companyId: undefined,
                    recipientSetId: undefined,
                    isActive: false, // Default to inactive for add modal
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Alarm Name" />
                </Form.Item>
                <Form.Item
                    label="Company"
                    name="companyId"
                    rules={[{ required: true, message: "Please select a company" }]}
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
                <Form.Item
                    label="Recipient Set"
                    name="recipientSetId"
                    rules={[{ required: true, message: "Please select a recipient set" }]}
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a recipient set"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {recipientSets.map(rs => (
                            <Select.Option key={rs.id} value={rs.id}>
                                {rs.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item shouldUpdate>
                    {({ getFieldValue }) => {
                        const isActive = getFieldValue('isActive') ?? false; // Provide default value
                        console.log('Switch render - isActive value:', isActive, 'type:', typeof isActive);
                        console.log('Switch render - all form values:', form.getFieldsValue());
                        return (
                            <Form.Item
                                label="Status"
                                name="isActive"
                                valuePropName="checked"
                                style={{ marginBottom: 0 }}
                            >
                                <Switch 
                                    size="default"
                                    checkedChildren="Active" 
                                    unCheckedChildren="Inactive"
                                    style={{
                                        backgroundColor: isActive ? '#52c41a' : '#ff4d4f',
                                        transform: 'scale(1.2)', // Make it 20% bigger
                                        transformOrigin: 'left center', // Scale from left edge to maintain alignment
                                        minWidth: '80px' // Ensure minimum width for text
                                    }}
                                />
                            </Form.Item>
                        );
                    }}
                </Form.Item>
            </Form>
        </Modal>
    );

    const getTriggersTable = (alarm: Alarm): React.ReactNode => {
        return (<TriggerList alarmId={alarm.id}/>);
    };
    const handleExpandRow = (record: Alarm) => {
        setExpandedRowKeys(keys =>
            keys.includes(record.id)
                ? keys.filter(key => key !== record.id)
                : [...keys, record.id]
        );
    };

    const customActions = (record: Alarm) => (
        <Tooltip title={expandedRowKeys.includes(record.id) ? "Hide Triggers" : "Triggers"}>
            <Button
                icon={expandedRowKeys.includes(record.id) ? <DownOutlined /> : <RightOutlined />}
                size="small"
                style={{ marginLeft: 4, padding: 0, minWidth: 0, width: 28, height: 28 }}
                onClick={() => handleExpandRow(record)}
            />
        </Tooltip>
    );

    if (error) return <div style={{ color: "red" }}>{error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Alarm>
                data={alarms}
                tableColumns={getColumns(companies, recipientSets)}
                title="Alarms"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                customActions={customActions}
                expandable={{
                    expandedRowRender: (alarm: Alarm) => {
                        return getTriggersTable(alarm);
                    },
                    expandedRowKeys,
                    onExpand: (_, record) => handleExpandRow(record),
                    showExpandColumn: false
                }}
                loading={loading}
            />
        </div>
    );
};

export default AlarmList;