import React, { useState, useEffect } from 'react';
import { Modal, Form, Input, Alert, FormInstance } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import type { UnitModel } from './unitModelAPI';

interface UnitModelModalProps {
    showModal: boolean;
    isEdit: boolean;
    modalLoading: boolean;
    form: FormInstance;
    onOk: () => Promise<void>;
    onCancel: () => void;
    validationErrors?: Record<string, string[]>; // API validation errors
}

export const UnitModelModal: React.FC<UnitModelModalProps> = ({
    showModal,
    isEdit,
    modalLoading,
    form,
    onOk,
    onCancel,
    validationErrors
}) => {
    const [formErrors, setFormErrors] = useState<string[]>([]);
    const [hasAttemptedSubmit, setHasAttemptedSubmit] = useState(false);

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
        code: [
            { required: true, message: 'Please enter a code' },
            { min: 1, message: 'Code must be at least 1 character' },
            { max: 20, message: 'Code cannot exceed 20 characters' },
            { 
                pattern: /^[a-zA-Z0-9\-_]+$/, 
                message: 'Code can only contain letters, numbers, hyphens, and underscores' 
            }
        ],
        name: [
            { required: true, message: 'Please enter a name' },
            { min: 2, message: 'Name must be at least 2 characters' },
            { max: 100, message: 'Name cannot exceed 100 characters' }
        ],
        description: [
            { max: 500, message: 'Description cannot exceed 500 characters' }
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
                        {isEdit ? "E" : "U"}
                    </div>
                    {isEdit ? "Edit Unit Model" : "Add Unit Model"}
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
                    code: "",
                    name: "",
                    description: "",
                }}
            >
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item
                        label="Code"
                        name="code"
                        rules={rules.code}
                        style={{ marginBottom: 0 }}
                        hasFeedback
                    >
                        <Input 
                            placeholder="Enter unit model code" 
                            maxLength={20}
                            showCount
                        />
                    </Form.Item>
                    
                    <Form.Item
                        label="Name"
                        name="name"
                        rules={rules.name}
                        style={{ marginBottom: 0 }}
                        hasFeedback
                    >
                        <Input 
                            placeholder="Enter unit model name" 
                            maxLength={100}
                            showCount
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Description"
                    name="description"
                    rules={rules.description}
                    style={{ marginBottom: 0 }}
                >
                    <Input.TextArea 
                        placeholder="Enter description (optional)" 
                        maxLength={500}
                        showCount
                        rows={3}
                    />
                </Form.Item>
            </Form>
        </Modal>
    );
};