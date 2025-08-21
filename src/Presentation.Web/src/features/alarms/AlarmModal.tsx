import React, { useState, useEffect } from 'react';
import { Modal, Form, Input, Select, Switch, Alert, FormInstance } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import type { Alarm } from './alarmAPI';
import type { Company } from '../companies/companyAPI';

interface AlarmModalProps {
    showModal: boolean;
    isEdit: boolean;
    modalLoading: boolean;
    form: FormInstance;
    companies: Company[];
    onOk: () => Promise<void>;
    onCancel: () => void;
    validationErrors?: Record<string, string[]>;
}

export const AlarmModal: React.FC<AlarmModalProps> = ({
    showModal,
    isEdit,
    modalLoading,
    form,
    companies,
    onOk,
    onCancel,
    validationErrors
}) => {
    const [formErrors, setFormErrors] = useState<string[]>([]);
    const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);

    // Watch form values
    const isActive = Form.useWatch('isActive', form);

    // Clear errors when modal closes
    useEffect(() => {
        if (!showModal) {
            setFormErrors([]);
            setHasAttemptedSubmit(false);
        }
    }, [showModal]);

    // Enhanced onOk handler with validation
    const handleOk = async () => {
        try {
            setHasAttemptedSubmit(true);
            
            // Validate form fields
            await form.validateFields();
            
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
            { required: true, message: 'Please enter an alarm name' },
            { min: 2, message: 'Name must be at least 2 characters' },
            { max: 100, message: 'Name cannot exceed 100 characters' }
        ],
        companyId: [
            { required: true, message: 'Please select a company' }
        ],
        recipientSetId: [
            // Optional field
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
                        background: 'linear-gradient(135deg, #fa541c 0%, #ff7a45 100%)',
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
            onCancel={onCancel}
            onOk={handleOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={600}
            styles={{
                header: {
                    background: '#fa541c',
                    borderRadius: '8px 8px 0 0',
                    padding: '16px 24px',
                    border: '2px solid #fa541c',
                    borderBottom: 'none',
                    marginBottom: '0'
                },
                body: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #fa541c',
                    borderTop: 'none',
                    borderBottom: 'none',
                    marginTop: '0'
                },
                footer: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #fa541c',
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
                }}>x</span>
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
                    companyId: undefined,
                    recipientSetId: undefined,
                    isActive: true,
                }}
            >
                <Form.Item
                    label="Alarm Name"
                    name="name"
                    rules={rules.name}
                    style={{ marginBottom: '16px' }}
                    hasFeedback
                >
                    <Input 
                        placeholder="Enter alarm name" 
                        maxLength={100}
                        showCount
                    />
                </Form.Item>

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
                            Recipient Set{' '}
                            <span style={{ 
                                color: '#8c8c8c', 
                                fontWeight: 'normal',
                                fontSize: '12px' 
                            }}>
                                (Optional)
                            </span>
                        </span>
                    }
                    name="recipientSetId"
                    rules={rules.recipientSetId}
                    style={{ marginBottom: '16px' }}
                    help="Select a recipient set for notifications, or leave blank if no notifications needed"
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select recipient set (optional)"
                        optionFilterProp="children"
                        notFoundContent="No recipient sets available"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {/* Add recipient sets here when available */}
                    </Select>
                </Form.Item>

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
                            transform: 'scale(1.2)',
                            transformOrigin: 'left center',
                            minWidth: '80px'
                        }}
                    />
                </Form.Item>
            </Form>
        </Modal>
    );
};