import { useEffect, useState } from "react";
import { Table, Button, Modal, Input, Form, InputNumber, Space, Popconfirm, message, Dropdown, Checkbox, Select } from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined, DownOutlined } from "@ant-design/icons";
import {
  fetchUnits,
  addUnit,
  updateUnit,
  deleteUnit,
  type Unit
} from "../features/units/unitsAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchUnitModels, type UnitModel } from "../features/unitmodels/unitModelAPI";
import { fetchSensorsByUnit, addSensor, updateSensor, deleteSensor, type Sensor } from "../features/sensors/sensorAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

const unitStatusOptions = [
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
  { value: "Maintenance", label: "Maintenance" },
  { value: "Decommissioned", label: "Decommissioned" },
  // Add more statuses as defined in your backend enum
];

const allColumnDefs = [
  { title: "ID", dataIndex: "id", key: "id", width: 100 },
  { title: "Unit Type ID", dataIndex: "unitTypeId", key: "unitTypeId", width: 120 },
  { title: "Phone Number", dataIndex: "phoneNumber", key: "phoneNumber", width: 140 },
  { title: "PIN", dataIndex: "pin", key: "pin", width: 80 },
  { title: "Manufacturer Code", dataIndex: "manufacturerCode", key: "manufacturerCode", width: 150 },
  { title: "Unit Code", dataIndex: "unitCode", key: "unitCode", width: 120 },
  { title: "Secret", dataIndex: "secret", key: "secret", width: 120 },
  { title: "Unit Status ID", dataIndex: "unitStatusID", key: "unitStatusID", width: 120 },
  { title: "Company", dataIndex: "companyID", key: "companyID", width: 180,
    render: (companyID: string | null, record: Unit, index: number, companies?: Company[]) => companyID
  },
  { title: "Days Before Not Reported", dataIndex: "daysBeforeNotReported", key: "daysBeforeNotReported", width: 180 },
  { title: "Custom Field Values", dataIndex: "customFieldValues", key: "customFieldValues", width: 180 },
];

