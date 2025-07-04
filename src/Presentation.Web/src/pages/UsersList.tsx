/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useEffect, useState } from "react";
import "react-resizable/css/styles.css";
import { usersApi } from "../features/user/authAPI";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getUserColumns, type ApplicationUser } from "../features/user/userColumns";
import type { ColumnType } from "antd/es/table";

const UsersList: React.FC = () => {

    const [users, setUsers] = useState<ApplicationUser[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [editingKey, setEditingKey] = useState<string | null>(null);
    const [editingUserName, setEditingUserName] = useState<string>("");

    useEffect(() => {
        setLoading(true);
        usersApi()
            .then((d) => {
                setUsers(d?.data || []);
                setLoading(false);
            })
            .catch((e) => {
                console.log(e);
                setLoading(false);
            });
    }, []);

    const handleEdit = (record: ApplicationUser) => {
        setEditingKey(record.id);
        setEditingUserName(record.userName);
    };

    const saveUserName = (id: string) => {
        setUsers(prev =>
            prev.map(user =>
                user.id === id ? { ...user, userName: editingUserName } : user
            )
        );
        setEditingKey(null);
        setEditingUserName("");
        // Optionally, call an API to persist the change here
    };

        //inline edit
    const columnMapper = (col: any): ColumnType<ApplicationUser> => {
        return col.key === "userName"
            ? {
                ...col,
                render: (_: any, record: ApplicationUser) => {
                    if (editingKey === record.id) {
                        return (
                            <input
                                value={editingUserName}
                                onChange={e => setEditingUserName(e.target.value)}
                                onBlur={() => saveUserName(record.id)}
                                onKeyDown={e => {
                                    if (e.key === "Enter") saveUserName(record.id);
                                }}
                                autoFocus
                                style={{ minWidth: 80 }}
                            />
                        );
                    }
                    return (
                        <span
                            style={{ cursor: "pointer" }}
                            onClick={() => handleEdit(record)}
                        >
                            {record.userName}
                        </span>
                    );
                },
            }
            : col
    }

    const userColumnDefinitions = getUserColumns({ columnMapper });

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <ExtendedAntDTable<ApplicationUser>
                data={users}
                tableColumns={userColumnDefinitions}
                title="Users List"
                loading={loading}
            />
        </div>
    );
};

export default UsersList;