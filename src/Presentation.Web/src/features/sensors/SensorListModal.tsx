import React, { useState, useEffect } from 'react';
import { Modal, Form, Input, InputNumber, Select, Alert, FormInstance } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import type { Sensor } from './sensorAPI';
import type { Company } from '../companies/companyAPI';
import type { Alarm } from '../alarms/alarmAPI';

interface SensorListModalProps {
    showModal: boolean;
    isEdit: boolean;
    modalLoading: boolean;
    form: FormInstance;
    companies: Company[];
    alarms: Alarm[];
    onOk: () => Promise<void>;
    onCancel: () => void;
    validationErrors?: Record<string, string[]>; // API validation errors
}

export const SensorListModal: React.FC<SensorListModalProps> = ({
    showModal,
    isEdit,
    modalLoading,
    form,
    companies,
    alarms,
    onOk,
    onCancel,
    validationErrors
}) => {
    const [formErrors, setFormErrors] = useState<string[]>([]);
    const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);

    // Watch form values for real-time validation
    const lowValue = Form.useWatch('lowValue', form);
    const highValue = Form.useWatch('highValue', form);
    const channel = Form.useWatch('channel', form);

    // Clear errors when modal closes
    useEffect(() => {
        if (!showModal) {
            setFormErrors([]);
            setHasAttemptedSubmit(false);
        }
    }, [showModal]);

    // Custom validation rules
    const validateChannelUniqueness = async (_: any, value: number) => {
        if (!value && value !== 0) return Promise.resolve();
        
        // In a real scenario, you'd check against existing sensors
        // For now, we'll simulate a validation check
        if (value < 0) {
            return Promise.reject(new Error('Channel must be a positive number'));
        }
        
        if (value > 99) {
            return Promise.reject(new Error('Channel cannot exceed 99'));
        }
        
        return Promise.resolve();
    };

    const validateValueRange = () => {
        const errors: string[] = [];
        
        if (lowValue !== null && lowValue !== undefined && 
            highValue !== null && highValue !== undefined) {
            if (lowValue >= highValue) {
                errors.push('Low value must be less than high value');
            }
        }
        
        setFormErrors(errors);
        return errors.length === 0;
    };

    // Enhanced onOk handler with validation
    const handleOk = async () => {
        try {
            setHasAttemptedSubmit(true);
            
            // Validate form fields
            await form.validateFields();
            
            // Custom validation
            if (!validateValueRange()) {
                return;
            }
            
            // Clear any previous errors
            setFormErrors([]);
            
            // Call parent onOk
            await onOk();
        } catch (error: any) {
            // Handle form validation errors
            if (error.errorFields) {
                const fieldErrors = error.errorFields.map((field: any) => field.errors[0]);
                setFormErrors(fieldErrors);
            }
        }
    };

    // Enhanced validation rules
    const getValidationRules = () => ({
        name: [
            { required: true, message: 'Please enter a sensor name' },
            { min: 2, message: 'Name must be at least 2 characters' },
            { max: 100, message: 'Name cannot exceed 100 characters' },
            { 
                pattern: /^[a-zA-Z0-9\s\-_]+$/, 
                message: 'Name can only contain letters, numbers, spaces, hyphens, and underscores' 
            }
        ],
        channel: [
            { required: true, message: 'Please enter a channel number' },
            { type: 'number', min: 0, message: 'Channel must be 0 or greater' },
            { validator: validateChannelUniqueness }
        ],
        channelType: [
            { type: 'number', min: 0, message: 'Channel type must be 0 or greater' }
        ],
        lowValue: [
            { type: 'number', message: 'Low value must be a number' }
        ],
        highValue: [
            { type: 'number', message: 'High value must be a number' }
        ],
        engineeringUnits: [
            { max: 50, message: 'Engineering units cannot exceed 50 characters' }
        ],
        unitId: [
            { required: true, message: 'Please enter a unit ID' },
            { type: 'number', min: 1, message: 'Unit ID must be greater than 0' }
        ],
        companyId: [
            { required: true, message: 'Please select a company' }
        ],
        alarmId: [
            // Removed required: true to make alarm optional
            // No validation rules needed since it's optional
        ]
    });

    const rules = getValidationRules();

    return (
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
                        {isEdit ? "E" : "S"}
                    </div>
                    {isEdit ? "Edit Sensor" : "Add Sensor"}
                </div>
            }
            open={showModal}
            onCancel={onCancel}
            onOk={handleOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={600}
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
            {/* Display form validation errors */}
            {formErrors.length > 0 && (
                <Alert
                    message="Validation Errors"
                    description={
                        <ul style={{ margin: 0, paddingLeft: 20 }}>
                            {formErrors.map((error, index) => (
                                <li key={index}>{error}</li>
                            ))}
                        </ul>
                    }
                    type="error"
                    icon={<ExclamationCircleOutlined />}
                    style={{ marginBottom: 16 }}
                    closable
                    onClose={() => setFormErrors([])}
                />
            )}

            {/* Display API validation errors */}
            {validationErrors && Object.keys(validationErrors).length > 0 && (
                <Alert
                    message="Server Validation Errors"
                    description={
                        <ul style={{ margin: 0, paddingLeft: 20 }}>
                            {Object.entries(validationErrors).map(([field, errors]) => 
                                errors.map((error, index) => (
                                    <li key={`${field}-${index}`}>
                                        <strong>{field}:</strong> {error}
                                    </li>
                                ))
                            )}
                        </ul>
                    }
                    type="error"
                    icon={<ExclamationCircleOutlined />}
                    style={{ marginBottom: 16 }}
                    closable
                />
            )}

            <Form
                layout="vertical"
                form={form}
                validateTrigger={hasAttemptedSubmit ? 'onChange' : 'onSubmit'}
                initialValues={{
                    name: "",
                    channel: 0,
                    channelType: undefined,
                    lowValue: undefined,
                    highValue: undefined,
                    engineeringUnits: "",
                    unitId: "",
                    companyId: "",
                    alarmId: undefined, // Changed from "" to undefined for optional field
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={rules.name}
                    style={{ marginBottom: '16px' }}
                    hasFeedback
                >
                    <Input 
                        placeholder="Enter sensor name" 
                        maxLength={100}
                        showCount
                    />
                </Form.Item>
                
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item
                        label="Channel"
                        name="channel"
                        rules={rules.channel}
                        style={{ marginBottom: 0 }}
                        hasFeedback
                    >
                        <InputNumber 
                            min={0} 
                            max={99}
                            style={{ width: "100%" }} 
                            placeholder="Channel number" 
                        />
                    </Form.Item>
                    <Form.Item 
                        label="Channel Type" 
                        name="channelType"
                        rules={rules.channelType}
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber 
                            min={0} 
                            style={{ width: "100%" }} 
                            placeholder="Channel type" 
                        />
                    </Form.Item>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item 
                        label="Low Value" 
                        name="lowValue"
                        rules={rules.lowValue}
                        style={{ marginBottom: 0 }}
                        help={lowValue !== null && lowValue !== undefined && 
                              highValue !== null && highValue !== undefined && 
                              lowValue >= highValue ? 
                              "Low value must be less than high value" : ""}
                        validateStatus={lowValue !== null && lowValue !== undefined && 
                                      highValue !== null && highValue !== undefined && 
                                      lowValue >= highValue ? "error" : ""}
                    >
                        <InputNumber 
                            style={{ width: "100%" }} 
                            placeholder="Minimum value" 
                            onChange={validateValueRange}
                        />
                    </Form.Item>
                    <Form.Item 
                        label="High Value" 
                        name="highValue"
                        rules={rules.highValue}
                        style={{ marginBottom: 0 }}
                        help={lowValue !== null && lowValue !== undefined && 
                              highValue !== null && highValue !== undefined && 
                              lowValue >= highValue ? 
                              "High value must be greater than low value" : ""}
                        validateStatus={lowValue !== null && lowValue !== undefined && 
                                      highValue !== null && highValue !== undefined && 
                                      lowValue >= highValue ? "error" : ""}
                    >
                        <InputNumber 
                            style={{ width: "100%" }} 
                            placeholder="Maximum value" 
                            onChange={validateValueRange}
                        />
                    </Form.Item>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item
                        label="Unit ID"
                        name="unitId"
                        rules={rules.unitId}
                        style={{ marginBottom: 0 }}
                        hasFeedback
                    >
                        <InputNumber 
                            min={1} 
                            style={{ width: "100%" }} 
                            placeholder="Unit identifier" 
                        />
                    </Form.Item>
                    
                    <Form.Item 
                        label="Engineering Units" 
                        name="engineeringUnits"
                        rules={rules.engineeringUnits}
                        style={{ marginBottom: 0 }}
                    >
                        <Input 
                            placeholder="e.g., °C, PSI, RPM" 
                            maxLength={50}
                            showCount
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Company"
                    name="companyId"
                    rules={rules.companyId}
                    style={{ marginBottom: '16px' }}
                    hasFeedback
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a company"
                        optionFilterProp="children"
                        notFoundContent="No companies available"
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
                    label={
                        <span>
                            Alarm{' '}
                            <span style={{ 
                                color: '#8c8c8c', 
                                fontWeight: 'normal',
                                fontSize: '12px' 
                            }}>
                                (Optional)
                            </span>
                        </span>
                    }
                    name="alarmId"
                    rules={rules.alarmId}
                    style={{ marginBottom: 0 }}
                    help="Select an alarm to associate with this sensor, or leave blank if no alarm is needed"
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select an alarm (optional)"
                        optionFilterProp="children"
                        notFoundContent="No alarms available"
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
            </Form>
        </Modal>
    );
};