const UnitList: React.FC = () => {
  const auth = useAppSelector(selectAuth);
  const token = auth?.accessToken;
  const [units, setUnits] = useState<Unit[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [form] = Form.useForm();
  const [modalLoading, setModalLoading] = useState(false);

  // Column visibility state
  const [visibleKeys, setVisibleKeys] = useState<string[]>(allColumnDefs.map(col => col.key as string));
  const [columns, setColumns] = useState(allColumnDefs);

  // Sensor-related states
  const [sensorModal, setSensorModal] = useState<{ open: boolean, unitId?: string, sensor?: Sensor }>({ open: false });
  const [sensorForm] = Form.useForm();
  const [sensorModalLoading, setSensorModalLoading] = useState(false);
  const [sensorsByUnit, setSensorsByUnit] = useState<Record<string, Sensor[]>>({});

  useEffect(() => {
    loadUnits();
    fetchCompanies(token).then(setCompanies).catch(() => setCompanies([]));
    fetchUnitModels(token).then(setUnitModels).catch(() => setUnitModels([]));
    // eslint-disable-next-line
  }, [token]);

  useEffect(() => {
    setColumns(
      allColumnDefs
        .filter(col => visibleKeys.includes(col.key as string))
        .map(col => {
          if (col.key === "companyID") {
            return {
              ...col,
              render: (companyID: string | null) => {
                if (!companyID) return "";
                const company = companies.find(c => c.id === companyID);
                return company ? company.name : companyID;
              }
            };
          }
          if (col.key === "unitTypeId") {
            return {
              ...col,
              render: (unitTypeId: string) => {
                const model = unitModels.find(m => m.id.toString() === unitTypeId);
                return model ? `${model.code} - ${model.name}` : unitTypeId;
              }
            };
          }
          if (col.key === "unitStatusID") {
            return {
              ...col,
              render: (status: string) => {
                const found = unitStatusOptions.find(opt => opt.value === status);
                return found ? found.label : status;
              }
            };
          }
          return col;
        })
    );
  }, [visibleKeys, companies, unitModels]);

  const loadUnits = async () => {
    setLoading(true);
    try {
      const data = await fetchUnits(token);
      setUnits(data);
      setError(null);
      const sensorsData = await Promise.all(data.map(unit => fetchSensorsByUnit(unit.id, token)));
      const sensorsMap = data.reduce((acc, unit, index) => {
        acc[unit.id] = sensorsData[index];
        return acc;
      }, {} as Record<string, Sensor[]>);
      setSensorsByUnit(sensorsMap);
    } catch (err: any) {
      setError(err.message || "An error occurred while fetching units.");
      setUnits([]);
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

  const handleEdit = (record: Unit) => {
    setIsEdit(true);
    setEditingId(record.id);
    form.setFieldsValue({
      unitTypeId: record.unitTypeId,
      phoneNumber: record.phoneNumber ?? "",
      pin: record.pin ?? "",
      manufacturerCode: record.manufacturerCode,
      unitCode: record.unitCode ?? "",
      secret: record.secret ?? "",
      unitStatusID: record.unitStatusID,
      companyID: record.companyID ?? undefined,
      daysBeforeNotReported: record.daysBeforeNotReported,
      customFieldValues: record.customFieldValues ?? "",
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteUnit(id, token);
      setUnits(prev => prev.filter(u => u.id !== id));
      message.success("Unit deleted");
    } catch (err: any) {
      message.error(err.message || "Failed to delete unit");
    }
  };

  const handleModalOk = async () => {
    try {
      setModalLoading(true);
      const values = await form.validateFields();
      if (isEdit && editingId !== null) {
        await updateUnit(editingId, values, token);
        setUnits(prev =>
          prev.map(u =>
            u.id === editingId ? { ...u, ...values } : u
          )
        );
        message.success("Unit updated");
      } else {
        const added = await addUnit(values, token);
        setUnits(prev => [...prev, added]);
        message.success("Unit added");
      }
      setShowModal(false);
      setEditingId(null);
      form.resetFields();
    } catch (err: any) {
      if (err.errorFields) return; // Form validation error
      message.error(err.message || "Failed to save unit");
    } finally {
      setModalLoading(false);
    }
  };

  const handleModalCancel = () => {
    setShowModal(false);
    setEditingId(null);
    form.resetFields();
  };

  // Sensor modal handlers
  const openSensorModal = (unitId: string, sensor?: Sensor) => {
    setSensorModal({ open: true, unitId, sensor });
    sensorForm.setFieldsValue(sensor ? { ...sensor } : { name: "", channel: 0, channelType: undefined, lowValue: undefined, highValue: undefined, engineeringUnits: "", unitId });
  };

  const handleSensorModalOk = async () => {
    try {
      setSensorModalLoading(true);
      const values = await sensorForm.validateFields();
      if (sensorModal.sensor) {
        await updateSensor(sensorModal.sensor.id, values, token);
        message.success("Sensor updated");
      } else {
        await addSensor({ ...values, unitId: sensorModal.unitId }, token);
        message.success("Sensor added");
      }
      setSensorModal({ open: false });
      sensorForm.resetFields();
      loadUnits(); // Reload sensors for the unit
    } catch (err: any) {
      if (err.errorFields) return;
      message.error(err.message || "Failed to save sensor");
    } finally {
      setSensorModalLoading(false);
    }
  };

  const handleSensorModalCancel = () => {
    setSensorModal({ open: false });
    sensorForm.resetFields();
  };

  const handleDeleteSensor = async (sensorId: string, unitId: string) => {
    try {
      await deleteSensor(sensorId, token);
      message.success("Sensor deleted");
      loadUnits(); // Reload sensors for the unit
    } catch (err: any) {
      message.error(err.message || "Failed to delete sensor");
    }
  };

  // Dropdown menu items for columns
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

  // Add the Actions column after filtering
  const tableColumns = [
    ...columns,
    {
      title: "Actions",
      key: "actions",
      width: 120,
      render: (_: any, record: Unit) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => handleEdit(record)}
          />
          <Popconfirm
            title="Delete this unit?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              icon={<DeleteOutlined />}
              size="small"
              danger
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  if (loading) return <div>Loading units...</div>;
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
      <h2>Units</h2>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={handleAdd}
        style={{ marginBottom: 16 }}
      >
        Add Unit
      </Button>
      <Modal
        title={isEdit ? "Edit Unit" : "Add Unit"}
        open={showModal}
        onCancel={handleModalCancel}
        onOk={handleModalOk}
        okText="Save"
        confirmLoading={modalLoading}
        destroyOnClose
      >
        <Form
          layout="vertical"
          form={form}
          initialValues={{
            unitTypeId: "",
            phoneNumber: "",
            pin: "",
            manufacturerCode: "",
            unitCode: "",
            secret: "",
            unitStatusID: "",
            companyID: undefined,
            daysBeforeNotReported: undefined,
            customFieldValues: "",
          }}
        >
          <Form.Item
            label="Unit Type"
            name="unitTypeId"
            rules={[{ required: true, message: "Please select a unit type" }]}
          >
            <Select
              showSearch
              allowClear
              placeholder="Select a unit type"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.children ?? "").toLowerCase().includes(input.toLowerCase())
              }
            >
              {unitModels.map(model => (
                <Select.Option key={model.id} value={model.id.toString()}>
                  {model.code} - {model.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item label="Phone Number" name="phoneNumber">
            <Input placeholder="Phone Number" />
          </Form.Item>
          <Form.Item label="PIN" name="pin">
            <Input placeholder="PIN" />
          </Form.Item>
          <Form.Item
            label="Manufacturer Code"
            name="manufacturerCode"
            rules={[{ required: true, message: "Please enter a manufacturer code" }]}
          >
            <Input placeholder="Manufacturer Code" />
          </Form.Item>
          <Form.Item label="Unit Code" name="unitCode">
            <Input placeholder="Unit Code" />
          </Form.Item>
          <Form.Item label="Secret" name="secret">
            <Input placeholder="Secret" />
          </Form.Item>
          <Form.Item
            label="Unit Status"
            name="unitStatusID"
            rules={[{ required: true, message: "Please select a unit status" }]}
          >
            <Select
              showSearch
              allowClear
              placeholder="Select a unit status"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.children ?? "").toLowerCase().includes(input.toLowerCase())
              }
            >
              {unitStatusOptions.map(status => (
                <Select.Option key={status.value} value={status.value}>
                  {status.label}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item label="Company" name="companyID">
            <Select
              allowClear
              showSearch
              placeholder="Select a company"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.children ?? "").toLowerCase().includes(input.toLowerCase())
              }
            >
              {companies.map(company => (
                <Select.Option key={company.id} value={company.id}>
                  {company.name}
                </Select.Option>
              ))}
            </Select>
          </Form.Item>
          <Form.Item label="Days Before Not Reported" name="daysBeforeNotReported">
            <InputNumber min={0} style={{ width: "100%" }} placeholder="Days Before Not Reported" />
          </Form.Item>
          <Form.Item label="Custom Field Values" name="customFieldValues">
            <Input placeholder="Custom Field Values" />
          </Form.Item>
        </Form>
      </Modal>
      <Table<Unit>
        bordered
        dataSource={units}
        columns={tableColumns}
        rowKey="id"
        pagination={false}
        scroll={{ x: "max-content" }}
        expandable={{
          expandedRowRender: (unit: Unit) => {
            const sensors = sensorsByUnit[unit.id] || [];
            return (
              <div>
                <Button
                  type="primary"
                  size="small"
                  onClick={() => openSensorModal(unit.id)}
                  style={{ marginBottom: 8 }}
                >
                  Add Sensor
                </Button>
                <Table<Sensor>
                  dataSource={sensors}
                  columns={[
                    { title: "Name", dataIndex: "name", key: "name" },
                    { title: "Channel", dataIndex: "channel", key: "channel" },
                    { title: "Channel Type", dataIndex: "channelType", key: "channelType" },
                    { title: "Low Value", dataIndex: "lowValue", key: "lowValue" },
                    { title: "High Value", dataIndex: "highValue", key: "highValue" },
                    { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits" },
                    {
                      title: "Actions",
                      key: "actions",
                      render: (_: any, record: Sensor) => (
                        <Space>
                          <Button size="small" onClick={() => openSensorModal(unit.id, record)}>Edit</Button>
                          <Popconfirm
                            title="Delete this sensor?"
                            onConfirm={() => handleDeleteSensor(record.id, unit.id)}
                            okText="Yes"
                            cancelText="No"
                          >
                            <Button size="small" danger>Delete</Button>
                          </Popconfirm>
                        </Space>
                      ),
                    },
                  ]}
                  rowKey="id"
                  pagination={false}
                  size="small"
                />
                <Modal
                  title={sensorModal.sensor ? "Edit Sensor" : "Add Sensor"}
                  open={sensorModal.open}
                  onCancel={handleSensorModalCancel}
                  onOk={handleSensorModalOk}
                  confirmLoading={sensorModalLoading}
                  destroyOnClose
                >
                  <Form layout="vertical" form={sensorForm}>
                    <Form.Item label="Name" name="name" rules={[{ required: true, message: "Please enter a name" }]}>
                      <Input />
                    </Form.Item>
                    <Form.Item label="Channel" name="channel" rules={[{ required: true, message: "Please enter a channel" }]}>
                      <InputNumber min={0} style={{ width: "100%" }} />
                    </Form.Item>
                    <Form.Item label="Channel Type" name="channelType">
                      <InputNumber min={0} style={{ width: "100%" }} />
                    </Form.Item>
                    <Form.Item label="Low Value" name="lowValue">
                      <InputNumber style={{ width: "100%" }} />
                    </Form.Item>
                    <Form.Item label="High Value" name="highValue">
                      <InputNumber style={{ width: "100%" }} />
                    </Form.Item>
                    <Form.Item label="Engineering Units" name="engineeringUnits">
                      <Input />
                    </Form.Item>
                  </Form>
                </Modal>
              </div>
            );
          },
        }}
      />
    </div>
  );
};

export default UnitList;