import { useEffect, useState } from "react";
import { Modal, Form, message, Button } from "antd";
import { BarChartOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { fetchReadings, fetchReadingsBySensorId, fetchReadingsByUnitId, deleteReading, type Reading } from "../features/readings/readingAPI";
import { getReadingColumns, getReadingColumnsWithoutSensor } from "../features/readings/readingColumns";
import ReadingModal from "../features/readings/ReadingModal";
import { fetchSensors, type Sensor } from "../features/sensors/sensorAPI";
import { fetchUnits, type Unit } from "../features/units/unitsAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable as NewExtendedAntDTable } from "../components/NewExtendedAntDTable";
import ReadingsChart from "../components/ReadingsChart";

interface ReadingsListProps {
    sensorId?: number; // Optional sensor ID to filter readings for a specific sensor
    unitId?: number;   // Optional unit ID to filter readings for a specific unit
}

/**
 * ReadingsList component displays telemetry readings data.
 * When sensorId prop is provided, it shows only readings for that specific sensor.
 * When unitId prop is provided, it shows only readings for that specific unit.
 * Usage:
 * - <ReadingsList /> - Shows all readings
 * - <ReadingsList sensorId={123} /> - Shows readings for sensor with ID 123
 * - <ReadingsList unitId={456} /> - Shows readings for unit with ID 456
 * - Route: /readings/sensor/:sensorId - URL-based sensor filtering
 * - Route: /readings/unit/:unitId - URL-based unit filtering
 */
const ReadingsList: React.FC<ReadingsListProps> = ({ sensorId, unitId }) => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [readings, setReadings] = useState<Reading[]>([]);
    const [sensors, setSensors] = useState<Sensor[]>([]);
    const [units, setUnits] = useState<Unit[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [showChartModal, setShowChartModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingKey, setEditingKey] = useState<{ dateRecordedUtc: string, sensorId: number } | null>(null);
    const [form] = Form.useForm();

    // Get column definitions based on whether we're filtering by sensor
    const allColumnDefs = sensorId ? getReadingColumnsWithoutSensor() : getReadingColumns();

    // Get display name for page title
    const getDisplayName = () => {
        if (sensorId) {
            const sensor = sensors.find(s => s.id === sensorId);
            return sensor ? `Telemetry - ${sensor.name}` : `Telemetry - Sensor ${sensorId}`;
        } else if (unitId) {
            const unit = units.find(u => u.id === unitId);
            return unit ? `Telemetry - Unit ${unit.unitCode || unit.id}` : `Telemetry - Unit ${unitId}`;
        }
        return "Telemetry";
    };

    useEffect(() => {
        loadReadings();
        fetchSensors(token).then(setSensors).catch(() => setSensors([]));
        fetchUnits(token).then(setUnits).catch(() => setUnits([]));
        // eslint-disable-next-line
    }, [token, sensorId, unitId]);

    const loadReadings = async () => {
        setLoading(true);
        try {
            let data: Reading[];
            if (sensorId) {
                data = await fetchReadingsBySensorId(sensorId, token);
            } else if (unitId) {
                data = await fetchReadingsByUnitId(unitId, token);
            } else {
                data = await fetchReadings(token);
            }
            setReadings(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching readings.");
            setReadings([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingKey(null);
        form.resetFields();
        // Pre-select sensor if sensorId is provided
        if (sensorId) {
            form.setFieldsValue({ sensorId: sensorId });
        }
        setShowModal(true);
    };

    const handleEdit = (record: Reading) => {
        setIsEdit(true);
        setEditingKey({ dateRecordedUtc: record.dateRecordedUtc, sensorId: record.sensor.id });
        form.setFieldsValue({
            dateReceivedUtc: dayjs(record.dateReceivedUtc),
            dateRecordedUtc: dayjs(record.dateRecordedUtc),
            sensorId: record.sensor.id,
            value: record.value,
        });
        setShowModal(true);
    };

    const handleDelete = async (record:Reading) => {
        try {
            if(!record.sensor || !record.dateRecordedUtc) {
                message.error("Sensor and date recorded are required for deletion");
                return;
            }
            await deleteReading(record, token);
            setReadings(prev => prev.filter(r => !(r.dateRecordedUtc === record.dateRecordedUtc && r.sensor.id === record.sensor.id)));
            message.success("Reading deleted");
        } catch (err: any) {
            message.error(err.message || "Failed to delete reading");
        }
    };

    const handleModalSuccess = (reading: Reading, isEdit: boolean, editingKey?: { dateRecordedUtc: string, sensorId: number } | null) => {
        if (isEdit && editingKey) {
            setReadings(prev =>
                prev.map(r =>
                    r.dateRecordedUtc === editingKey.dateRecordedUtc && r.sensor.id === editingKey.sensorId
                        ? reading
                        : r
                )
            );
        } else {
            setReadings(prev => [...prev, reading]);
        }
        setShowModal(false);
        setEditingKey(null);
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingKey(null);
        form.resetFields();
    };

    const ChartModal: React.FC = () => (
        <Modal
            title="Telemetry Chart"
            open={showChartModal}
            onCancel={() => setShowChartModal(false)}
            footer={null}
            width={1000}
        >
            <ReadingsChart readings={readings} />
        </Modal>
    );

    // Custom action for showing chart
    const chartAction = () => (
        <Button
            icon={<BarChartOutlined />}
            size="small"
            style={{ 
                margin: "0 10px 0 0",
                backgroundColor: "#1890ff",
                borderColor: "#1890ff",
                color: "white"
            }}
            onClick={() => setShowChartModal(true)}
        >
            Graph
        </Button>
    );

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <ReadingModal 
                open={showModal}
                isEdit={isEdit}
                editingKey={editingKey}
                sensors={sensors}
                token={token}
                onCancel={handleModalCancel}
                onSuccess={handleModalSuccess}
                form={form}
            />
            <ChartModal />
            <NewExtendedAntDTable<Reading>
                data={readings}
                tableColumns={allColumnDefs}
                title={getDisplayName()}
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                headerActions={chartAction()}
                loading={loading}
            />
        </div>
    );
};

export default ReadingsList;