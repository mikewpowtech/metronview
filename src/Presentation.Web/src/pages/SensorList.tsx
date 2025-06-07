import { useEffect, useState } from "react";
import { Table, Button, Dropdown, Checkbox, Modal, Input, Form, InputNumber } from "antd";
import { DownOutlined } from "@ant-design/icons";
import ResizableTitle from "../components/ResizableTitle";
import { fetchSensors, addSensor, type Sensor } from "../features/sensors/sensorAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

const allColumnDefs = [
  { title: "ID", dataIndex: "id", key: "id", width: 100 },
  { title: "Name", dataIndex: "name", key: "name", width: 150 },
  { title: "Channel", dataIndex: "channel", key: "channel", width: 80 },
  { title: "Channel Type", dataIndex: "channelType", key: "channelType", width: 100 },
  { title: "Low Value", dataIndex: "lowValue", key: "lowValue", width: 100 },
  { title: "High Value", dataIndex: "highValue", key: "highValue", width: 100 },
  { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits", width: 150 },
  { title: "Unit ID", dataIndex: "unitId", key: "unitId", width: 120 },
  { title: "Company ID", dataIndex: "companyID", key: "companyID", width: 120 },
];

const SensorList: React.FC = () => {
  const auth = useAppSelector(selectAuth);
  const token = auth?.accessToken;
  const [sensors, setSensors] = useState<Sensor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [newSensor, setNewSensor] = useState<Omit<Sensor, "id">>({
    name: "",
    channel: 0,
    channelType: undefined,
    lowValue: undefined,
    highValue: undefined,
    engineeringUnits: "",
    unitId: "",
    companyID: "",
  });

  // Table column visibility
  const [visibleKeys, setVisibleKeys] = useState<string[]>(allColumnDefs.map(col => col.key as string));
  const [columns, setColumns] = useState(allColumnDefs);

  useEffect(() => {
    setLoading(true);
    fetchSensors(token)
      .then((data) => {
        setSensors(data);
        setError(null);
      })
      .catch((err) => {
        setError(err.message || "An error occurred while fetching sensors.");
        setSensors([]);
      })
      .finally(() => setLoading(false));
  }, [token]);

  useEffect(() => {
    setColumns(
      allColumnDefs.filter(col => visibleKeys.includes(col.key as string))
    );
  }, [visibleKeys]);

  const handleResize = (index: number) => (_: any, { size }: any) => {
    const nextColumns = [...columns];
    nextColumns[index] = {
      ...nextColumns[index],
      width: size.width,
    };
    setColumns(nextColumns);
  };

  const resizeableColumns = columns.map((col, index) => ({
    ...col,
    onHeaderCell: (column: any) => ({
      width: column.width,
      onResize: handleResize(index),
    }),
  }));

  const handleAddSensor = async () => {
    try {
      const addedSensor = await addSensor(newSensor, token);
      setSensors((prev) => [...prev, addedSensor]);
      setShowModal(false);
      setNewSensor({
        name: "",
        channel: 0,
        channelType: undefined,
        lowValue: undefined,
        highValue: undefined,
        engineeringUnits: "",
        unitId: "",
        companyID: "",
      });
    } catch (err: any) {
      setError(err.message || "An error occurred while adding the sensor.");
    }
  };

  const columnMenuItems = allColumnDefs.map(col => ({
    key: col.key,
    label: (
      <Checkbox
        checked={visibleKeys.includes(col.key as string)}
        onChange={e => {
          const checked = e.target.checked;
          setVisibleKeys(keys =>
            checked
              ? [...keys, col.key as string]
              : keys.filter(k => k !== col.key)
          );
        }}
        disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
        style={{ width: "100%", padding: "4px 12px" }}
      >
        {col.title}
      </Checkbox>
    ),
  }));

  if (loading) return <div>Loading sensors...</div>;
  if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Dropdown menu={{ items: columnMenuItems }} trigger={["click"]}>
          <Button>
            Columns <DownOutlined />
          </Button>
        </Dropdown>
      </div>
      <h2>Sensors List</h2>
      <Button onClick={() => setShowModal(true)} style={{ marginBottom: 16 }}>
        Add Sensor
      </Button>
      <Modal
        title="Add Sensor"
        open={showModal}
        onCancel={() => setShowModal(false)}
        onOk={handleAddSensor}
        okText="Save"
        destroyOnClose
      >
        <Form
          layout="vertical"
          onFinish={handleAddSensor}
          initialValues={{
            name: "",
            channel: 0,
            channelType: undefined,
            lowValue: undefined,
            highValue: undefined,
            engineeringUnits: "",
            unitId: "",
            companyID: "",
          }}
        >
          <Form.Item label="Name">
            <Input
              value={newSensor.name ?? ""}
              onChange={e => setNewSensor(u => ({ ...u, name: e.target.value }))}
              placeholder="Sensor Name"
            />
          </Form.Item>
          <Form.Item label="Channel" required>
            <InputNumber
              min={0}
              value={newSensor.channel}
              onChange={v => setNewSensor(u => ({ ...u, channel: v ?? 0 }))}
              placeholder="Channel"
              style={{ width: "100%" }}
            />
          </Form.Item>
          <Form.Item label="Channel Type">
            <InputNumber
              min={0}
              value={newSensor.channelType}
              onChange={v => setNewSensor(u => ({ ...u, channelType: v ?? undefined }))}
              placeholder="Channel Type"
              style={{ width: "100%" }}
            />
          </Form.Item>
          <Form.Item label="Low Value">
            <InputNumber
              value={newSensor.lowValue}
              onChange={v => setNewSensor(u => ({ ...u, lowValue: v ?? undefined }))}
              placeholder="Low Value"
              style={{ width: "100%" }}
            />
          </Form.Item>
          <Form.Item label="High Value">
            <InputNumber
              value={newSensor.highValue}
              onChange={v => setNewSensor(u => ({ ...u, highValue: v ?? undefined }))}
              placeholder="High Value"
              style={{ width: "100%" }}
            />
          </Form.Item>
          <Form.Item label="Engineering Units">
            <Input
              value={newSensor.engineeringUnits ?? ""}
              onChange={e => setNewSensor(u => ({ ...u, engineeringUnits: e.target.value }))}
              placeholder="Engineering Units"
            />
          </Form.Item>
          <Form.Item label="Unit ID">
            <Input
              value={newSensor.unitId ?? ""}
              onChange={e => setNewSensor(u => ({ ...u, unitId: e.target.value }))}
              placeholder="Unit ID"
            />
          </Form.Item>
          <Form.Item label="Company ID">
            <Input
              value={newSensor.companyID ?? ""}
              onChange={e => setNewSensor(u => ({ ...u, companyID: e.target.value }))}
              placeholder="Company ID"
            />
          </Form.Item>
        </Form>
      </Modal>
      <Table<Sensor>
        bordered
        components={{
          header: {
            cell: ResizableTitle,
          },
        }}
        dataSource={sensors}
        columns={resizeableColumns}
        rowKey="id"
        pagination={false}
        scroll={{ x: "max-content" }}
      />
    </div>
  );
};

export default SensorList;