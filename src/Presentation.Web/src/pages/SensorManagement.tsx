import React, { useEffect, useState } from "react";
import { Button, Card, message, Form, Space } from "antd";
import { PlusOutlined, EditOutlined } from "@ant-design/icons";
import { SensorListModal } from "../features/sensors/SensorListModal";
import {
    fetchSensors,
    addSensor,
    updateSensor,
    type Sensor,
    type SensorRequest
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

// Interface for API validation errors
interface ValidationError {
    field: string;
    message: string;
}

interface ApiError {
    message: string;
    errors?: Record<string, string[]>;
    validationErrors?: ValidationError[];
}

const SensorManagement: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    
    // Modal state
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [modalLoading, setModalLoading] = useState(false);
    const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
    const [form] = Form.useForm();
    
    // Data state
    const [sensors, setSensors] = useState<Sensor[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [loading, setLoading] = useState(false);

    // Load initial data
    useEffect(() => {
        loadData();
    }, [token]);

    const loadData = async () => {
        setLoading(true);
        try {
            const [sensorsData, companiesData, alarmsData] = await Promise.all([
                fetchSensors(token),
                fetchCompanies(token),
                fetchAlarms(token)
            ]);
            
            setSensors(sensorsData);
            setCompanies(companiesData);
            setAlarms(alarmsData);
        } catch (error) {
            console.error('Failed to load data:', error);
            message.error('Failed to load data');
        } finally {
            setLoading(false);
        }
    };

    // Parse API errors
    const parseApiError = (error: any): ApiError => {
        if (error.response?.data) {
            const { data } = error.response;
            
            // Handle different error response formats
            if (data.errors) {
                return {
                    message: data.message || 'Validation failed',
                    errors: data.errors
                };
            }
            
            if (data.validationErrors) {
                const errors: Record<string, string[]> = {};
                data.validationErrors.forEach((validationError: ValidationError) => {
                    if (!errors[validationError.field]) {
                        errors[validationError.field] = [];
                    }
                    errors[validationError.field].push(validationError.message);
                });
                
                return {
                    message: data.message || 'Validation failed',
                    errors
                };
            }
            
            return {
                message: data.message || error.message || 'An error occurred'
            };
        }
        
        return {
            message: error.message || 'An unknown error occurred'
        };
    };

    // Modal handlers
    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        setValidationErrors({});
        form.resetFields();
        setShowModal(true);
    };

    const handleEdit = (sensor: Sensor) => {
        setIsEdit(true);
        setEditingId(sensor.id);
        setValidationErrors({});
        form.setFieldsValue({
            name: sensor.name ?? "",
            channel: sensor.channel,
            channelType: sensor.channelType,
            lowValue: sensor.lowValue,
            highValue: sensor.highValue,
            engineeringUnits: sensor.engineeringUnits ?? "",
            unitId: sensor.unitId,
            companyId: sensor.companyId,
            alarmId: sensor.alarmId,
        });
        setShowModal(true);
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            setValidationErrors({}); // Clear previous validation errors
            
            const values = await form.validateFields();
            
            // Normalize values
            const normalizedValues: SensorRequest = {
                ...values,
                unitId: Number(values.unitId) || 0,
                companyId: values.companyId || null,
                alarmId: values.alarmId || null,
                channelType: values.channelType || null,
                lowValue: values.lowValue || null,
                highValue: values.highValue || null,
            };
            
            if (isEdit && editingId !== null) {
                const sensorData = {
                    ...normalizedValues,
                    id: editingId
                };
                
                await updateSensor(editingId, sensorData, token);
                setSensors(prev =>
                    prev.map(s =>
                        s.id === editingId ? { ...s, ...normalizedValues } : s
                    )
                );
                message.success("Sensor updated successfully");
            } else {
                const newSensor = await addSensor(normalizedValues, token);
                setSensors(prev => [...prev, newSensor]);
                message.success("Sensor added successfully");
            }
            
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            console.error('Form submission error:', err);
            
            // Handle form validation errors (from Ant Design)
            if (err.errorFields) {
                // These are handled by the form itself, don't close modal
                return;
            }
            
            // Handle API errors
            const apiError = parseApiError(err);
            
            if (apiError.errors) {
                // Set validation errors to be displayed in the modal
                setValidationErrors(apiError.errors);
                
                // Show a general error message
                message.error(apiError.message);
            } else {
                // Generic error handling
                message.error(apiError.message);
                
                // Close modal for non-validation errors
                if (!apiError.message.toLowerCase().includes('validation')) {
                    setShowModal(false);
                    setEditingId(null);
                    form.resetFields();
                }
            }
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        setValidationErrors({});
        form.resetFields();
    };

    return (
        <div style={{ padding: "24px" }}>
            <Card 
                title="Sensor Management" 
                loading={loading}
                extra={
                    <Button 
                        type="primary" 
                        icon={<PlusOutlined />}
                        onClick={handleAdd}
                    >
                        Add Sensor
                    </Button>
                }
            >
                {/* Your sensor list/table here */}
                <div>
                    {sensors.map(sensor => (
                        <Card 
                            key={sensor.id} 
                            size="small" 
                            style={{ marginBottom: 8 }}
                            extra={
                                <Button 
                                    size="small" 
                                    icon={<EditOutlined />}
                                    onClick={() => handleEdit(sensor)}
                                >
                                    Edit
                                </Button>
                            }
                        >
                            <p><strong>Name:</strong> {sensor.name}</p>
                            <p><strong>Channel:</strong> {sensor.channel}</p>
                            <p><strong>Unit ID:</strong> {sensor.unitId}</p>
                            {sensor.engineeringUnits && (
                                <p><strong>Units:</strong> {sensor.engineeringUnits}</p>
                            )}
                        </Card>
                    ))}
                </div>
            </Card>

            {/* The SensorListModal with validation error support */}
            <SensorListModal
                showModal={showModal}
                isEdit={isEdit}
                modalLoading={modalLoading}
                form={form}
                companies={companies}
                alarms={alarms}
                validationErrors={validationErrors}
                onOk={handleModalOk}
                onCancel={handleModalCancel}
            />
        </div>
    );
};

export default SensorManagement;