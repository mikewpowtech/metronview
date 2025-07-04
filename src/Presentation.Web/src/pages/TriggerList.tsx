import React, { useEffect, useState } from "react";
import { Modal, Input, Form, InputNumber, message, Select, Switch, Button } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import {
    fetchTriggers,
    addTrigger,
    updateTrigger,
    deleteTrigger,
    type Trigger,
    fetchTriggersByAlarmId,
    fetchTriggerTypes,
    fetchCommunicationModes,
    type TriggerType,
    type CommunicationMode
} from "../features/triggers/triggerAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getColumns } from "../features/triggers/triggerColumns";

const { TextArea } = Input;

export interface TriggerListProps {
    alarmId?: number;
}

const TriggerList: React.FC<TriggerListProps> = ({ alarmId }) => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [triggers, setTriggers] = useState<Trigger[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Reference data for display and form dropdowns
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [triggerTypes, setTriggerTypes] = useState<TriggerType[]>([]);
    const [communicationModes, setCommunicationModes] = useState<CommunicationMode[]>([]);

    useEffect(() => {
        loadData();
        // eslint-disable-next-line
    }, [token, alarmId]);

    const loadData = async () => {
        console.log('Loading all data...');
        setLoading(true);
        
        // Set a timeout to prevent infinite loading
        const timeoutId = setTimeout(() => {
            console.warn('Loading timeout reached, setting loading to false');
            setLoading(false);
        }, 10000); // 10 second timeout
        
        try {
            // Load both triggers and reference data in parallel
            const [triggersResult, referenceDataResult] = await Promise.allSettled([
                loadTriggers(),
                loadReferenceData()
            ]);
            
            // Clear the timeout since we completed
            clearTimeout(timeoutId);
            
            // Handle triggers result
            if (triggersResult.status === 'rejected') {
                console.error('Triggers loading failed:', triggersResult.reason);
                setError(triggersResult.reason?.message || "An error occurred while fetching triggers.");
                setTriggers([]);
            }
            
            // Handle reference data result
            if (referenceDataResult.status === 'rejected') {
                console.error('Reference data loading failed:', referenceDataResult.reason);
                // Don't set error for reference data failures, just log them
            }
            
        } catch (err: any) {
            console.error('Error loading data:', err);
            setError(err.message || "An error occurred while loading data.");
            clearTimeout(timeoutId);
        } finally {
            console.log('Setting loading to false');
            setLoading(false);
        }
    };

    const loadReferenceData = async () => {
        console.log('Loading reference data...');
        try {
            console.log('Fetching alarms...');
            const alarmsData = await fetchAlarms(token).catch((err) => {
                console.error('Alarms fetch failed:', err);
                return [];
            });
            
            console.log('Fetching trigger types...');
            const triggerTypesData = await fetchTriggerTypes(token).catch((err) => {
                console.error('Trigger types fetch failed:', err);
                return [];
            });
            
            console.log('Fetching communication modes...');
            const communicationModesData = await fetchCommunicationModes(token).catch((err) => {
                console.error('Communication modes fetch failed:', err);
                return [];
            });
            
            console.log('Setting reference data...', { alarmsData, triggerTypesData, communicationModesData });
            setAlarms(alarmsData);
            setTriggerTypes(triggerTypesData);
            setCommunicationModes(communicationModesData);
        } catch (err) {
            console.error("Error loading reference data:", err);
            throw err; // Re-throw to be caught by loadData
        }
    };

    const loadTriggers = async () => {
        console.log('Loading triggers...', { alarmId });
        try {
            let data: Trigger[];
            if (alarmId) {
                console.log('Fetching triggers by alarm ID:', alarmId);
                data = await fetchTriggersByAlarmId(alarmId, token);
            } else {
                console.log('Fetching all triggers');
                data = await fetchTriggers(token);
            }
            console.log('Triggers loaded:', data);
            setTriggers(data);
            setError(null);
        } catch (err: any) {
            console.error('Error loading triggers:', err);
            setError(err.message || "An error occurred while fetching triggers.");
            setTriggers([]);
            throw err; // Re-throw to be caught by loadData
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        // Set default alarm if specified and default enabled state
        const defaultValues: any = {
            isEnabled: false // Default new triggers to disabled so user must explicitly enable
        };
        if (alarmId) {
            defaultValues.alarmId = alarmId;
        }
        form.setFieldsValue(defaultValues);
        setShowModal(true);
    };

    const handleEdit = (record: Trigger) => {
        setIsEdit(true);
        setEditingId(record.id);
        console.log('handleEdit - Editing trigger record:', record);
        console.log('handleEdit - Trigger isEnabled value:', record.isEnabled, typeof record.isEnabled);
        
        // Reset form and immediately set values
        form.resetFields();
        const formValues = {
            alarmId: record.alarmId,
            triggerTypeId: record.triggerTypeId,
            triggerValue: record.triggerValue,
            communicationModeId: record.communicationModeId,
            subject: record.subject ?? "",
            body: record.body ?? "",
            minimumSendIntervalMinutes: record.minimumSendIntervalMinutes,
            isEnabled: Boolean(record.isEnabled), // Ensure it's a proper boolean
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

    const handleDelete = async (record: Trigger) => {
        try {
            if (record.id) {
                await deleteTrigger(record.id, token);
                setTriggers(prev => prev.filter(t => t.id !== record.id));
                message.success("Trigger deleted");
            } else {
                message.error("Trigger ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete trigger");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            
            if (isEdit && editingId !== null) {
                await updateTrigger(editingId, { ...values, id: editingId }, token);
                setTriggers(prev =>
                    prev.map(t =>
                        t.id === editingId ? { ...t, ...values } : t
                    )
                );
                message.success("Trigger updated");
            } else {
                const added = await addTrigger(values, token);
                setTriggers(prev => [...prev, added]);
                message.success("Trigger added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save trigger");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    const getTitle = () => {
        if (alarmId) {
            const alarm = alarms.find(a => a.id === alarmId);
            return `Triggers for: ${alarm ? alarm.name : `Alarm ${alarmId}`}`;
        }
        return "Triggers";
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
                    {isEdit ? "Edit Trigger" : "Add Trigger"}
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
                    alarmId: alarmId || undefined,
                    triggerValue: 0,
                    subject: "",
                    body: "",
                    minimumSendIntervalMinutes: 0,
                    isEnabled: false, // Default to disabled for add modal
                }}
                style={{ marginTop: 16 }}
            >
                <Form.Item
                    label="Alarm"
                    name="alarmId"
                    rules={[{ required: true, message: "Please select an alarm" }]}
                    style={{ marginBottom: 12 }}
                >
                    <Select
                        showSearch
                        placeholder="Select an alarm"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {alarms.map(alarm => (
                            <Select.Option key={alarm.id} value={alarm.id}>
                                {alarm.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item
                        label="Trigger Type"
                        name="triggerTypeId"
                        rules={[{ required: true, message: "Please select a trigger type" }]}
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Select
                            showSearch
                            placeholder="Select trigger type"
                            optionFilterProp="children"
                            filterOption={(input, option) =>
                                typeof option?.children === "string" &&
                                (option.children as string).toLowerCase().includes(input.toLowerCase())
                            }
                        >
                            {triggerTypes.map(triggerType => (
                                <Select.Option key={triggerType.id} value={triggerType.id}>
                                    {triggerType.code} - {triggerType.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Form.Item
                        label="Value"
                        name="triggerValue"
                        rules={[{ required: true, message: "Please enter a trigger value" }]}
                        style={{ marginBottom: 12, width: 100 }}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            placeholder="Value"
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Communication Mode"
                    name="communicationModeId"
                    rules={[{ required: true, message: "Please select a communication mode" }]}
                    style={{ marginBottom: 12 }}
                >
                    <Select
                        showSearch
                        placeholder="Select communication mode"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {communicationModes.map(mode => (
                            <Select.Option key={mode.id} value={mode.id}>
                                {mode.code} - {mode.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item 
                        label="Subject" 
                        name="subject"
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Input placeholder="Email/SMS Subject" />
                    </Form.Item>

                    <Form.Item
                        label="Send Interval (min)"
                        name="minimumSendIntervalMinutes"
                        style={{ marginBottom: 12, width: 150 }}
                    >
                        <InputNumber
                            min={0}
                            style={{ width: "100%" }}
                            placeholder="Minutes"
                        />
                    </Form.Item>
                </div>

                <Form.Item 
                    label="Message Body" 
                    name="body"
                    style={{ marginBottom: 12 }}
                >
                    <TextArea
                        rows={3}
                        placeholder="Message body content"
                    />
                </Form.Item>

                <Form.Item shouldUpdate>
                    {({ getFieldValue }) => {
                        const isEnabled = getFieldValue('isEnabled') ?? false; // Provide default value
                        console.log('Trigger Switch render - isEnabled value:', isEnabled, 'type:', typeof isEnabled);
                        console.log('Trigger Switch render - all form values:', form.getFieldsValue());
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

    const columns = getColumns(triggerTypes, communicationModes, alarms)
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    // If no triggers and this is for a specific alarm, show a compact message
    if (!loading && triggers.length === 0 && alarmId) {
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
                    No triggers configured for this alarm
                </div>
                <Button 
                    type="primary" 
                    size="small" 
                    icon={<PlusOutlined />}
                    onClick={handleAdd}
                >
                    Add Trigger
                </Button>
                <MainModal />
            </div>
        );
    }

    return (
        <div style={{ 
            padding: alarmId ? "8px 0" : "12px 0 12px 30px"
        }}>
            <MainModal />
            <ExtendedAntDTable<Trigger>
                data={triggers}
                tableColumns={columns}
                title={getTitle()}
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                loading={loading}
            />
        </div>
    );
};

export default TriggerList;