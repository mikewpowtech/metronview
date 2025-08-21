import React from 'react';
import { Modal, Form, Input, InputNumber, Select, Switch } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import type { Unit } from './unitsAPI';
import type { Company } from '../companies/companyAPI';
import type { UnitModel } from '../unitmodels/unitModelAPI';

interface UnitListModalProps {
    showModal: boolean;
    isEdit: boolean;
    modalLoading: boolean;
    form: any; // FormInstance from antd
    companies: Company[];
    unitModels: UnitModel[];
    onOk: () => Promise<void>;
    onCancel: () => void;
}

export const UnitListModal: React.FC<UnitListModalProps> = ({
    showModal,
    isEdit,
    modalLoading,
    form,
    companies,
    unitModels,
    onOk,
    onCancel
}) => {
    const statusValue = Form.useWatch('status', form) || 0;
    
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
                    <PlusOutlined style={{ color: '#ffffff' }} />
                    {isEdit ? "Edit Unit" : "Add Unit"}
                </div>
            }
            open={showModal}
            onCancel={onCancel}
            onOk={onOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={520}
            styles={{
                header: {
                    background: '#1890ff',
                    borderRadius: '8px 8px 0 0',
                    padding: '12px 20px',
                    border: '2px solid #1890ff',
                    borderBottom: 'none',
                    marginBottom: '0'
                },
                body: {
                    background: '#ffffff',
                    padding: '16px 20px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderBottom: 'none',
                    marginTop: '0'
                },
                footer: {
                    background: '#ffffff',
                    padding: '12px 20px',
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
            <Form
                layout="vertical"
                form={form}
                size="small"
                initialValues={{
                    unitTypeId: "",
                    phoneNumber: "",
                    pin: "",
                    manufacturerCode: "",
                    unitCode: "",
                    secret: "",
                    status: 0, // Default to Active
                    companyID: undefined,
                    daysBeforeNotReported: undefined,
                    customFieldValues: "",
                }}
            >
                {/* Row 1: Manufacturer Code */}
                <Form.Item
                    label="Manufacturer Code"
                    name="manufacturerCode"
                    rules={[{ required: true, message: "Please enter a manufacturer code" }]}
                    style={{ marginBottom: 10 }}
                >
                    <Input placeholder="Manufacturer Code" />
                </Form.Item>

                {/* Row 2: Unit Type */}
                <Form.Item
                    label="Unit Type"
                    name="unitTypeId"
                    rules={[{ required: true, message: "Please select a unit type" }]}
                    style={{ marginBottom: 10 }}
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a unit type"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {unitModels.map(model => (
                            <Select.Option key={model.id} value={model.id.toString()}>
                                {model.code} - {model.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                {/* Row 3: Company */}
                <Form.Item label="Company" name="companyID" style={{ marginBottom: 10 }}>
                    <Select
                        allowClear
                        showSearch
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

                {/* Row 4: Phone Number and PIN */}
                <div style={{ display: 'flex', gap: '12px' }}>
                    <Form.Item label="Phone Number" name="phoneNumber" style={{ flex: 1, marginBottom: 10 }}>
                        <Input placeholder="Phone Number" />
                    </Form.Item>
                    <Form.Item label="PIN" name="pin" style={{ flex: 1, marginBottom: 10 }}>
                        <Input placeholder="PIN" />
                    </Form.Item>
                </div>

                {/* Row 5: Unit Code and Secret */}
                <div style={{ display: 'flex', gap: '12px' }}>
                    <Form.Item label="Unit Code" name="unitCode" style={{ flex: 1, marginBottom: 10 }}>
                        <Input placeholder="Unit Code" />
                    </Form.Item>
                    <Form.Item label="Secret" name="secret" style={{ flex: 1, marginBottom: 10 }}>
                        <Input placeholder="Secret" />
                    </Form.Item>
                </div>

                {/* Row 6: Days Before Not Reported */}
                <Form.Item label="Days Before Not Reported" name="daysBeforeNotReported" style={{ marginBottom: 10 }}>
                    <InputNumber min={0} style={{ width: "100%" }} placeholder="Days" />
                </Form.Item>

                {/* Row 7: Status */}
                <Form.Item
                    label="Status"
                    name="status"
                    style={{ marginBottom: 10 }}
                    valuePropName="checked"
                    getValueFromEvent={(checked) => checked ? 0 : 1}
                    getValueProps={(value) => ({ checked: value === 0 })}
                >
                    <Switch
                        size="default"
                        checkedChildren="Active"
                        unCheckedChildren="Inactive"
                        style={{
                            backgroundColor: statusValue === 0 ? '#52c41a' : '#ff4d4f',
                            transform: 'scale(1.2)', // Make it 20% bigger
                            transformOrigin: 'left center', // Scale from left edge to maintain alignment
                            minWidth: '80px' // Ensure minimum width for text
                        }}
                    />
                </Form.Item>
                
                {/* Row 8: Custom Field Values */}
                <Form.Item label="Custom Field Values" name="customFieldValues" style={{ marginBottom: 6 }}>
                    <Input placeholder="Custom Field Values" />
                </Form.Item>
            </Form>
        </Modal>
    );
};