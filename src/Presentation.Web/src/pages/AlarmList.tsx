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
        setShowModal(true);
    };

    const handleEdit = (record: Alarm) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            companyId: record.companyId ?? undefined,
            recipientSetId: record.recipientSetId ?? undefined,
            isActive: record.isActive ?? true,
        });
        setShowModal(true);
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
                isActive: values.isActive
            };
            
            if (isEdit && editingId !== null) {
                // For update, send the alarm data with the ID
                const alarmWithId = { ...alarmData, id: editingId };
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
            title={isEdit ? "Edit Alarm" : "Add Alarm"}
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
                    companyId: undefined,
                    recipientSetId: undefined,
                    isActive: true,
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
                <Form.Item
                    label="Active"
                    name="isActive"
                    valuePropName="checked"
                >
                    <Switch />
                </Form.Item>
            </Form>
        </Modal>
    );

    const getTriggersTable = (alarm: Alarm): React.ReactNode => {
        return (<TriggerList alarmId={alarm.id}></TriggerList>);
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