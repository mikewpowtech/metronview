import React, { useEffect, useState } from "react";
import { Card, Button, List, Spin } from "antd";
import { PlusOutlined, EditOutlined } from "@ant-design/icons";
import { SensorListModal } from "../features/sensors/SensorListModal";
import { useSensorModal } from "../hooks/useSensorModal";
import {
    fetchSensorsByUnit,
    type Sensor
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

interface UnitDetailsWithSensorsProps {
    unitId: number;
}

const UnitDetailsWithSensors: React.FC<UnitDetailsWithSensorsProps> = ({ unitId }) => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    
    // Data state
    const [sensors, setSensors] = useState<Sensor[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [loading, setLoading] = useState(false);

    // Use the custom hook
    const {
        showModal,
        isEdit,
        modalLoading,
        form,
        openAddModal,
        openEditModal,
        handleModalOk,
        handleModalCancel,
    } = useSensorModal({
        token,
        onSensorAdded: (newSensor) => {
            setSensors(prev => [...prev, newSensor]);
        },
        onSensorUpdated: (updatedSensor) => {
            setSensors(prev =>
                prev.map(s => s.id === updatedSensor.id ? updatedSensor : s)
            );
        },
    });

    useEffect(() => {
        loadData();
    }, [unitId, token]);

    const loadData = async () => {
        setLoading(true);
        try {
            const [sensorsData, companiesData, alarmsData] = await Promise.all([
                fetchSensorsByUnit(unitId, token),
                fetchCompanies(token),
                fetchAlarms(token)
            ]);
            
            setSensors(sensorsData);
            setCompanies(companiesData);
            setAlarms(alarmsData);
        } catch (error) {
            console.error('Failed to load data:', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card
            title={`Sensors for Unit ${unitId}`}
            extra={
                <Button 
                    type="primary" 
                    icon={<PlusOutlined />}
                    onClick={openAddModal}
                >
                    Add Sensor
                </Button>
            }
        >
            <Spin spinning={loading}>
                <List
                    dataSource={sensors}
                    renderItem={(sensor) => (
                        <List.Item
                            actions={[
                                <Button 
                                    key="edit"
                                    size="small" 
                                    icon={<EditOutlined />}
                                    onClick={() => openEditModal(sensor)}
                                >
                                    Edit
                                </Button>
                            ]}
                        >
                            <List.Item.Meta
                                title={sensor.name || `Sensor ${sensor.id}`}
                                description={
                                    <div>
                                        <p>Channel: {sensor.channel}</p>
                                        <p>Engineering Units: {sensor.engineeringUnits}</p>
                                        {sensor.lowValue !== null && <p>Low Value: {sensor.lowValue}</p>}
                                        {sensor.highValue !== null && <p>High Value: {sensor.highValue}</p>}
                                    </div>
                                }
                            />
                        </List.Item>
                    )}
                />
            </Spin>

            {/* The SensorListModal */}
            <SensorListModal
                showModal={showModal}
                isEdit={isEdit}
                modalLoading={modalLoading}
                form={form}
                companies={companies}
                alarms={alarms}
                onOk={handleModalOk}
                onCancel={handleModalCancel}
            />
        </Card>
    );
};

export default UnitDetailsWithSensors;