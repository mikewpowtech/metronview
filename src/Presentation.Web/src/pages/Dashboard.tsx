/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import { Button, Tooltip } from "antd";
import { SettingFilled } from "@ant-design/icons";
import { fetchDashboardUnits, type UnitSummary } from "../features/dashboard/dashboardAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { useNavigate } from "react-router-dom";
import dayjs from "dayjs"; // Add this import for date 
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import type { ColumnsType } from "antd/es/table";

// Update columns to match UnitSummary fields
const allColumnDefs : ColumnsType<UnitSummary> = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Unit Type", dataIndex: "unitType", key: "unitType", width: 120 },
    {
        title: "Last Comms(UTC)",
        dataIndex: "lastComms",
        key: "lastComms",
        width: 140,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : "",
    },
    { title: "Carrier", dataIndex: "carrier", key: "carrier", width: 110 },
    { title: "Signal", dataIndex: "signal", key: "signal", width: 90 },
    { title: "Company", dataIndex: "company", key: "company", width: 150 },
];

const Dashboard: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [units, setUnits] = useState<UnitSummary[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();


    useEffect(() => {
        loadUnits();
        // eslint-disable-next-line
    }, [token]);

    const loadUnits = async () => {
        setLoading(true);
        try {
            const data = await fetchDashboardUnits(token);
            setUnits(data);
            setError(null);
        } catch (err: unknown) {
            if (err && typeof err === "object" && "message" in err) {
                setError((err as { message?: string }).message || "An error occurred while fetching dashboard data.");
            } else {
                setError("An error occurred while fetching dashboard data.");
            }
            setUnits([]);
        } finally {
            setLoading(false);
        }
    };

    const actions = (record: UnitSummary) => (
        <Tooltip title="configuration">
            <Button
                icon={<SettingFilled />}
                size="small"
                style={{ padding: 0, minWidth: 0, width: 28, height: 28 }}
                onClick={() => navigate(`/configurationuploads/${record.id}`)}
            />
        </Tooltip>
    );

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <ExtendedAntDTable<UnitSummary>
                data={units}
                tableColumns={allColumnDefs}
                title="Dashboard"
                customActions={actions}
                loading={loading}
            />
        </div>
    );
};

export default Dashboard;