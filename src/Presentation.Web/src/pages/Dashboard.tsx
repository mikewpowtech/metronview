import { useEffect, useState } from "react";
import { Table, Button, Space, Dropdown, Checkbox, message } from "antd";
import { DownOutlined } from "@ant-design/icons";
import { fetchUnits, type Unit } from "../features/units/unitsAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchUnitModels, type UnitModel } from "../features/unitmodels/unitModelAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

// Persistent column visibility utilities
const COLUMN_VISIBILITY_KEY = "dashboard.visibleColumns";
function getPersistedVisibleKeys(defaultKeys: string[]) {
  if (typeof window === "undefined") return defaultKeys;
  const stored = localStorage.getItem(COLUMN_VISIBILITY_KEY);
  if (!stored) return defaultKeys;
  try {
    const parsed = JSON.parse(stored);
    if (Array.isArray(parsed)) return parsed;
    return defaultKeys;
  } catch {
    return defaultKeys;
  }
}
function setPersistedVisibleKeys(keys: string[]) {
  if (typeof window !== "undefined") {
    localStorage.setItem(COLUMN_VISIBILITY_KEY, JSON.stringify(keys));
  }
}

const allColumnDefs = [
  { title: "ID", dataIndex: "id", key: "id", width: 100 },
  { title: "Unit Type", dataIndex: "unitTypeId", key: "unitTypeId", width: 120 },
  { title: "Last Comms(UTC)", dataIndex: "lastComms", key: "lastComms", width: 140 },
  { title: "Alarm", dataIndex: "alarm", key: "alarm", width: 80 },
{ title: "Readings", dataIndex: "readings", key: "readings", width: 80 },
{ title: "Ambient Temperature", dataIndex: "ambientTemperature", key: "ambientTemperature", width: 150 },
  { title: "Carrier", dataIndex: "carrier", key: "carrier", width: 120 },
    { title: "Signal", dataIndex: "signal", key: "signal", width: 120 },
    { title: "Latitude", dataIndex: "latitude", key: "latitude", width: 120 },
    { title: "Longitude", dataIndex: "longitude", key: "longitude", width: 120 },
    { title: "Unit ID", dataIndex: "unitId", key: "unitId", width: 120 },
  { title: "Company", dataIndex: "companyID", key: "companyID", width: 180 },
];

const Dashboard: React.FC = () => {
  const auth = useAppSelector(selectAuth);
  const token = auth?.accessToken;
  const [units, setUnits] = useState<Unit[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Persistent column visibility state
  const defaultVisibleKeys = allColumnDefs.filter(col => col.key !== "id").map(col => col.key as string);
  const [visibleKeys, setVisibleKeys] = useState<string[]>(() => getPersistedVisibleKeys(defaultVisibleKeys));
  const [columns, setColumns] = useState(allColumnDefs);

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
              render: (_: never, record: Unit) => {
                if (!record.companyID) return "";
                const company = companies.find(c => c.id === record.companyID);
                return company?.name ?? "";
              }
            };
          }
          if (col.key === "unitTypeId") {
            return {
              ...col,
              render: (_: never, record: Unit) => {
                const model = unitModels.find(m => m.id === record.unitTypeId);
                return model?.name ?? "not specified";
              }
            };
          }
          return col;
        })
    );
    setPersistedVisibleKeys(visibleKeys);
  }, [visibleKeys, companies, unitModels]);

  const loadUnits = async () => {
    setLoading(true);
    try {
      const data = await fetchUnits(token);
      setUnits(data);
      setError(null);
    } catch (err: any) {
      setError(err.message || "An error occurred while fetching units.");
      setUnits([]);
    } finally {
      setLoading(false);
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
          setVisibleKeys(keys => {
            const newKeys = checked
              ? [...keys, col.key as string]
              : keys.filter(k => k !== col.key);
            setPersistedVisibleKeys(newKeys);
            return newKeys;
          });
        }}
        disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
        style={{ width: "100%", padding: "4px 12px" }}
      >
        {col.title}
      </Checkbox>
    ),
  }));

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
      <h2>Home - Units & Readings Overview</h2>
      <Table<Unit>
        bordered
        dataSource={units}
        columns={columns}
        rowKey="id"
        pagination={false}
        scroll={{ x: "max-content" }}
      />
    </div>
  );
};

export default Dashboard;