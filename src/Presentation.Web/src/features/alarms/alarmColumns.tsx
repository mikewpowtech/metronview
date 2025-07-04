import type { ColumnsType } from "antd/es/table";
import { Tag, Tooltip } from "antd";
import type { Alarm } from "./alarmAPI";
import type { Company } from "../companies/companyAPI";
import type { RecipientSet } from "../recipientSets/recipientSetAPI";

export function getColumns(
    companies: Company[] = [],
    recipientSets: RecipientSet[] = []
): ColumnsType<Alarm> {
    return [
        {
            title: "ID",
            dataIndex: "id",
            key: "id",
            width: 80,
            sorter: (a, b) => a.id - b.id,
        },
        {
            title: "Name",
            dataIndex: "name",
            key: "name",
            width: 200,
            ellipsis: {
                showTitle: false,
            },
            render: (name) => (
                <Tooltip placement="topLeft" title={name}>
                    {name || "-"}
                </Tooltip>
            ),
            sorter: (a, b) => a.name.localeCompare(b.name),
        },
        {
            title: "Company",
            dataIndex: "companyId",
            key: "companyId",
            width: 200,
            render: (companyId) => {
                const company = companies.find(c => c.id === companyId);
                return company ? company.name : `Company ${companyId}`;
            },
            sorter: (a, b) => a.companyId - b.companyId,
        },
        {
            title: "Recipient Set",
            dataIndex: "recipientSetId",
            key: "recipientSetId",
            width: 200,
            render: (recipientSetId) => {
                const recipientSet = recipientSets.find(rs => rs.id === recipientSetId);
                return (
                    <Tag color="blue">
                        {recipientSet ? recipientSet.name : `Set ${recipientSetId}`}
                    </Tag>
                );
            },
            sorter: (a, b) => a.recipientSetId - b.recipientSetId,
        },
        {
            title: "Status",
            dataIndex: "isActive",
            key: "isActive",
            width: 100,
            render: (isActive) => (
                <Tag color={isActive ? "success" : "error"}>
                    {isActive ? "Active" : "Inactive"}
                </Tag>
            ),
            filters: [
                { text: "Active", value: true },
                { text: "Inactive", value: false },
            ],
            onFilter: (value, record) => record.isActive === value,
        },
    ];
}
